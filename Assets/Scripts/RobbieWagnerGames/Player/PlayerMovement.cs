using System.Collections;
using DG.Tweening;
using RobbieWagnerGames.Managers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RobbieWagnerGames
{
    [RequireComponent(typeof(CharacterController), (typeof(SpriteRenderer)))]
    public class PlayerMovement : MonoBehaviour
    {
        public static PlayerMovement Instance { get; private set; }

        [Header("Movement Settings")]
        [SerializeField] private float defaultWalkSpeed = 3f;
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private CharacterController characterController;
        
        [Header("Animation")]
        [SerializeField] public UnitAnimator movementAnimator;
        
        [Header("Audio")]
        [SerializeField] private AudioSource footstepAudioSource;
        [SerializeField] private AudioClip[] footstepSoundClips;

        private SpriteRenderer spriteRenderer;
        private PlayerMovementActions inputActions;
        private Vector3 movementVector;
        private float currentWalkSpeed;
        private Vector3 lastFramePos;
        private Vector3 lastPosition;
        
        private const float GRAVITY = -7.5f;
        private bool isGrounded;
        private bool movingForcibly;
        private int currentGroundType;

        public bool CanMove { get; private set; } = true;
        public bool IsMoving { get; private set; } = false;
        public int CurrentGroundType
        {
            get => currentGroundType;
            set
            {
                if (currentGroundType == value) return;
                currentGroundType = value;
                ChangeFootstepSounds(footstepSoundClips[currentGroundType]);
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;

            spriteRenderer = GetComponent<SpriteRenderer>();
            characterController = GetComponent<CharacterController>();
            
            InitializeMovement();
            InitializeInput();
        }

        private void InitializeMovement()
        {
            currentWalkSpeed = defaultWalkSpeed;
            movementVector = Vector3.zero;
        }

        private void InitializeInput()
        {
            inputActions = new PlayerMovementActions();
            inputActions.Enable();
            // InputManager.Instance.Controls.EXPLORATION.Move.performed += OnMove;
            // InputManager.Instance.Controls.EXPLORATION.Move.canceled += StopPlayer;
        }

        private void LateUpdate()
        {
            CheckGroundStatus();
            ApplyGravity();
            
            if (characterController.enabled)
            {
                characterController.Move(movementVector * (currentWalkSpeed * Time.deltaTime));
            }

            lastFramePos = transform.position;

            if (movingForcibly)
            {
                Animate();
            }
        }

        private void CheckGroundStatus()
        {
            if (Physics.Raycast(transform.position, Vector3.down, out var hit, 0.05f, groundMask))
            {
                isGrounded = true;
                UpdateGroundType(hit);
            }
            else
            {
                isGrounded = false;
                StartCoroutine(FootStepStopTimer(0.25f));
            }
        }

        private void UpdateGroundType(RaycastHit hit)
        {
            var groundInfo = hit.collider.GetComponent<GroundInfo>();
            CurrentGroundType = groundInfo != null && (int)groundInfo.groundType < footstepSoundClips.Length 
                ? (int)groundInfo.groundType 
                : 0;
        }

        private void ApplyGravity()
        {
            movementVector.y = isGrounded ? 0f : movementVector.y + GRAVITY * Time.deltaTime;
        }

        private void OnMove(InputAction.CallbackContext context)
        {
            if (!CanMove) return;

            Vector2 input = context.ReadValue<Vector2>();
            UpdateMovementState(input);
            movementVector.x = input.x;
            movementVector.z = input.y;
        }

        private void UpdateMovementState(Vector2 input)
        {
            if (movementVector.x != input.x && input.x != 0f)
            {
                UpdateHorizontalMovementAnimation(input.x);
                IsMoving = true;
            }
            else if (input.x == 0 && movementVector.z != input.y && input.y != 0f)
            {
                UpdateVerticalMovementAnimation(input.y);
                IsMoving = true;
            }
            else if (input == Vector2.zero)
            {
                SetIdleAnimation();
                IsMoving = false;
                StopMovementSounds();
            }
        }

        private void UpdateHorizontalMovementAnimation(float xInput)
        {
            movementAnimator.ChangeAnimationState(xInput > 0 
                ? UnitAnimationState.WalkRight 
                : UnitAnimationState.WalkLeft);
        }

        private void UpdateVerticalMovementAnimation(float yInput)
        {
            movementAnimator.ChangeAnimationState(yInput > 0 
                ? UnitAnimationState.WalkForward 
                : UnitAnimationState.WalkBack);
        }

        private void SetIdleAnimation()
        {
            if (movementVector.x > 0)
                movementAnimator.ChangeAnimationState(UnitAnimationState.IdleRight);
            else if (movementVector.x < 0)
                movementAnimator.ChangeAnimationState(UnitAnimationState.IdleLeft);
            else if (movementVector.z > 0)
                movementAnimator.ChangeAnimationState(UnitAnimationState.IdleForward);
            else
                movementAnimator.ChangeAnimationState(UnitAnimationState.IdleBack);
        }

        private void OnDisable()
        {
            StopPlayer();
        }



        public void StopPlayer(InputAction.CallbackContext context)
        {
            StopPlayer();
        }

        public void StopPlayer()
        { 
            SetIdleAnimation();
            movementVector = Vector3.zero;
            IsMoving = false;
            StopMovementSounds();
        }

        public void SetMovementEnabled(bool enabled)
        {
            if (CanMove == enabled) return;
            
            CanMove = enabled;
            
            if (CanMove)
            {
                inputActions.Enable();
                spriteRenderer.enabled = true;
            }
            else
            {
                CeasePlayerMovement();
                inputActions.Disable();
                spriteRenderer.enabled = false;
            }
        }

        public void CeasePlayerMovement()
        {
            StopPlayer();
            CanMove = false;
        }

        public IEnumerator MovePlayerToSpot(Vector3 position, float unitsPerSecond = -1)
        {
            CeasePlayerMovement();
            PlayMovementSounds();
            characterController.enabled = false;

            lastPosition = transform.position;
            float speed = unitsPerSecond < 0 ? currentWalkSpeed : unitsPerSecond;
            
            movingForcibly = true;
            yield return transform.DOMove(position, Vector3.Distance(position, transform.position) / speed)
                .SetEase(Ease.Linear)
                .WaitForCompletion();
            movingForcibly = false;

            characterController.enabled = true;
        }

        public void Warp(Vector3 position)
        {
            characterController.enabled = false;
            transform.position = position;
            characterController.enabled = true;
        }

        private void Animate()
        {
            Vector3 positionDelta = transform.position - lastPosition;

            if (Mathf.Abs(positionDelta.x) > Mathf.Abs(positionDelta.z))
            {
                movementAnimator.ChangeAnimationState(positionDelta.x > 0 
                    ? UnitAnimationState.WalkRight 
                    : UnitAnimationState.WalkLeft);
            }
            else
            {
                movementAnimator.ChangeAnimationState(positionDelta.z > 0 
                    ? UnitAnimationState.WalkForward 
                    : UnitAnimationState.WalkBack);
            }

            lastPosition = transform.position;
        }

        private void ChangeFootstepSounds(AudioClip clip)
        {
            // Implementation for changing footstep sounds
        }

        public void PlayMovementSounds()
        {
            // Implementation for playing movement sounds
        }

        public void StopMovementSounds()
        {
            // Implementation for stopping movement sounds
        }

        private IEnumerator FootStepStopTimer(float timeToTurnOff)
        {
            float timerValue = 0f;
            while (timerValue < timeToTurnOff)
            {
                yield return null;
                if (isGrounded) break;
                timerValue += Time.deltaTime;
            }
            
            if (timerValue >= timeToTurnOff) 
            {
                StopMovementSounds();
            }
        }

        private void OnDestroy()
        {
            // InputManager.Instance.Controls.EXPLORATION.Move.performed -= OnMove;
            // InputManager.Instance.Controls.EXPLORATION.Move.canceled -= StopPlayer;
        }
    }
}