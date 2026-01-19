using UnityEngine;
using System.Collections;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
	[CreateAssetMenu(menuName = "RoguelikeAsteroids/Boss Patterns/Circular Movement")]
    public class CircularMovementPattern : BossMovementPattern
    {
        public float radius = 5f;
        public float revolutions = 2f;
        [SerializeField] private  float startAngle = 0f;
        [SerializeField] private Vector2 center;
        
        public override IEnumerator ExecutePattern(LevelBoss boss, System.Action onComplete = null)
        {
            Debug.Log("executing circle move");
            Transform bossTransform = boss.transform;
            float angle = startAngle;
            float totalAngle = 360f * revolutions + startAngle;
            float time = 0f;
            
            while (time < duration)
            {
                angle = Mathf.Lerp(startAngle, totalAngle, time / duration);
                float x = Mathf.Cos(angle * Mathf.Deg2Rad) * radius;
                float y = Mathf.Sin(angle * Mathf.Deg2Rad) * radius;
                
                bossTransform.position = center + new Vector2(x, y);
                
                time += Time.deltaTime;
                yield return null;
            }
            
            Debug.Log("circle move complete");
            onComplete?.Invoke();
        }
        
        public override void CancelPattern()
        {
            // Clean up if needed
        }
    }
}