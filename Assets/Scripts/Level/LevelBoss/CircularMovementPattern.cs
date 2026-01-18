using UnityEngine;
using RobbieWagnerGames;
using System.Collections;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
	    [CreateAssetMenu(menuName = "RoguelikeAsteroids/Boss Patterns/Circular Movement")]
    public class CircularMovementPattern : BossMovementPattern
    {
        public float radius = 5f;
        public float revolutions = 2f;
        
        public override IEnumerator ExecutePattern(LevelBoss boss, System.Action onComplete = null)
        {
            Transform bossTransform = boss.transform;
            Vector3 startPosition = bossTransform.position;
            float angle = 0f;
            float totalAngle = 360f * revolutions;
            float time = 0f;
            
            while (time < duration)
            {
                angle = Mathf.Lerp(0, totalAngle, time / duration);
                float x = Mathf.Cos(angle * Mathf.Deg2Rad) * radius;
                float y = Mathf.Sin(angle * Mathf.Deg2Rad) * radius;
                
                bossTransform.position = startPosition + new Vector3(x, y, 0);
                
                time += Time.deltaTime;
                yield return null;
            }
            
            onComplete?.Invoke();
        }
        
        public override void CancelPattern()
        {
            // Clean up if needed
        }
    }
}