using System.Collections.Generic;
using RobbieWagnerGames.Managers;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;
using System;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
    public partial class RunMap : MonoBehaviour
    {
        [SerializeField] private GameObject selectionIndicator;
        [SerializeField] private float selectionIndicatorOffset = .1f;
        [SerializeField] private float navigationDeadzone = 0.1f;
        private float mousePointerPos = 0;
        [SerializeField] private float controllerNavigationDelay = 0.2f;
        [SerializeField] private float controllerRepeatRate = 0.1f;
        
        [SerializeField] private bool maintainSelectionAlways = true;
        [SerializeField] private float selectionCheckInterval = 0.5f;
        [SerializeField] private bool restoreSelectionOnWindowFocus = true;

        private LevelButton selectedButton;
        private List<LevelButton> pastButtons = new List<LevelButton>(), presentButtons = new List<LevelButton>(), futureButtons = new List<LevelButton>();
        private Vector2 lastControllerInput = Vector2.zero;
        private float lastControllerNavigationTime = 0f;
        private bool canNavigateWithController = false;
        private static bool isUsingController = false;

        private GameObject forcedSelectionObject;
        private Coroutine selectionMaintenanceCoroutine;

        public bool IsActive => canvasGroup.interactable && navigateAction != null;

        private InputAction navigateAction, submitAction, pointAction, clickAction, cancelAction;

        protected void Awake()
        {
            RunManager.Instance.OnRunContinued += DisplayRunUI;
            RunManager.Instance.OnStartLevel += HideRunUI;

            selectionIndicator.SetActive(false);

            Application.focusChanged += OnApplicationFocusChanged;
        }

        protected void OnEnable()
        {
            InputManager.Instance.onActionMapsUpdated += OnActionMapsUpdated;
        }

        protected void OnDisable()
        {
            InputManager.Instance.onActionMapsUpdated -= OnActionMapsUpdated;
            StopSelectionMaintenance();
        }

        protected void OnDestroy()
        {
            Application.focusChanged -= OnApplicationFocusChanged;
            StopSelectionMaintenance();
        }

        private void Update()
        {
            if (!IsActive) return;
            
            HandleInputModeDetection();
            HandleControllerNavigation();
            CheckForLostSelection();
        }

        private void HandleInputModeDetection()
        {
            bool controllerInputThisFrame = false;
            
            if (navigateAction.ReadValue<Vector2>().magnitude > navigationDeadzone)
                controllerInputThisFrame = true;
            
            if (submitAction.WasPressedThisFrame())
                controllerInputThisFrame = true;
            
            if (cancelAction.WasPressedThisFrame())
                controllerInputThisFrame = true;
            
            bool mouseInputThisFrame = false;
            if (pointAction != null)
            {
                float pointerMagnitude = pointAction.ReadValue<Vector2>().magnitude;
                float mouseDelta = Mathf.Abs(pointerMagnitude - mousePointerPos);
                if (mouseDelta > .1f)
                    mouseInputThisFrame = true;
                mousePointerPos = pointerMagnitude;
            }
            
            if (clickAction != null && clickAction.WasPressedThisFrame())
                mouseInputThisFrame = true;
            
            if (controllerInputThisFrame)
            {
                if (!isUsingController)
                {
                    isUsingController = true;
                    ForceControllerMode();
                }
            }
            else if (mouseInputThisFrame)
            {
                if (isUsingController)
                {
                    isUsingController = false;
                    ClearSelection();
                }
            }
            
            UpdateSelectionIndicatorVisibility();
        }

        private void ForceControllerMode()
        {
            if (selectedButton == null || !selectedButton.button.interactable)
                RestoreOrSetDefaultSelection();
            
            UpdateSelectionIndicatorVisibility();
            canNavigateWithController = true;
        }

        private void CheckForLostSelection()
        {
            if (!IsActive || !maintainSelectionAlways || !isUsingController) return;
            
            if (selectedButton == null || !selectedButton.button.interactable)
                RestoreOrSetDefaultSelection();
        }

        private void HandleControllerNavigation()
        {
            if (!isUsingController || !canNavigateWithController || presentButtons.Count == 0) return;

            if (navigateAction != null)
            {
                Vector2 input = navigateAction.ReadValue<Vector2>();
                
                if (input != Vector2.zero && (lastControllerInput == Vector2.zero || Time.time - lastControllerNavigationTime > controllerRepeatRate))
                {
                    HandleNavigationInput(input);
                    lastControllerNavigationTime = Time.time;
                }
                else if (input == Vector2.zero && lastControllerInput != Vector2.zero)
                    canNavigateWithController = true;
                
                lastControllerInput = input;
                
                if (submitAction != null && submitAction.WasPressedThisFrame() && selectedButton != null && selectedButton.button.interactable)
                    selectedButton.button.onClick?.Invoke();
                
                if (cancelAction != null && cancelAction.WasPressedThisFrame())
                    HideRunUI(null);
            }
        }

        private void HandleNavigationInput(Vector2 input)
        {
            if (selectedButton == null || presentButtons.Count == 0) return;

            bool inputHandled = false;
            
            if (Mathf.Abs(input.y) > Mathf.Abs(input.x))
            {
                if (input.y > 0.5f)
                    NavigateVertical(-1);
                else if (input.y < -0.5f)
                    NavigateVertical(1);
                inputHandled = true;
            }
            else
            {
                if (input.x < -0.5f)
                    NavigateHorizontal(-1);
                else if (input.x > 0.5f)
                    NavigateHorizontal(1);
                inputHandled = true;
            }
            
            if (inputHandled)
            {
                canNavigateWithController = false;
                Invoke(nameof(ResetControllerNavigation), controllerNavigationDelay);
            }
        }

        private void NavigateVertical(int direction)
        {
            if (presentButtons.Count == 0) return;
            
            int currentIndex = presentButtons.IndexOf(selectedButton);
            if (currentIndex < 0) return;
            
            Vector3 currentPos = selectedButton.transform.localPosition;
            LevelButton bestMatch = null;
            float bestScore = float.MaxValue;
            float minVerticalDistance = .1f;
            
            foreach (LevelButton button in presentButtons)
            {
                if (button == selectedButton) continue;
                
                Vector3 directionVector = button.transform.localPosition - currentPos;
                
                if ((direction < 0 && directionVector.y > minVerticalDistance) || (direction > 0 && directionVector.y < -minVerticalDistance)) 
                {
                    float verticalAlignment = Mathf.Abs(directionVector.y);
                    float horizontalProximity = 1f / (Mathf.Abs(directionVector.x) + 1f);
                    float score = verticalAlignment * 0.7f + horizontalProximity * 0.3f;
                    
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestMatch = button;
                    }
                }
            }
            
            if (bestMatch != null)
                SelectButton(bestMatch);
        }
        
        private void NavigateHorizontal(int direction)
        {
            if (selectedButton == null || presentButtons.Count == 0 || Math.Abs(direction) < .1f) return;
            
            Vector3 currentPos = selectedButton.transform.localPosition;
            float yTolerance = 1f;
            
            LevelButton bestMatch = null;
            float closestDistance = float.MaxValue;
            foreach (LevelButton button in presentButtons)
            {
                if (button == selectedButton) continue;
                
                if (Mathf.Abs(button.transform.localPosition.y - currentPos.y) < yTolerance)
                {
                    Vector3 directionVector = button.transform.localPosition - currentPos;
                    if ((direction < 0 && directionVector.x < -0.1f) || (direction > 0 && directionVector.x > 0.1f))
                    {
                        float horizontalDistance = Mathf.Abs(directionVector.x);
                        
                        if (horizontalDistance < closestDistance)
                        {
                            closestDistance = horizontalDistance;
                            bestMatch = button;
                        }
                    }
                }
            }
            
            if (bestMatch != null)
                SelectButton(bestMatch);
        }

        private void ResetControllerNavigation()
        {
            canNavigateWithController = true;
        }

        private void SelectButton(LevelButton button)
        {
            if (button == null || !button.button.interactable) return;
            
            if (selectedButton != null && selectedButton != button)
                UpdateButtonAppearance(selectedButton);
            
            selectedButton = button;
            forcedSelectionObject = button.gameObject;
            
            if (selectedButton.button.image != null)
            {
                Color originalColor = GameConstants.Instance.levelColors[selectedButton.level.levelType];
                selectedButton.button.image.color = new Color(
                    Mathf.Min(originalColor.r * 1.3f, 1f),
                    Mathf.Min(originalColor.g * 1.3f, 1f),
                    Mathf.Min(originalColor.b * 1.3f, 1f),
                    originalColor.a
                );
            }
            
            UpdateSelectionIndicator();
        }

        private void ClearSelection()
        {
            if (selectedButton != null && selectedButton.button.image != null)
                UpdateButtonAppearance(selectedButton);
            
            selectedButton = null;
            forcedSelectionObject = null;
            
            UpdateSelectionIndicatorVisibility();
        }

        private void UpdateSelectionIndicator()
        {
            if (selectedButton == null) return;
            
            if (isUsingController)
            {
                selectionIndicator.SetActive(true);
                
                RectTransform buttonRect = selectedButton.GetComponent<RectTransform>();
                RectTransform indicatorRect = selectionIndicator.GetComponent<RectTransform>();
                
                indicatorRect.SetParent(buttonRect.parent);
                indicatorRect.SetAsLastSibling();
                
                Vector3[] buttonCorners = new Vector3[4];
                buttonRect.GetWorldCorners(buttonCorners);
                
                Vector3 leftCenter = new Vector3(
                    buttonCorners[0].x,
                    (buttonCorners[0].y + buttonCorners[1].y) * 0.5f,
                    buttonCorners[0].z
                );
                
                Vector3 localPos = indicatorRect.parent.InverseTransformPoint(leftCenter);
                float indicatorWidth = indicatorRect.rect.width * indicatorRect.localScale.x;
                localPos.x -= indicatorWidth * 0.5f + selectionIndicatorOffset;
                
                indicatorRect.localPosition = localPos;
                indicatorRect.localScale = Vector3.one;
                indicatorRect.SetAsLastSibling();
            }
        }

        private void UpdateSelectionIndicatorVisibility()
        {
            bool shouldBeActive = isUsingController && selectedButton != null && IsActive;
            
            if (shouldBeActive && !selectionIndicator.activeSelf)
            {
                selectionIndicator.SetActive(true);
                UpdateSelectionIndicator();
            }
            else if (!shouldBeActive && selectionIndicator.activeSelf)
                selectionIndicator.SetActive(false);
        }

        private void RestoreOrSetDefaultSelection()
        {
            if (!IsActive || presentButtons.Count == 0) return;
            
            GameObject selectionToRestore = null;
            
            if (forcedSelectionObject != null && forcedSelectionObject.activeInHierarchy && forcedSelectionObject.GetComponent<Button>()?.interactable == true)
                selectionToRestore = forcedSelectionObject;
            else
            {
                foreach (LevelButton button in presentButtons)
                {
                    if (button.button.interactable && button.gameObject.activeInHierarchy)
                    {
                        selectionToRestore = button.gameObject;
                        break;
                    }
                }
            }
            
            if (selectionToRestore != null)
            {
                LevelButton button = selectionToRestore.GetComponent<LevelButton>();
                if (button != null)
                    SelectButton(button);
            }
        }

        private void StartSelectionMaintenance()
        {
            InputManager.Instance.EnableActionMap(ActionMapName.UI);
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
            yield return new WaitForSeconds(.2f);
            navigateAction = InputManager.Instance.GetAction(ActionMapName.UI, "Navigate");
            submitAction = InputManager.Instance.GetAction(ActionMapName.UI, "Submit");
            pointAction = InputManager.Instance.GetAction(ActionMapName.UI, "Point");
            clickAction = InputManager.Instance.GetAction(ActionMapName.UI, "Click");
            cancelAction = InputManager.Instance.GetAction(ActionMapName.UI, "Cancel");
            while (IsActive)
            {
                yield return new WaitForSecondsRealtime(selectionCheckInterval);
                
                if (isUsingController && maintainSelectionAlways)
                {
                    if (selectedButton == null || !selectedButton.button.interactable)
                        RestoreOrSetDefaultSelection();
                }
            }
        }

        private void OnApplicationFocusChanged(bool hasFocus)
        {
            if (!IsActive) return;
            
            if (hasFocus && restoreSelectionOnWindowFocus)
                StartCoroutine(RestoreSelectionAfterFocus());
            else if (!hasFocus)
                forcedSelectionObject = null;
        }
        
        private IEnumerator RestoreSelectionAfterFocus()
        {
            yield return new WaitForSecondsRealtime(.01f);
            
            RestoreOrSetDefaultSelection();
        }
        
        protected virtual void OnActionMapsUpdated(List<ActionMapName> activeMaps)
        {
            if (!activeMaps.Contains(ActionMapName.UI))
            {
                ClearSelection();
                forcedSelectionObject = null;
            }
        }
    }
}