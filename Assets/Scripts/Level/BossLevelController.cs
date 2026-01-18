using UnityEngine;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
    public class BossLevelController : AsteroidsLevelController
    {
        [SerializeField] private LevelBoss boss;
        [SerializeField] private bool spawnAsteroids = true;
        [SerializeField] private float asteroidSpawnRate = 2f;

        protected override void Awake()
        {
            RunManager.Instance.OnStartLevel += StartLevel;
            PlayerManager.Instance.OnPlayerHit += OnPlayerHit;
        }

        protected override void StartLevel(Level level)
        {
            base.StartLevel(level);

            //TODO: create a cinematic intro sequence using either do tween or a "movement pattern"
            StartBossBattle();
        }
        
        private void StartBossBattle()
        {
            boss.OnBossDestroyed += OnBossDefeated;
            //SETUP BOSS BEHAVIOR/TRIGGER IT
        }
        
        private void OnBossDefeated()
        {
            CompleteLevel();
        }
         
        protected override void OnLevelTimerComplete()
        {
            // Don't auto-complete on timer for boss levels
            // Boss must be defeated
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            if (boss != null)
                boss.OnBossDestroyed -= OnBossDefeated;
        }
    }
}