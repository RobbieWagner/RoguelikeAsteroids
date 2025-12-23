using System;
using System.Collections;
using RobbieWagnerGames.Managers;
using RobbieWagnerGames.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityExtensionMethods;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
    public class Player : MonoBehaviourSingleton<Player>
    {
        [Header("Movement")]
        private Vector2 movementVector = Vector2.zero;
        [SerializeField] private float speed = 5;
        [SerializeField] private Rigidbody2D rb2d;
        
        [Header("Look")]
        [SerializeField] private Transform lookAtTarget;        
        private Vector2 lookVector = Vector2.zero;

        [Header("Shooter")]
        [SerializeField] private float shooterCooldown = 1f;
        private float shooterCooldownTimer = 0f;
        [SerializeField] private Bullet bullet;
        [SerializeField] private float bulletSpawnDistance = .5f; 
        [SerializeField] private float bulletSpeed = 50;
        [SerializeField] private float bulletTimeToLive = 2f;
        [SerializeField] private AudioSource fireSound;

        [SerializeField] private Vector2 movementBounds;

        protected override void Awake()
        {
            base.Awake();

            InputManager.Instance.Controls.GAME.Move.performed += OnMove;
            InputManager.Instance.Controls.GAME.Move.canceled += OnStop;

            InputManager.Instance.Controls.GAME.ControllerLook.performed += OnControllerLook;
            InputManager.Instance.Controls.GAME.ControllerLook.canceled += OnControllerLookCanceled;

            InputManager.Instance.Controls.GAME.MousePosition.performed += OnMouseDelta;

            InputManager.Instance.Controls.GAME.Shoot.performed += OnShoot;

            EnableControls();
        }

        private void Update()
        {
            UpdateMovement();
            UpdateLook();
            UpdateShooter();
        }

        private void UpdateMovement()
        {
            Vector2 moveVector = movementVector;
            moveVector *= speed;
            rb2d.linearVelocity = moveVector;
            
            ClampPositionToBounds();
        }

        private void ClampPositionToBounds()
        {
            Vector2 currentPos = transform.position;
            
            currentPos.x = Mathf.Clamp(currentPos.x, -movementBounds.x, movementBounds.x);
            currentPos.y = Mathf.Clamp(currentPos.y, -movementBounds.y, movementBounds.y);
            
            transform.position = currentPos;
        }

        private void UpdateLook()
        {
            lookAtTarget.position = (Vector2)transform.position + lookVector;
            transform.LookAt2D(lookAtTarget);
        }

        private void UpdateShooter()
        {
            shooterCooldownTimer = Mathf.Clamp(shooterCooldownTimer - Time.deltaTime, 0, shooterCooldown); 
        }

        private void OnMove(InputAction.CallbackContext context)
        {
            movementVector = context.ReadValue<Vector2>();
        }

        private void OnStop(InputAction.CallbackContext context)
        {
            movementVector = Vector2.zero;
        }
        
        private void OnControllerLook(InputAction.CallbackContext context)
        {
            lookVector = context.ReadValue<Vector2>();
        }

        private void OnControllerLookCanceled(InputAction.CallbackContext context)
        {
            lookVector = Vector2.zero;
        }

        private void OnMouseDelta(InputAction.CallbackContext context)
        {
            if (Camera.main == null) return;

            Vector2 mousePos = Mouse.current.position.ReadValue();

            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(
                new Vector3(mousePos.x, mousePos.y, Camera.main.transform.position.z * -1)
            );
            mouseWorldPos.z = 0;

            Vector2 direction = mouseWorldPos - transform.position;

            lookVector = direction.normalized;
        }

        private void OnShoot(InputAction.CallbackContext context)
        {
            if (shooterCooldownTimer <= 0)
            {
                shooterCooldownTimer = shooterCooldown;

                StartCoroutine(FireBullet());
                if(!fireSound.isPlaying)
                    fireSound.Play();
            }
        }

        private IEnumerator FireBullet()
        {
            Bullet newBullet = Instantiate(bullet);
            newBullet.transform.position = (Vector2) transform.position + (lookVector * bulletSpawnDistance);

            Vector2 aimDirection = lookVector == Vector2.zero ? Vector2.up : lookVector;

            yield return null;

            StartCoroutine(newBullet.Fire(aimDirection, bulletSpeed, bulletTimeToLive));
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if(other.gameObject.CompareTag("asteroid"))
                OnPlayerKilled();
        }

        private void OnPlayerKilled()
        {
            DisableControls();
            GameManager.Instance.NotifyGameOver();
        }

        public void DisableControls() => InputManager.Instance.DisableActionMap(ActionMapName.GAME);
        public void EnableControls() => InputManager.Instance.EnableActionMap(ActionMapName.GAME);

        protected override void OnDestroy()
        {
            CleanupInputCallbacks();
            base.OnDestroy();
        }

        private void OnDisable()
        {
            if (!gameObject.scene.isLoaded)
                CleanupInputCallbacks();
        }

        private void CleanupInputCallbacks()
        {
            if (InputManager.Instance != null && InputManager.Instance.Controls != null)
            {
                var gameControls = InputManager.Instance.Controls.GAME;
                
                gameControls.Move.performed -= OnMove;
                gameControls.Move.canceled -= OnStop;
                gameControls.ControllerLook.performed -= OnControllerLook;
                gameControls.ControllerLook.canceled -= OnControllerLookCanceled;
                gameControls.MousePosition.performed -= OnMouseDelta;
                gameControls.Shoot.performed -= OnShoot;
            }
        }
    }
}