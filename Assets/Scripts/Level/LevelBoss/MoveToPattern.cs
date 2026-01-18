using UnityEngine;
using System.Collections;
using DG.Tweening;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
	[CreateAssetMenu(menuName = "RoguelikeAsteroids/Boss Patterns/Move To")]
    public class MoveToPattern : BossMovementPattern
    {   
		[SerializeField] private Vector2 location;
        public override IEnumerator ExecutePattern(LevelBoss boss, System.Action onComplete = null)
        {
			Transform bossTransform = boss.transform;

			yield return bossTransform.DOMove(location, duration).SetEase(Ease.Linear).WaitForCompletion();
                  
            onComplete?.Invoke();
        }
        
        public override void CancelPattern()
        {
            // Clean up if needed
        }
    }
}