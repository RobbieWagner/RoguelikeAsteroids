using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using RobbieWagnerGames.Managers;
using System.Collections;
using UnityEngine.InputSystem;

namespace RobbieWagnerGames.UI
{
    [RequireComponent(typeof(Canvas))]
    public class Menu : MonoBehaviour
    {
        protected static Menu _activeMenu = null;
        public static Menu activeMenu
        {
            get => _activeMenu;
            set
            {
                if (_activeMenu == value) return;
                
                if (_activeMenu != null)
                {
                    _activeMenu.Close();
                    _activeMenu.StopInputHandling();
                }
                
                _activeMenu = value;
                
                if (_activeMenu != null)
                {
                    _activeMenu.StartCoroutine(_activeMenu.RefreshInputHandling());
                    _activeMenu.Open();
                    _activeMenu.StartInputHandling();
                }
            }
        }

        [Header("Menu Configuration")]
        [SerializeField] protected bool enableOnStart = true;
        [SerializeField] protected bool disableGameControlsWhenOpen = true;
        [SerializeField] protected bool restorePreviousActionMapsOnClose = true;
        
        [Header("Controller Navigation")]
        [SerializeField] protected Selectable firstSelected;
        [SerializeField] protected bool wrapNavigation = true;
        [SerializeField] protected float controllerNavigationDelay = 0.2f;
        [SerializeField] protected float controllerRepeatRate = 0.1f;
        [SerializeField] protected float mouseInactivityTimeout = 2f;
        
        [Header("Selection Persistence")]
        [SerializeField] protected bool maintainSelectionAlways = true;
        [SerializeField] protected float selectionCheckInterval = 0.5f;
        [SerializeField] protected bool restoreSelectionOnWindowFocus = true;
        
        protected Canvas canvas;
        protected List<Selectable> selectableElements = new List<Selectable>();
        protected int currentSelectedIndex = 0;
        
        protected Vector2 lastControllerInput = Vector2.zero;
        protected float lastControllerNavigationTime = 0f;
        protected float lastMouseActivityTime = 0f;
        protected float lastFrameMousePos;
        protected bool canNavigateWithController = true;
        protected static bool isUsingController = false;
        protected bool ignoreNextSubmit = false;
        
        protected List<ActionMapName> previousActiveMaps = new List<ActionMapName>();
        
        protected Coroutine selectionMaintenanceCoroutine;
        protected GameObject forcedSelectionObject;
        
        public virtual bool IsOpen => canvas != null && canvas.enabled;

        [SerializeField] protected Image selectionIcon;
        [SerializeField] protected float selectionIconOffset = 15f;
        [SerializeField] protected bool placeIconOnLeft = true;
        
        protected virtual void Awake()
        {
            canvas = GetComponent<Canvas>();
            
            if (!enableOnStart)
                Close();
            
            RefreshSelectableElements();
            
            Application.focusChanged += OnApplicationFocusChanged;
        }
        
        protected virtual void OnEnable()
        {
            InputManager.Instance.onActionMapsUpdated += OnActionMapsUpdated;
            
            InputAction submitAction = InputManager.Instance.GetAction(ActionMapName.UI, "Submit");
            InputAction cancelAction = InputManager.Instance.GetAction(ActionMapName.UI, "Cancel");
            InputAction clickAction = InputManager.Instance.GetAction(ActionMapName.UI, "Click");
            
            if (submitAction != null)
                submitAction.performed += OnSubmitPerformed;
            if (cancelAction != null)
                cancelAction.performed += OnCancelPerformed;
            if (clickAction != null)
                clickAction.performed += OnClickPerformed;
            
            RefreshSelectableElements();
            
            if (IsOpen)
                SetupNavigation();
        }
        
        protected virtual void OnDisable()
        {
            InputManager.Instance.onActionMapsUpdated -= OnActionMapsUpdated;
            
            InputAction submitAction = InputManager.Instance.GetAction(ActionMapName.UI, "Submit");
            InputAction cancelAction = InputManager.Instance.GetAction(ActionMapName.UI, "Cancel");
            InputAction clickAction = InputManager.Instance.GetAction(ActionMapName.UI, "Click");
            
            if (submitAction != null)
                submitAction.performed -= OnSubmitPerformed;
            if (cancelAction != null)
                cancelAction.performed -= OnCancelPerformed;
            if (clickAction != null)
                clickAction.performed -= OnClickPerformed;
            
            StopSelectionMaintenance();
        }
        
