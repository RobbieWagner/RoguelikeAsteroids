using UnityEngine;
using System.Collections;
using DG.Tweening;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
    [CreateAssetMenu(menuName = "RoguelikeAsteroids/Boss Patterns/Radial Attack")]
    public class RadialAttackPattern : BossAttackPattern
    {
        public int numberOfProjectiles = 8;
        public float spreadAngle = 360f;
        
        
        // Have follow the player?
        public override IEnumerator ExecutePattern(LevelBoss boss, System.Action onComplete = null)
        {
            Transform bossTransform = boss.transform;
            
            for (int i = 0; i < numberOfProjectiles; i++)
            {
                float angle = spreadAngle / numberOfProjectiles * i;
                Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.up;
                
                GameObject projectile = Instantiate(projectilePrefab, 
                    bossTransform.position, 
                    Quaternion.identity);
                
                Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
                if (rb != null)
                    rb.linearVelocity = direction * attackSpeed;
                
                yield return new WaitForSeconds(0.1f);
            }
            
            onComplete?.Invoke();
        }
        
        public override void CancelPattern()
        {
            // Clean up if needed
        }
    }
}