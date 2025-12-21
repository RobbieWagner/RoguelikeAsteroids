using System;
using RobbieWagnerGames.Audio;
using RobbieWagnerGames.RoguelikeAsteroids;
using Unity.Mathematics;
using UnityEngine;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
    public enum DestructionReason
    {
        NONE = -1,
        BULLET_HIT,
        OUT_OF_BOUNDS,
        CLEANUP
    }

    public class Shootable : MonoBehaviour
    {
        private float currentSpeed;
        private Vector2 currentDirection;
        [SerializeField] private Rigidbody2D rb2d;

        public float boundsRadius;
        private DestructionReason destructionReason = DestructionReason.NONE;

        [SerializeField] private Collider2D shootableCollider;

        protected virtual void OnCollisionEnter2D(Collision2D collision)
        {
            if(collision.gameObject.CompareTag("bullet"))
            {
                destructionReason = DestructionReason.BULLET_HIT;
                BasicAudioManager.Instance.Play(AudioSourceName.ShootableDestroyed);
                Destroy(gameObject);
            }
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

        private void OnDestroy()
        {
            OnShootableDestroyed?.Invoke(this, destructionReason);
        }

        public event Action<Shootable, DestructionReason> OnShootableDestroyed;
    }
}