        protected virtual void OnDestroy()
        {
            Application.focusChanged -= OnApplicationFocusChanged;
            StopSelectionMaintenance();
        }
        
        protected virtual void Update()
        {
            if (!IsOpen || activeMenu != this || !InputManager.Instance.Controls.UI.enabled) return;
            
            HandleInputModeDetection();
            HandleControllerNavigation();
            HandleMouseNavigation();
            CheckForLostSelection();
            UpdateSelectionIconVisibility();
        }

        private void OnSubmitPerformed(InputAction.CallbackContext context)
        {
            if (!IsOpen || activeMenu != this || !isUsingController) return;
            
            if (ignoreNextSubmit)
            {
                ignoreNextSubmit = false;
                return;
            }
            
            float value = context.ReadValue<float>();
            if (value > 0.5f)
                HandleSubmit();
        }

        private void OnCancelPerformed(InputAction.CallbackContext context)
        {
            if (!IsOpen || activeMenu != this || !isUsingController) return;
            
            float value = context.ReadValue<float>();
            if (value > 0.5f)
                HandleCancel();
        }

        private void OnClickPerformed(InputAction.CallbackContext context)
        {
            if (!IsOpen || activeMenu != this || isUsingController) return;
            
            float value = context.ReadValue<float>();
            if (value > 0.5f)
                HandleMouseClick();
        }
        
        protected virtual void HandleInputModeDetection()
        {
            bool controllerInputThisFrame = false;
            
            InputAction navigateAction = InputManager.Instance.GetAction(ActionMapName.UI, "Navigate");
            if (navigateAction != null && navigateAction.ReadValue<Vector2>().magnitude > 0.1f)
                controllerInputThisFrame = true;
            
            InputAction submitAction = InputManager.Instance.GetAction(ActionMapName.UI, "Submit");
            if (submitAction != null && submitAction.WasPressedThisFrame())
                controllerInputThisFrame = true;
            
            bool mouseInputThisFrame = false;
            InputAction pointAction = InputManager.Instance.GetAction(ActionMapName.UI, "Point");
            if (pointAction != null)
            {
                Vector2 mousePos = pointAction.ReadValue<Vector2>();
                float mouseDelta = Mathf.Abs(lastFrameMousePos - mousePos.magnitude);
                if (mouseDelta > 10f)
                {
                    mouseInputThisFrame = true;
                    lastMouseActivityTime = Time.time;
                }
                lastFrameMousePos = mousePos.magnitude;
            }
            
            if (controllerInputThisFrame)
            {
                isUsingController = true;
                ForceControllerMode();
            }
            else if (mouseInputThisFrame)
                isUsingController = false;
        }

        private void UpdateSelectionIconVisibility()
        {
            if (selectionIcon != null)
            {
                bool shouldShowIcon = isUsingController;
                selectionIcon.gameObject.SetActive(shouldShowIcon);
                
                if (shouldShowIcon)
                {
                    GameObject selected = EventSystemManager.Instance.CurrentSelected;
                    if (selected != null)
                    {
                        Selectable selectable = selected.GetComponent<Selectable>();
                        if (selectable != null)
                            UpdateSelectionIcon(selectable);
                    }
                }
            }
        }

        protected virtual void ForceControllerMode()
        {
            if (!EventSystemManager.Instance.HasSelection())
                RestoreOrSetDefaultSelection();
        }
        
        protected virtual void CheckForLostSelection()
        {
            if (!IsOpen || !maintainSelectionAlways || !isUsingController) return;
            
            if (!EventSystemManager.Instance.HasSelection())
                RestoreOrSetDefaultSelection();
        }
        
        protected virtual void HandleControllerNavigation()
        {
            if (!isUsingController || !canNavigateWithController || selectableElements.Count == 0) return;
            
            InputAction navigateAction = InputManager.Instance.GetAction(ActionMapName.UI, "Navigate");
            if (navigateAction != null)
            {
                Vector2 input = navigateAction.ReadValue<Vector2>();
                
                if (input != Vector2.zero && (lastControllerInput == Vector2.zero || 
                    Time.time - lastControllerNavigationTime > controllerRepeatRate))
                {
                    HandleNavigationInput(input);
                    lastControllerNavigationTime = Time.time;
                }
                else if (input == Vector2.zero && lastControllerInput != Vector2.zero)
                    canNavigateWithController = true;
                
                lastControllerInput = input;
                
                InputAction submitAction = InputManager.Instance.GetAction(ActionMapName.UI, "Submit");
                if (submitAction != null && submitAction.WasPressedThisFrame())
                    HandleSubmit();
                
                InputAction cancelAction = InputManager.Instance.GetAction(ActionMapName.UI, "Cancel");
                if (cancelAction != null && cancelAction.WasPressedThisFrame())
                    HandleCancel();
            }
        }
        
