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
        protected bool canNavigateWithController = true;
        protected bool isUsingController = false;
        
        protected List<ActionMapName> previousActiveMaps = new List<ActionMapName>();
        
        protected Coroutine selectionMaintenanceCoroutine;
        protected GameObject forcedSelectionObject;
        
        public bool IsOpen => canvas != null && canvas.enabled;

        [SerializeField] protected Image selectionIcon;
        [SerializeField] protected float selectionIconOffset = 15f;
        
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
            
            RefreshSelectableElements();
            
            if (IsOpen)
                SetupNavigation();
        }
        
        protected virtual void OnDisable()
        {
            InputManager.Instance.onActionMapsUpdated -= OnActionMapsUpdated;
            
            StopSelectionMaintenance();
        }
        
        protected virtual void OnDestroy()
        {
            Application.focusChanged -= OnApplicationFocusChanged;
            StopSelectionMaintenance();
        }
        
        protected virtual void Update()
        {
            if (!IsOpen) return;
            
            HandleInputModeDetection();
            HandleControllerNavigation();
            HandleMouseNavigation();
            CheckForLostSelection();
        }
        
        private void HandleInputModeDetection()
        {
            bool controllerInputThisFrame = false;
            
            var navigateAction = InputManager.Instance.GetAction(ActionMapName.UI, "Navigate");
            if (navigateAction != null && navigateAction.ReadValue<Vector2>().magnitude > 0.1f)
                controllerInputThisFrame = true;
            
            var submitAction = InputManager.Instance.GetAction(ActionMapName.UI, "Submit");
            if (submitAction != null && submitAction.WasPressedThisFrame())
                controllerInputThisFrame = true;
            
            bool mouseInputThisFrame = false;
            var pointAction = InputManager.Instance.GetAction(ActionMapName.UI, "Point");
            if (pointAction != null)
            {
                Vector2 mouseDelta = pointAction.ReadValue<Vector2>();
                if (mouseDelta.magnitude > 0.1f)
                {
                    mouseInputThisFrame = true;
                    lastMouseActivityTime = Time.time;
                }
            }
            
            if (controllerInputThisFrame)
            {
                isUsingController = true;
                ForceControllerMode();
            }
            else if (mouseInputThisFrame || (Time.time - lastMouseActivityTime < mouseInactivityTimeout && !isUsingController))
                isUsingController = false;
            
            if (isUsingController && !controllerInputThisFrame && Time.time - lastMouseActivityTime < mouseInactivityTimeout)
                isUsingController = false;
        }
        
        private void ForceControllerMode()
        {
            if (!EventSystemManager.Instance.HasSelection())
                RestoreOrSetDefaultSelection();
        }
        
        private void CheckForLostSelection()
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
            
            isUsingController = false;
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
                foreach (var map in previousActiveMaps)
                    InputManager.Instance.EnableActionMap(map, false);
                previousActiveMaps.Clear();
            }
            
            forcedSelectionObject = null;
            
            EventSystemManager.Instance.SetSelected(null);
            
            OnClosed();
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
            
            isUsingController = true;
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
        
        private void StartSelectionMaintenance()
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
        
        private void RestoreOrSetDefaultSelection()
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
                foreach (var element in selectableElements)
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

            yield return new WaitForSecondsRealtime(.01f);
            
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
            
            var currentElement = selectableElements[currentSelectedIndex];
            
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
            
            var currentElement = selectableElements[currentSelectedIndex];
            
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
            Close();
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
            // Place the selection icon to the left of the selected obj if using controller
            if(element != null && selectionIcon != null && isUsingController)
            {
                selectionIcon.gameObject.SetActive(true);
                
                RectTransform elementRect = element.GetComponent<RectTransform>();
                Vector3 elementPosition = elementRect.position;
                RectTransform iconRect = selectionIcon.GetComponent<RectTransform>();
                
                iconRect.SetParent(elementRect.parent);
                    iconRect.SetAsLastSibling();
                
                Vector3[] elementCorners = new Vector3[4];
                elementRect.GetWorldCorners(elementCorners);

                Vector3 leftCenter = new Vector3(
                    elementCorners[0].x, // left x
                    (elementCorners[0].y + elementCorners[1].y) * 0.5f, // center y between bottom-left and top-left
                    elementCorners[0].z
                );

                Vector3 localPos = iconRect.parent.InverseTransformPoint(leftCenter);
                float iconWidth = iconRect.rect.width * iconRect.localScale.x;
                localPos.x -= iconWidth * 0.5f + selectionIconOffset;
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