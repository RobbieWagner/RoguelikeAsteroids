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
        
        public override IEnumerator ExecutePattern(LevelBoss boss, float duration = 1, System.Action onComplete = null)
        {
            Transform bossTransform = boss.transform;
            Vector2 bossPos = boss.transform.position;
            Vector2 toCenter = (Vector2.zero - bossPos).normalized;
            float centerAngle = Mathf.Atan2(toCenter.y, toCenter.x) * Mathf.Rad2Deg;
            float startAngle = centerAngle - spreadAngle;
            
            for (int i = 0; i < numberOfProjectiles; i++)
            {
                float angle = spreadAngle / numberOfProjectiles * i + startAngle;
                Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.up;
                
                Bullet projectile = Instantiate(projectilePrefab, 
                    bossTransform.position, 
                    Quaternion.identity);

                projectile.Fire(direction, attackSpeed, 4);
                
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