        protected virtual void HandleMouseNavigation()
        {
            if (isUsingController) return;

            InputAction clickAction = InputManager.Instance.GetAction(ActionMapName.UI, "Click");
            
            if (clickAction != null && clickAction.WasPressedThisFrame())
                HandleMouseClick();
        }
        
        public virtual void Open()
        {
            if (disableGameControlsWhenOpen)
            {
                previousActiveMaps.Clear();
                previousActiveMaps.AddRange(InputManager.Instance.CurrentActiveMaps);
            }

            InputManager.Instance.EnableActionMap(ActionMapName.UI);
            
            canvas.enabled = true;

            lastMouseActivityTime = Time.time - mouseInactivityTimeout;
            
            SetupNavigation();
            StartSelectionMaintenance();

            OnOpened();
        }

        public virtual void Close()
        {
            if (canvas == null) 
                canvas = GetComponent<Canvas>();
            canvas.enabled = false;
            
            StopSelectionMaintenance();
            
            if (restorePreviousActionMapsOnClose && disableGameControlsWhenOpen)
            {
                InputManager.Instance.DisableAllActionMaps();
                foreach (ActionMapName map in previousActiveMaps)
                    InputManager.Instance.EnableActionMap(map, false);
                previousActiveMaps.Clear();
            }
            
            forcedSelectionObject = null;
            
            EventSystemManager.Instance.SetSelected(null);
            
            OnClosed();
        }

        protected void StartInputHandling()
        {
            if (isUsingController)
                SetupNavigation();
        }

        protected void StopInputHandling()
        {
            EventSystemManager.Instance.ClearSelection();
            forcedSelectionObject = null;
        }
        
        public virtual void Toggle()
        {
            if (IsOpen)
                Close();
            else
                Open();
        }
        
        public virtual void RefreshSelectableElements()
        {
            selectableElements.Clear();
            
            Selectable[] selectables = GetComponentsInChildren<Selectable>(true);
            foreach (Selectable selectable in selectables)
            {
                if (selectable.interactable && selectable.gameObject.activeInHierarchy)
                    selectableElements.Add(selectable);
            }
            
            selectableElements.Sort((a, b) =>
            {
                Vector3 aPos = a.transform.position;
                Vector3 bPos = b.transform.position;
                return bPos.y.CompareTo(aPos.y);
            });
        }
        
        protected virtual void SetupNavigation()
        {
            RefreshSelectableElements();
            
            lastMouseActivityTime = Time.time - mouseInactivityTimeout;
            
            Selectable elementToSelect = firstSelected;
            
            if (elementToSelect == null || !elementToSelect.interactable || !elementToSelect.gameObject.activeInHierarchy)
            {
                foreach (Selectable element in selectableElements)
                {
                    if (element.interactable && element.gameObject.activeInHierarchy)
                    {
                        elementToSelect = element;
                        break;
                    }
                }
            }
            
            if (elementToSelect != null)
            {
                ForceSelectElement(elementToSelect);
                currentSelectedIndex = selectableElements.IndexOf(elementToSelect);
            }
        }
        
        protected void StartSelectionMaintenance()
        {
            StopSelectionMaintenance();
            selectionMaintenanceCoroutine = StartCoroutine(SelectionMaintenanceRoutine());
        }
        
        private void StopSelectionMaintenance()
        {
            if (selectionMaintenanceCoroutine != null)
            {
                StopCoroutine(selectionMaintenanceCoroutine);
                selectionMaintenanceCoroutine = null;
            }
        }
        
        private IEnumerator SelectionMaintenanceRoutine()
        {
            while (IsOpen)
            {
                yield return new WaitForSecondsRealtime(selectionCheckInterval);
                
                if (isUsingController && maintainSelectionAlways)
                {
                    if (!EventSystemManager.Instance.HasSelection())
                        RestoreOrSetDefaultSelection();
                }
            }
        }
        
