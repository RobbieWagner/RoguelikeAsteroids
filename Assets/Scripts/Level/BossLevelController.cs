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
            Player.Instance?.DisableControls();
            StartBossBattle();
        }
        
        private void StartBossBattle()
        {
            boss.OnBossDestroyed += OnBossDefeated;
            boss.OnIntroPhaseComplete += OnIntroPhaseComplete;
            boss.OnOutroPhaseComplete += OnOutroPhaseComplete;
            boss.StartBoss();
        }
        
        private void OnIntroPhaseComplete()
        {
            Player.Instance?.EnableControls();
            ShootableManager.Instance.StartAsteroidSpawner();
        }
        
        private void OnOutroPhaseComplete()
        {
            CompleteLevel();
        }
        
        private void OnBossDefeated()
        {
            
        }
         
        protected override void OnLevelTimerComplete()
        {
            
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            if (boss != null)
            {
                boss.OnBossDestroyed -= OnBossDefeated;
                boss.OnIntroPhaseComplete -= OnIntroPhaseComplete;
                boss.OnOutroPhaseComplete -= OnOutroPhaseComplete;
            }
        }
    }
}