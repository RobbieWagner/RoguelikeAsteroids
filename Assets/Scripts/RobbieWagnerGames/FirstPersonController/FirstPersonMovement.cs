using System.Collections;
using RobbieWagnerGames.Managers;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using RobbieWagnerGames.Utilities;

namespace RobbieWagnerGames.FirstPerson
{
    /// <summary>
    /// Handles first-person character movement with ground detection
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonMovement : MonoBehaviourSingleton<FirstPersonMovement>
    {
        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private float gravity = -9.8f;
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private float groundCheckDistance = 0.1f;

        [Header("Footstep Settings")]
        [SerializeField] private float footstepInterval = 0.5f;
        
        private CharacterController characterController;
        private Vector3 moveInput = Vector3.zero;
        private Vector3 velocity = Vector3.zero;
        private InputAction moveAction;
        private float nextFootstepTime;
        private bool isGrounded;
        private GroundType currentGroundType = GroundType.None;

        private bool canMove = true;
        public bool CanMove
        {
            get => canMove;
            set
            {
                if (value == canMove) return;
                
                canMove = value;
                UpdateMovementState();
                onMovementStateChanged?.Invoke(canMove);
            }
        }

        public bool IsMoving { get; private set; }
        public GroundType CurrentGroundType 
        {
            get => currentGroundType;
            private set
            {
                if (currentGroundType == value) return;
                
                currentGroundType = value;
                OnGroundTypeChanged(value);
            }
        }

        public delegate void MovementStateChanged(bool canMove);
        public event MovementStateChanged onMovementStateChanged;

        protected override void Awake()
        {
            base.Awake();
            characterController = GetComponent<CharacterController>();
            SetupInput();
        }

        private void Update()
        {
            if (!CanMove) return;

            UpdateGroundCheck();
            ApplyGravity();
            MoveCharacter();
            UpdateFootsteps();
        }

        private void SetupInput()
        {
            moveAction = InputManager.Instance.GetAction(ActionMapName.GAME, "Move");
            moveAction.performed += OnMovePerformed;
            moveAction.canceled += OnMoveCanceled;
        }

        private void UpdateGroundCheck()
        {
            Vector3 rayOrigin = transform.position + Vector3.up * 0.01f;
            isGrounded = Physics.Raycast(rayOrigin, Vector3.down, 
                out RaycastHit hit, groundCheckDistance, groundMask);

            if (isGrounded)
            {
                GroundInfo groundInfo = hit.collider.GetComponent<GroundInfo>();
                CurrentGroundType = groundInfo != null ? groundInfo.Type : GroundType.None;
            }
            else
            {
                CurrentGroundType = GroundType.None;
            }
        }

        private void ApplyGravity()
        {
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f; // Small force to keep character grounded
            }
            else
            {
                velocity.y += gravity * Time.deltaTime;
            }
        }

        private void MoveCharacter()
        {
            if (moveInput == Vector3.zero) return;

            Vector3 moveDirection = transform.right * moveInput.x + transform.forward * moveInput.z;
            characterController.Move(moveDirection * walkSpeed * Time.deltaTime + velocity * Time.deltaTime);
        }

        private void UpdateFootsteps()
        {
            if (!IsMoving || !isGrounded) return;

            if (Time.time >= nextFootstepTime)
            {
                PlayFootstepSound();
                nextFootstepTime = Time.time + footstepInterval;
            }
        }

        private void PlayFootstepSound()
        {
            // Implement footstep sound logic based on CurrentGroundType
            // AudioManager.PlayFootstep(CurrentGroundType);
        }

        private void OnGroundTypeChanged(GroundType newType)
        {
            // Handle ground type changes (e.g., adjust footstep sounds)
        }

        private void OnMovePerformed(InputAction.CallbackContext context)
        {
            Vector2 input = context.ReadValue<Vector2>();
            moveInput = new Vector3(input.x, 0, input.y);
            IsMoving = moveInput.magnitude > 0.1f;
        }

        private void OnMoveCanceled(InputAction.CallbackContext context)
        {
            moveInput = Vector3.zero;
            IsMoving = false;
        }

        private void UpdateMovementState()
        {
            if (CanMove)
            {
                moveAction.Enable();
            }
            else
            {
                moveAction.Disable();
                moveInput = Vector3.zero;
                IsMoving = false;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            if (moveAction != null)
            {
                moveAction.performed -= OnMovePerformed;
                moveAction.canceled -= OnMoveCanceled;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Vector3 rayOrigin = transform.position + Vector3.up * 0.01f;
            Gizmos.DrawLine(rayOrigin, rayOrigin + Vector3.down * groundCheckDistance);
        }

        /// <summary>
        /// Smoothly moves the player to a specific world position using DoTween.
        /// </summary>
        /// <param name="targetPosition">The world position to move to.</param>
        /// <param name="duration">The duration of the move in seconds.</param>
        /// <param name="restoreMovement">Whether to restore movement after rotation.</param>
        public IEnumerator MoveToWorldPositionCo(Vector3 targetPosition, float duration, bool restoreMovement = false)
        {
            CanMove = false;
            characterController.enabled = false;

            // Use DoTween to animate the transform's position
            yield return transform.DOMove(targetPosition, duration).SetEase(Ease.InOutSine).WaitForCompletion();

            // Ensure final position is set
            transform.position = targetPosition;

            if (restoreMovement)
            {
                CanMove = true;
                characterController.enabled = true;
            }
        }
    }
}