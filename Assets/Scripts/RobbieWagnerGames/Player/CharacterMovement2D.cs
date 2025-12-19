using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RobbieWagnerGames.Managers
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class CharacterMovement2D : MonoBehaviour
    {
        public static CharacterMovement2D Instance { get; private set; }

        [SerializeField] private UnitAnimator unitAnimator;
        [SerializeField] private float currentWalkSpeed = 3f;
        [SerializeField] [Range(-1, 0)] private float wallPushback = -0.08f;
        [SerializeField] private AudioSource footstepAudioSource;

        private SpriteRenderer spriteRenderer;
        private HashSet<Collider2D> colliders;
        private Vector2 movementVector = Vector2.zero;
        private Vector3 lastFramePos = Vector3.zero;
        
        public bool CanMove { get; set; } = true;
        public bool IsMoving { get; private set; } = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            
            spriteRenderer = GetComponent<SpriteRenderer>();
            colliders = new HashSet<Collider2D>();
            
            InitializeInput();
        }

        private void InitializeInput()
        {
            // InputManager.Instance.Controls.EXPLORATION.Move.performed += OnMove;
            // InputManager.Instance.Controls.EXPLORATION.Move.canceled += StopPlayer;
        }

        private void LateUpdate()
        {
            if (CanMove)
            {
                MoveCharacter();
                HandleWallPushback();
            }

            lastFramePos = transform.position;
            HandleMovementSounds();
        }

        private void MoveCharacter()
        {
            transform.Translate(movementVector * (currentWalkSpeed * Time.deltaTime));
        }

        private void HandleWallPushback()
        {
            foreach (var collider in colliders)
            {
                transform.position = Vector2.MoveTowards(transform.position, 
                    collider.transform.position, wallPushback);
            }
        }

        private void HandleMovementSounds()
        {
            if (IsMoving && footstepAudioSource != null && !footstepAudioSource.isPlaying)
            {
                PlayMovementSounds();
            }
        }

        private void OnMove(InputAction.CallbackContext context)
        {
            if (!CanMove) return;

            Vector2 input = context.ReadValue<Vector2>();
            UpdateMovementState(input);
            movementVector = input;
        }

        private void UpdateMovementState(Vector2 input)
        {
            if (movementVector.x != input.x && input.x != 0f)
            {
                UpdateHorizontalMovementAnimation(input.x);
                IsMoving = true;
            }
            else if (input.x == 0 && movementVector.y != input.y && input.y != 0f)
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
            unitAnimator.ChangeAnimationState(xInput > 0 ? 
                UnitAnimationState.WalkRight : UnitAnimationState.WalkLeft);
        }

        private void UpdateVerticalMovementAnimation(float yInput)
        {
            unitAnimator.ChangeAnimationState(yInput > 0 ? 
                UnitAnimationState.WalkForward : UnitAnimationState.WalkBack);
        }

        private void SetIdleAnimation()
        {
            if (movementVector.x > 0)
                unitAnimator.ChangeAnimationState(UnitAnimationState.IdleRight);
            else if (movementVector.x < 0)
                unitAnimator.ChangeAnimationState(UnitAnimationState.IdleLeft);
            else if (movementVector.y > 0)
                unitAnimator.ChangeAnimationState(UnitAnimationState.IdleForward);
            else
                unitAnimator.ChangeAnimationState(UnitAnimationState.IdleBack);
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
            movementVector = Vector2.zero;
            IsMoving = false;
            StopMovementSounds();
        }

        public void PlayMovementSounds()
        {
            if(footstepAudioSource != null)
                footstepAudioSource?.Play();
        }

        public void StopMovementSounds()
        {
            if(footstepAudioSource != null)
                footstepAudioSource?.Stop();
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            Debug.Log("New collision");
            colliders.Add(other.collider);
        }

        private void OnCollisionExit2D(Collision2D other)
        {
            colliders.Remove(other.collider);
        }

        private void OnDestroy()
        {
            // InputManager.Instance.Controls.EXPLORATION.Move.performed -= OnMove;
            // InputManager.Instance.Controls.EXPLORATION.Move.canceled -= StopPlayer;
        }
    }
}