using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using RobbieWagnerGames.Audio;
using UnityEngine;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
    public class ResourcePip : MonoBehaviour
    {
        private ResourceType resourceType = ResourceType.TITANIUM;
        private int amount = 1;
        
        [SerializeField] private float lifetime = 10f;
        [SerializeField] private float pickupRadius = 0.5f;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Collider2D triggerCollider;
        [SerializeField] private Rigidbody2D rb2d;
        private List<float> scales = new List<float>(){.03f, .065f, .1f};

        [SerializeField] private float drag = 1f;

        [SerializedDictionary("Resource","Color")] [SerializeField] private SerializedDictionary<ResourceType, Color> resourceColors = new SerializedDictionary<ResourceType,Color>();
        
        private float timeAlive = 0f;
        
        public void Initialize(ResourceType type, int pipAmount)
        {
            resourceType = type;
            amount = pipAmount;
            SetupVisuals();
        }
        
        private void SetupVisuals()
        {
            spriteRenderer.color = resourceColors[resourceType];
            
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
        }

        private void UpdateRigidbody()
        {
            if(rb2d.linearVelocity.magnitude > 0)
            {
                Vector2 dragForce = -rb2d.linearVelocity.normalized * drag * rb2d.linearVelocity.magnitude * rb2d.linearVelocity.magnitude;
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

            if (timeAlive >= lifetime)
                Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                CollectResource();
            }
        }
        
        private void CollectResource()
        {
            ResourceManager.Instance?.AddResource(resourceType, amount);
            BasicAudioManager.Instance.Play(AudioSourceName.ResourceCollected);
            
            Destroy(gameObject);
        }
        
        public void AddRandomForce(float forceMultiplier = 1f)
        {
            Vector2 randomDirection = UnityEngine.Random.insideUnitCircle.normalized;
            rb2d.AddForce(randomDirection * forceMultiplier, ForceMode2D.Impulse);
        }
    }
}