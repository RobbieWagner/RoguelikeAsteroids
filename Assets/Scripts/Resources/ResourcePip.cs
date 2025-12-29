using System;
using System.Collections.Generic;
using RobbieWagnerGames.Audio;
using UnityEngine;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
    public class ResourcePip : MonoBehaviour
    {
        private ResourceType resourceType = ResourceType.TITANIUM;
        private int amount = 1;
        
        [SerializeField] private float lifetime = 10f;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Collider2D triggerCollider;
        [SerializeField] private Rigidbody2D rb2d;
        private List<float> scales = new List<float>(){.03f, .065f, .1f};

        private bool isBeingCollected = false;

        [SerializeField] private float drag = 1f;
        public float Drag => isBeingCollected ? .1f : drag;
        [SerializeField] private float collectionRadius = 0.5f;
        [SerializeField] private float magnetSpeed = 5f;
        
        private float timeAlive = 0f;
        private bool isCollected = false;
        
        public ResourceType ResourceType => resourceType;
        public int Amount => amount;
        
        public event Action<ResourcePip, ResourceType, int> OnPipCollected;
        public event Action<ResourcePip> OnPipDestroyed;
        
        public void Initialize(ResourceType type, int pipAmount)
        {
            resourceType = type;
            amount = pipAmount;
            SetupVisuals();
        }
        
        private void SetupVisuals()
        {
            spriteRenderer.color = GameConstants.Instance.resourceColors[resourceType];
            
            float scale;
            switch(amount)
            {
                case 5:
                    scale = scales[1];
                    break;
                case 10:
                    scale = scales[2];
                    break;
                case 1:
                default:
                    scale = scales[0];
                    break;
            }
            transform.localScale = new Vector3(scale, scale, 1f);
        }
        
        private void Update()
        {
            UpdateLifetime();
            UpdateRigidbody();
            
            if (!isCollected && Player.Instance != null)
                UpdateMagnetEffect();
        }

        private void UpdateMagnetEffect()
        {
            float distanceToPlayer = Vector2.Distance(transform.position, Player.Instance.transform.position);
            
            if (distanceToPlayer < collectionRadius)
                CollectPip();
            else if (distanceToPlayer < collectionRadius * 4f)
            {
                isBeingCollected = true;
                Vector2 directionToPlayer = (Player.Instance.transform.position - transform.position).normalized;
                rb2d.AddForce(directionToPlayer * magnetSpeed * Time.deltaTime, ForceMode2D.Force);
            }
            else
            {
                isBeingCollected = false;
            }
        }

        private void UpdateRigidbody()
        {
            if(rb2d.linearVelocity.magnitude > 0)
            {
                Vector2 dragForce = -rb2d.linearVelocity.normalized * Drag * rb2d.linearVelocity.magnitude * rb2d.linearVelocity.magnitude;
                rb2d.AddForce(dragForce);
                
                if (rb2d.linearVelocity.magnitude < 0.01f)
                    rb2d.linearVelocity = Vector2.zero;
            }
        }

        private void UpdateLifetime()
        {
            timeAlive += Time.deltaTime;

            if (timeAlive > lifetime * 0.8f)
            {
                float alpha = 1f - ((timeAlive - lifetime * 0.8f) / (lifetime * 0.2f));
                Color color = spriteRenderer.color;
                color.a = alpha;
                spriteRenderer.color = color;
            }

            if (timeAlive >= lifetime && !isCollected)
                DestroyPip();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!isCollected && other.CompareTag("Player"))
                CollectPip();
        }
        
        private void CollectPip()
        {
            if (isCollected) return;
            
            isCollected = true;
            
            ResourceManager.Instance?.AddResource(resourceType, amount);
            BasicAudioManager.Instance?.Play(AudioSourceName.ResourceCollected);
            
            OnPipCollected?.Invoke(this, resourceType, amount);
            DestroyPip();
        }
        
        private void DestroyPip()
        {
            OnPipDestroyed?.Invoke(this);
            
            OnPipCollected = null;
            OnPipDestroyed = null;
            
            Destroy(gameObject);
        }
        
        public void AddRandomForce(float forceMultiplier = 1f)
        {
            Vector2 randomDirection = UnityEngine.Random.insideUnitCircle.normalized;
            rb2d.AddForce(randomDirection * forceMultiplier, ForceMode2D.Impulse);
        }
        
        private void OnDestroy()
        {
            OnPipCollected = null;
            OnPipDestroyed = null;
        }
    }
}