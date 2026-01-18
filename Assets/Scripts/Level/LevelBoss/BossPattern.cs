using UnityEngine;
using System.Collections;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
    public abstract class BossPattern : ScriptableObject
    {
        public string patternName;
        public float duration;
        public bool canBeInterrupted = false;
        
        public abstract IEnumerator ExecutePattern(LevelBoss boss, System.Action onComplete = null);
        public abstract void CancelPattern();
    }

    public abstract class BossMovementPattern : BossPattern
    {
        public AnimationCurve movementCurve;
        public float movementSpeed = 5f;
    }

    public abstract class BossAttackPattern : BossPattern
    {
        public GameObject projectilePrefab;
        public int damage = 1;
        public float attackSpeed = 10f;
    }
}