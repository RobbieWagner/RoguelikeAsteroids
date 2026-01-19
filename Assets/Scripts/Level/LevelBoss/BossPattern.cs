using UnityEngine;
using System.Collections;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
    public abstract class BossPattern : ScriptableObject
    {
        public float duration;
        public bool canBeInterrupted = false;
        
        public abstract IEnumerator ExecutePattern(LevelBoss boss, System.Action onComplete = null);
        public abstract void CancelPattern();
    }

    public abstract class BossMovementPattern : BossPattern
    {
    }

    public abstract class BossAttackPattern : BossPattern
    {
        public GameObject projectilePrefab;
        public int damage = 1;
        public float attackSpeed = 10f;
    }
}