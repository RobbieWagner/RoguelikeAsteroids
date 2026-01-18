using UnityEngine;
using System.Collections;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
	[CreateAssetMenu(menuName = "RoguelikeAsteroids/Boss Patterns/Idle Movement")]
    public class IdleMovementPattern : BossMovementPattern
    {   
        public override IEnumerator ExecutePattern(LevelBoss boss, System.Action onComplete = null)
        {
            yield return new WaitForSeconds(duration);       
            onComplete?.Invoke();
        }
        
        public override void CancelPattern()
        {
            // Clean up if needed
        }
    }
}