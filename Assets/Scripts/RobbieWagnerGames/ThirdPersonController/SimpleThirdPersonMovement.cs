using RobbieWagnerGames.Managers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RobbieWagnerGames.FirstPerson
{
    [RequireComponent(typeof(CharacterController))]
    public class SimpleThirdPersonMovement : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CharacterController characterController;
        [SerializeField] private Transform referenceCamera;
        
        [Header("Movement Settings")]
        [SerializeField] private float movementSpeed = 6f;
        [SerializeField] private float turnSmoothTime = 0.1f;
        
        [Header("Grounding Settings")]
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private float groundCheckDistance = 0.1f;
        [SerializeField] private float groundCheckOffset = 0.01f;
        
        private const float GRAVITY = -9.8f;
        
        private float turnVelocity;
        private bool isMoving = false;
        private bool isGrounded = false;
        private bool canMove = true;
        
        private Vector3 inputVector = Vector3.zero;
        private GroundType currentGroundType = GroundType.None;
        
        public static SimpleThirdPersonMovement Instance { get; private set; }
        
        public GroundType CurrentGroundType
        {
            get => currentGroundType;
            private set
            {
                if (currentGroundType == value) return;
                currentGroundType = value;
            }
        }
        
        public bool CanMove
        {
            get => canMove;
            set
            {
                if (value == canMove) return;
                canMove = value;
                OnToggleMovement?.Invoke(canMove);
            }
        }
        
        public delegate void ToggleMovementDelegate(bool isEnabled);
        public event ToggleMovementDelegate OnToggleMovement;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            SetupControls();
        }
        
        private void OnDestroy()
        {
            CleanupControls();
        }
        
        private void SetupControls()
        {
            // InputManager.Instance.Controls.EXPLORATION.Move.performed += HandleMovementInput;
            // InputManager.Instance.Controls.EXPLORATION.Move.canceled += HandleStopInput;
            OnToggleMovement += ToggleMovement;
            
            if (CanMove)
            {
                InputManager.Instance.EnableActionMap(ActionMapName.EXPLORATION);
            }
        }
        
        private void CleanupControls()
        {
            if (InputManager.Instance != null)
            {
                // InputManager.Instance.Controls.EXPLORATION.Move.performed -= HandleMovementInput;
                // InputManager.Instance.Controls.EXPLORATION.Move.canceled -= HandleStopInput;
                OnToggleMovement -= ToggleMovement;
            }
        }
        
        private void LateUpdate()
        {
            UpdateGroundCheck();
            HandleMovement();
        }
        
        private void HandleMovementInput(InputAction.CallbackContext context)
        {
            Vector2 input = context.ReadValue<Vector2>();
            Vector3 direction = new Vector3(input.x, 0, input.y).normalized;
            
            isMoving = direction.magnitude >= 0.1f;
            inputVector = isMoving ? direction : Vector3.zero;
        }
        
        private void HandleStopInput(InputAction.CallbackContext context)
        {
            isMoving = false;
            inputVector = Vector3.zero;
        }
        
        private void HandleMovement()
        {
            if (!isMoving || inputVector.magnitude < 0.1f || !characterController.enabled) return;
            
            float targetAngle = Mathf.Atan2(inputVector.x, inputVector.z) * Mathf.Rad2Deg + referenceCamera.eulerAngles.y;
            float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnVelocity, turnSmoothTime);
            
            transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);
            
            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            characterController.Move(moveDirection.normalized * movementSpeed * Time.deltaTime);
        }
        
        private void UpdateGroundCheck()
        {
            Vector3 rayOrigin = transform.position + new Vector3(0, groundCheckOffset, 0);
            bool wasGrounded = isGrounded;
            
            isGrounded = Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 
                                       groundCheckDistance, groundMask);
            
            if (isGrounded)
            {
                GroundInfo groundInfo = hit.collider.GetComponent<GroundInfo>();
                CurrentGroundType = groundInfo != null ? groundInfo.Type : GroundType.None;
            }
            else
            {
                CurrentGroundType = GroundType.None;
            }
            
            inputVector.y = isGrounded ? 0f : inputVector.y + GRAVITY * Time.deltaTime;
        }
        
        private void ToggleMovement(bool isEnabled)
        {
            if (isEnabled)
            {
                // InputManager.Instance.Controls.EXPLORATION.Move.Enable();
            }
            else
            {
                // InputManager.Instance.Controls.EXPLORATION.Move.Disable();
            }
        }
        
        #if UNITY_EDITOR
        private void OnValidate()
        {
            if (characterController == null)
            {
                characterController = GetComponent<CharacterController>();
            }
        }
        #endif
    }
}