        protected void RestoreOrSetDefaultSelection()
        {
            if (!IsOpen || selectableElements.Count == 0) return;
            
            GameObject selectionToRestore = null;
            
            if (forcedSelectionObject != null && 
                forcedSelectionObject.activeInHierarchy && 
                forcedSelectionObject.GetComponent<Selectable>()?.interactable == true)
                selectionToRestore = forcedSelectionObject;
            else if (currentSelectedIndex >= 0 && currentSelectedIndex < selectableElements.Count &&
                    selectableElements[currentSelectedIndex].interactable &&
                    selectableElements[currentSelectedIndex].gameObject.activeInHierarchy)
                selectionToRestore = selectableElements[currentSelectedIndex].gameObject;
            else
            {
                foreach (Selectable element in selectableElements)
                {
                    if (element.interactable && element.gameObject.activeInHierarchy)
                    {
                        selectionToRestore = element.gameObject;
                        currentSelectedIndex = selectableElements.IndexOf(element);
                        break;
                    }
                }
            }
            
            if (selectionToRestore != null)
                ForceSelectElement(selectionToRestore.GetComponent<Selectable>());
        }
        
        protected void ForceSelectElement(Selectable element)
        {
            if (element == null) return;
            StartCoroutine(ForceSelectElementCoroutine(element));
        }
        
        private IEnumerator ForceSelectElementCoroutine(Selectable element)
        {
            if (element == EventSystemManager.Instance.CurrentSelected)
                yield break;

            yield return new WaitForSecondsRealtime(.1f);
            
            EventSystemManager.Instance.SetSelected(element.gameObject);
            forcedSelectionObject = element.gameObject;
            
            currentSelectedIndex = selectableElements.IndexOf(element);
            OnElementSelected(element);
        }
        
        protected virtual void HandleNavigationInput(Vector2 input)
        {
            if (selectableElements.Count == 0) return;
            
            bool inputHandled = false;
            
            if (Mathf.Abs(input.y) > Mathf.Abs(input.x))
            {
                if (input.y > 0.5f)
                {
                    NavigateVertical(-1);
                    inputHandled = true;
                }
                else if (input.y < -0.5f)
                {
                    NavigateVertical(1);
                    inputHandled = true;
                }
            }
            else
            {
                if (input.x < -0.5f)
                {
                    NavigateHorizontal(-1);
                    inputHandled = true;
                }
                else if (input.x > 0.5f)
                {
                    NavigateHorizontal(1);
                    inputHandled = true;
                }
            }
            
            if (inputHandled)
            {
                canNavigateWithController = false;
                Invoke(nameof(ResetControllerNavigation), controllerNavigationDelay);
            }
        }
        
        protected virtual void NavigateVertical(int direction)
        {
            // If this is a SettingsMenu, skip base navigation logic
            if (this is SettingsMenu) // Placeholder logic to prevent call in settings menu due to bug
                return;

            if (selectableElements.Count == 0) return;

            int newIndex = currentSelectedIndex;
            int attempts = 0;

            do
            {
                newIndex += direction;

                if (wrapNavigation)
                {
                    if (newIndex < 0) newIndex = selectableElements.Count - 1;
                    else if (newIndex >= selectableElements.Count) newIndex = 0;
                }
                else
                    newIndex = Mathf.Clamp(newIndex, 0, selectableElements.Count - 1);

                attempts++;

                if (attempts > selectableElements.Count)
                {
                    Debug.LogWarning("Failed to find valid element to navigate to", this);
                    return;
                }

            } while (!selectableElements[newIndex].interactable || !selectableElements[newIndex].gameObject.activeInHierarchy);

            currentSelectedIndex = newIndex;
            ForceSelectElement(selectableElements[currentSelectedIndex]);
        }
        
        protected virtual void NavigateHorizontal(int direction)
        {
            if (currentSelectedIndex < 0 || currentSelectedIndex >= selectableElements.Count) return;
            
            Selectable currentElement = selectableElements[currentSelectedIndex];
            
            if (currentElement is Slider slider)
            {
                float step = slider.maxValue - slider.minValue;
                if (slider.wholeNumbers) step = 1f;
                else step /= 20f;
                
                slider.value += direction * step;
                slider.onValueChanged?.Invoke(slider.value);
            }
            else if (currentElement is Dropdown dropdown)
            {
                int newValue = dropdown.value + direction;
                if (wrapNavigation)
                {
                    if (newValue < 0) newValue = dropdown.options.Count - 1;
                    else if (newValue >= dropdown.options.Count) newValue = 0;
                }
                else
                    newValue = Mathf.Clamp(newValue, 0, dropdown.options.Count - 1);
                
                dropdown.value = newValue;
                dropdown.onValueChanged?.Invoke(newValue);
            }
            else if (currentElement is Toggle toggle)
            {
                toggle.isOn = !toggle.isOn;
                toggle.onValueChanged?.Invoke(toggle.isOn);
            }
            else
            {
                Navigation navigation = currentElement.navigation;
                Selectable target = direction < 0 ? navigation.selectOnLeft : navigation.selectOnRight;
                
                if (target != null && target.interactable && target.gameObject.activeInHierarchy)
                {
                    int targetIndex = selectableElements.IndexOf(target);
                    if (targetIndex >= 0)
                    {
                        currentSelectedIndex = targetIndex;
                        ForceSelectElement(target);
                    }
                }
            }
        }
        
