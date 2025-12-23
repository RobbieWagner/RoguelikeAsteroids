using System;
using UnityEngine;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
    public class PlayerAttractionField : MonoBehaviour
    {
        [SerializeField] private float attractionRadius = 5f;
        [SerializeField] private float attractionForce = 10f;
        [SerializeField] private int maxAttractedPerFrame = 20;
        [SerializeField] private LayerMask attractionLayerMask;
        
        private readonly Collider2D[] nearbyPipsBuffer = new Collider2D[100];
        private ContactFilter2D contactFilter;
        
        private void Awake()
        {
            contactFilter = new ContactFilter2D
            {
                layerMask = attractionLayerMask,
                useLayerMask = true,
                useTriggers = true
            };
        }
        
        private void FixedUpdate()
        {
            int colliderCount = Physics2D.OverlapCircle(
                transform.position, 
                attractionRadius, 
                contactFilter, 
                nearbyPipsBuffer
            );
            
            int pipsToProcess = Math.Min(colliderCount, maxAttractedPerFrame);
            
            for (int i = 0; i < pipsToProcess; i++)
            {
                if (nearbyPipsBuffer[i] != null)
                {
                    Rigidbody2D rb2d = nearbyPipsBuffer[i].GetComponent<Rigidbody2D>();
                    if (rb2d != null)
                        ApplyAttractionForce(rb2d);
                }
            }
            
            // if (pipCount == nearbyPipsBuffer.Length)
            // {
            //     Debug.LogWarning("Attraction field buffer full! Some pips may not be attracted.");
            // }
        }
        
        private void ApplyAttractionForce(Rigidbody2D pipRb)
        {
            Vector2 directionToPlayer = ((Vector2)transform.position - pipRb.position).normalized;
            float distance = Vector2.Distance(transform.position, pipRb.transform.position);
            
            float strengthMultiplier = 1f - (distance / attractionRadius);
            strengthMultiplier = Mathf.Clamp01(strengthMultiplier);
            
            float force = attractionForce * strengthMultiplier * strengthMultiplier;
            
            pipRb.AddForce(directionToPlayer * force);
        }
    }
}