using UnityEngine;
using System.Collections;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
    public abstract class BossPattern : ScriptableObject
    {
        public bool canBeInterrupted = false;
        
        public abstract IEnumerator ExecutePattern(LevelBoss boss, float duration = 1, System.Action onComplete = null);
        public abstract void CancelPattern();
    }

    public abstract class BossMovementPattern : BossPattern
    {
    }

    public abstract class BossAttackPattern : BossPattern
    {
        public Bullet projectilePrefab;
        public int damage = 1;
        public float attackSpeed = 10f;
    }
}