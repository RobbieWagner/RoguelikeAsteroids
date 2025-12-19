using System.Collections;
using DG.Tweening;
using RobbieWagnerGames.Managers;
using RobbieWagnerGames.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RobbieWagnerGames.FirstPerson
{
    /// <summary>
    /// Handles first-person camera rotation with mouse and controller support
    /// </summary>
    public partial class FirstPersonLook : MonoBehaviourSingleton<FirstPersonLook>
    {
        [Header("Look Settings")]
        [SerializeField] private float mouseSensitivity = 500f;
        [SerializeField] private float controllerSensitivity = 0.5f;
        [SerializeField] private Transform playerBody;
        [SerializeField] private bool lockCursorOnStart = true;
        [SerializeField] private Camera playerCamera;

        [Header("View Constraints")]
        [SerializeField] private float minVerticalAngle = -90f;
        [SerializeField] private float maxVerticalAngle = 90f;

        private float xRotation = 0f;
        private Vector2 currentLookInput = Vector2.zero;
        private InputAction mouseLookAction;
        private InputAction controllerLookAction;
        private InputAction interactAction;
        private bool isUsingController = false;

        private bool _canLook = true;
        public bool canLook
        {
            get => _canLook;
            set
            {
                if (value == _canLook) return;
                
                _canLook = value;
                UpdateCursorState();
                onLookStateChanged?.Invoke(_canLook);
            }
        }

        public delegate void LookStateChanged(bool canLook);
        public event LookStateChanged onLookStateChanged;

        protected override void Awake()
        {
            base.Awake();
            InitializeCursor();
            SetupInput();
            SetupInteraction();
            OnCurrentInteractableChanged += HandleCurrentInteractableChanged;
            OnCurrentInteractableUnchanged += HandleCurrentInteractableUnchanged;
        }

        private void Update()
        {
            if (!canLook) return;

            ProcessLookInput();
            UpdateInteractionState();
        }

        private void InitializeCursor()
        {
            Cursor.lockState = lockCursorOnStart ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !lockCursorOnStart;
        }

        private void SetupInput()
        {
            mouseLookAction = InputManager.Instance.GetAction(ActionMapName.EXPLORATION , "MouseLook");
            mouseLookAction.performed += OnLookPerformed;
            mouseLookAction.canceled += OnLookCanceled;

            controllerLookAction = InputManager.Instance.GetAction(ActionMapName.EXPLORATION , "ControllerLook");
            controllerLookAction.performed += OnLookPerformed;
            controllerLookAction.canceled += OnLookCanceled;

            interactAction = InputManager.Instance.GetAction(ActionMapName.EXPLORATION, "Interact");
            interactAction.performed += OnInteractPerformed;
        }

        private void ProcessLookInput()
        {
            if (currentLookInput == Vector2.zero) return;

            float sensitivity = isUsingController ? controllerSensitivity : mouseSensitivity;
            Vector2 scaledInput = currentLookInput * sensitivity * Time.deltaTime;

            // Vertical rotation (up/down)
            xRotation -= scaledInput.y;
            xRotation = Mathf.Clamp(xRotation, minVerticalAngle, maxVerticalAngle);
            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

            // Horizontal rotation (left/right)
            playerBody.Rotate(Vector3.up * scaledInput.x);
        }

        private void OnLookPerformed(InputAction.CallbackContext context)
        {
            currentLookInput = context.ReadValue<Vector2>();
            isUsingController = context.control.device is Gamepad;
        }

        private void OnLookCanceled(InputAction.CallbackContext context)
        {
            currentLookInput = Vector2.zero;
        }

        private void UpdateCursorState()
        {
            if (canLook)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (mouseLookAction != null)
            {
                mouseLookAction.performed -= OnLookPerformed;
                mouseLookAction.canceled -= OnLookCanceled;
            }

            if (controllerLookAction != null)
            {
                controllerLookAction.performed -= OnLookPerformed;
                controllerLookAction.canceled -= OnLookCanceled;
            }

            if (interactAction != null)
            {
                interactAction.performed -= OnInteractPerformed;
            }
        }

        public IEnumerator RotateToWorldOrientationCo(Quaternion targetRotation, float duration, bool restoreMovement = false)
        {
            // Disable look control while tweening
            canLook = false;

            // Calculate target pitch (x) and yaw (y) from the target rotation in world space
            Vector3 targetEuler = targetRotation.eulerAngles;
            float targetYaw = targetEuler.y;
            float targetPitch = targetEuler.x;
            float startYaw = playerBody.eulerAngles.y;
            float startPitch = transform.localEulerAngles.x;

            startYaw = Mathf.DeltaAngle(0f, startYaw);
            startPitch = Mathf.DeltaAngle(0f, startPitch);
            targetYaw = Mathf.DeltaAngle(0f, targetYaw);
            targetPitch = Mathf.DeltaAngle(0f, targetPitch);

            // Tween yaw (player body) and pitch (camera) using a DOTween Sequence so they run together
            var seq = DOTween.Sequence();
            seq.Join(DOTween.To(() => startYaw, val =>
            {
                float newYaw = val;
                playerBody.eulerAngles = new Vector3(playerBody.eulerAngles.x, newYaw, playerBody.eulerAngles.z);
            }, targetYaw, duration).SetEase(Ease.InOutSine));

            seq.Join(DOTween.To(() => startPitch, val =>
            {
                float newPitch = val;
                // Apply pitch to this transform's local rotation
                transform.localEulerAngles = new Vector3(newPitch, 0f, 0f);
            }, targetPitch, duration).SetEase(Ease.InOutSine));

            // Start and wait for completion
            yield return seq.Play().WaitForCompletion();

            // Ensure final rotations are exact
            playerBody.eulerAngles = new Vector3(playerBody.eulerAngles.x, targetEuler.y, playerBody.eulerAngles.z);
            transform.localEulerAngles = new Vector3(targetEuler.x, 0f, 0f);

            // Update internal look state to match the new camera pitch to prevent jitter
            xRotation = Mathf.Clamp(targetEuler.x, minVerticalAngle, maxVerticalAngle);
            currentLookInput = Vector2.zero;

            if (restoreMovement)
            {
                canLook = true;
            }
        }
    }
}