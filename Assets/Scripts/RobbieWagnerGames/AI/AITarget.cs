using System.Collections;
using UnityEngine;

namespace RobbieWagnerGames.AI
{
    /// <summary>
    /// Base class for objects that can be targeted by AI agents
    /// </summary>
    public class AITarget : MonoBehaviour
    {
        [Header("Target Settings")]
        [SerializeField] private bool destroyOnCaught = false;
        [SerializeField] private float respawnTime = 0f;
        
        private Vector3 originalPosition;
        private bool isActive = true;

        protected virtual void Awake()
        {
            originalPosition = transform.position;
        }

        /// <summary>
        /// Called when an agent reaches/catches this target
        /// </summary>
        public virtual void OnCaught(AIAgent agent)
        {
            if (!isActive) return;

            Debug.Log($"{agent.name} caught {name}", this);

            if (destroyOnCaught)
            {
                HandleDestroy();
            }
            else if (respawnTime > 0)
            {
                StartCoroutine(RespawnCoroutine());
            }
        }

        private IEnumerator RespawnCoroutine()
        {
            isActive = false;
            SetTargetVisible(false);
            
            yield return new WaitForSeconds(respawnTime);
            
            transform.position = originalPosition;
            isActive = true;
            SetTargetVisible(true);
        }

        private void HandleDestroy()
        {
            isActive = false;
            Destroy(gameObject);
        }

        protected virtual void SetTargetVisible(bool visible)
        {
            foreach (var renderer in GetComponentsInChildren<Renderer>())
            {
                renderer.enabled = visible;
            }
        }

        public bool IsActive => isActive;
    }
}