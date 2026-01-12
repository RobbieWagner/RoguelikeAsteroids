using System;
using System.Collections;
using DG.Tweening;
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
        private Vector2 movementInput = Vector2.zero;
        [SerializeField] private float speed = 5;
        [SerializeField] private Rigidbody2D rb2d;
        [SerializeField] private Collider2D coll;
        
        [Header("Movement Smoothing")]
        [SerializeField] private float acceleration = 10f;
        [SerializeField] private float deceleration = 15f;
        [SerializeField] private float currentSpeed = 0f;
        
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

        private Coroutine disableCollCo = null;

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

            PlayerManager.Instance.RegisterPlayer(this);


        }

        private void Update()
        {
            UpdateMovement();
            UpdateLook();
            UpdateShooter();
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if(other.gameObject.CompareTag("asteroid"))
            {
                Debug.Log("hello");
                other.GetComponent<Shootable>().destructionReason = DestructionReason.COLLISION_W_PLAYER;
                Destroy(other.gameObject);
                OnPlayerHit();
            }
        }

        private void OnPlayerHit()
        {
            PlayerManager.Instance.PlayerHit();
        }

        private void UpdateMovement()
        {
            float targetSpeed = movementInput.magnitude > 0.1f ? speed : 0f;
            
            if (targetSpeed > currentSpeed)
                currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.deltaTime);
            else if (targetSpeed < currentSpeed)
                currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.deltaTime);
       
            if (currentSpeed > 0.01f)
            {
                Vector2 moveDirection = movementInput.magnitude > 0 ? movementInput.normalized : rb2d.linearVelocity.normalized;
                Vector2 targetVelocity = moveDirection * currentSpeed;
                rb2d.linearVelocity = targetVelocity;
            }
            else
            {
                rb2d.linearVelocity = Vector2.zero;
                currentSpeed = 0f;
            }
            
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
            movementInput = context.ReadValue<Vector2>();
        }

        private void OnStop(InputAction.CallbackContext context)
        {
            movementInput = Vector2.zero;
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

        public void DisableColliderTemporarily(float duration)
        {
            if (disableCollCo == null)
                disableCollCo = StartCoroutine(DisableColliderTemporarilyCo(duration));
        }

        private IEnumerator DisableColliderTemporarilyCo(float duration)
        {
            coll.enabled = false;
            yield return new WaitForSeconds(duration);
            coll.enabled = true;
            disableCollCo = null;
        }
    }
}