using System;
using UnityEngine;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
    public enum DestructionReason
    {
        NONE = -1,
        BULLET_HIT,
        OUT_OF_BOUNDS,
        CLEANUP,
        COLLISION_W_PLAYER
    }

    public class Shootable : MonoBehaviour
    {
        private float currentSpeed;
        private Vector2 currentDirection;
        [SerializeField] private Rigidbody2D rb2d;

        public float boundsRadius;
        public DestructionReason destructionReason {get; set;}

        [SerializeField] private Collider2D shootableCollider;
        public ResourceGatherData resourceData = new ResourceGatherData();
        public int durability = 1;

        protected virtual void Awake() {
            GenerateRandomResources();
            destructionReason = DestructionReason.NONE;
        }

        protected virtual void OnCollisionEnter2D(Collision2D collision)
        {
            if(collision.gameObject.CompareTag("bullet"))
            {
                durability--;
                if (durability <= 0)
                    OnShootableDestroyedFromShot();
            }
        }

        protected virtual void OnShootableDestroyedFromShot()
        {
            destructionReason = DestructionReason.BULLET_HIT;
            SpawnResourcePips();
            Destroy(gameObject);
        }

        protected virtual void SpawnResourcePips()
        {
            PipController.Instance.SpawnResourcePips(transform, resourceData);
        }

        public void Initialize(float speed, Vector2 direction, float newBoundsRadius)
        {
            currentSpeed = speed;
            currentDirection = direction;
            boundsRadius = newBoundsRadius;

            rb2d.linearVelocity = currentSpeed * currentDirection;
        }

        private void Update() 
        {
            Vector2 pos = transform.position;
            if(pos.x < -boundsRadius || pos.x > boundsRadius || 
            pos.y < -boundsRadius || pos.y > boundsRadius)
            {
                destructionReason = DestructionReason.OUT_OF_BOUNDS;
                Destroy(gameObject);
            }
        }

        protected virtual void OnDestroy()
        {
            OnShootableDestroyed?.Invoke(this, destructionReason);
        }

        protected virtual void GenerateRandomResources()
        {
            resourceData = new ResourceGatherData();
            
            int totalResourceValue = UnityEngine.Random.Range(1, 10);
            
            ResourceType[] resourceTypes = {
                ResourceType.TITANIUM,
                ResourceType.PLATINUM,
                ResourceType.IRIDIUM
            };
            
            for (int i = 0; i < totalResourceValue; i++)
            {
                ResourceType randomType = resourceTypes[UnityEngine.Random.Range(0, resourceTypes.Length)];
                resourceData.AddResource(randomType, 1);
            }
        }

        public event Action<Shootable, DestructionReason> OnShootableDestroyed;
    }
}