        protected virtual void SelectElement(Selectable element)
        {
            if (element == null) return;
            
            EventSystemManager.Instance.SetSelected(element.gameObject);
            
            OnElementSelected(element);
        }
        
        protected virtual void HandleSubmit()
        {
            if (currentSelectedIndex < 0 || currentSelectedIndex >= selectableElements.Count) return;
            
            Selectable currentElement = selectableElements[currentSelectedIndex];
            
            if (currentElement is Button button)
                button.onClick?.Invoke();
            else if (currentElement is Toggle toggle)
            {
                toggle.isOn = !toggle.isOn;
                toggle.onValueChanged?.Invoke(toggle.isOn);
            }
            
            OnElementSubmitted(currentElement);
        }
        
        protected virtual void HandleCancel()
        {
            OnCancelPressed();
        }
        
        protected virtual void HandleMouseClick()
        {
            OnMouseClicked();
        }
        
        protected void ResetControllerNavigation()
        {
            canNavigateWithController = true;
        }
   

        private IEnumerator RefreshInputHandling()
        {
            ignoreNextSubmit = true;
            
            InputManager.Instance.DisableActionMap(ActionMapName.UI);
            yield return new WaitForSecondsRealtime(.3f);
            InputManager.Instance.EnableActionMap(ActionMapName.UI);
        }
        
        private void OnApplicationFocusChanged(bool hasFocus)
        {
            if (!IsOpen) return;
            
            if (hasFocus && restoreSelectionOnWindowFocus)
                StartCoroutine(RestoreSelectionAfterFocus());
            else if (!hasFocus)
                forcedSelectionObject = null;
        }
        
        private IEnumerator RestoreSelectionAfterFocus()
        {
            yield return new WaitForSecondsRealtime(.01f);
            
            isUsingController = true;
            RestoreOrSetDefaultSelection();
        }
        
        protected virtual void OnActionMapsUpdated(List<ActionMapName> activeMaps)
        {
            if (!activeMaps.Contains(ActionMapName.UI))
            {
                EventSystemManager.Instance.SetSelected(null);
                forcedSelectionObject = null;
            }
        }
        
        protected virtual void OnOpened() { }
        protected virtual void OnClosed() { }
        protected virtual void OnElementSelected(Selectable element)
        {
            UpdateSelectionIcon(element);
        }

        private void UpdateSelectionIcon(Selectable element)
        {
            if (element != null && selectionIcon != null && isUsingController)
            {
                selectionIcon.gameObject.SetActive(true);

                RectTransform elementRect = element.GetComponent<RectTransform>();
                RectTransform iconRect = selectionIcon.GetComponent<RectTransform>();

                iconRect.SetParent(elementRect.parent);
                iconRect.SetAsLastSibling();

                Vector3[] elementCorners = new Vector3[4];
                elementRect.GetWorldCorners(elementCorners);

                Vector3 targetPosition;
                
                if (placeIconOnLeft)
                {
                    targetPosition = new Vector3(
                        elementCorners[0].x,
                        (elementCorners[0].y + elementCorners[1].y) * 0.5f, 
                        elementCorners[0].z
                    );
                }
                else
                {
                    targetPosition = new Vector3(
                        elementCorners[3].x,
                        (elementCorners[2].y + elementCorners[3].y) * 0.5f,
                        elementCorners[3].z
                    );
                }

                Vector3 localPos = iconRect.parent.InverseTransformPoint(targetPosition);
                float iconWidth = iconRect.rect.width * iconRect.localScale.x;
                
                if (placeIconOnLeft)
                    localPos.x -= iconWidth * 0.5f + selectionIconOffset;
                else
                    localPos.x += iconWidth * 0.5f + selectionIconOffset;
                
                iconRect.localPosition = localPos;
                iconRect.localScale = Vector3.one;
                selectionIcon.transform.SetAsLastSibling();
            }
        }

        protected virtual void OnElementSubmitted(Selectable element) { }
        protected virtual void OnCancelPressed() { }
        protected virtual void OnMouseClicked() { }
    }
}