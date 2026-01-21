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
            Player.Instance?.DisableControls();
            StartBossBattle();
            ConfigureLevel(level);
        }
        
        private void StartBossBattle()
        {
            boss.OnBossDefeated += OnBossDefeated;
            boss.OnIntroPhaseComplete += OnIntroPhaseComplete;
            boss.OnOutroPhaseComplete += OnOutroPhaseComplete;
            boss.StartBoss();
        }
        
        private void OnIntroPhaseComplete()
        {
            StartCoroutine(PlayerHUD.Instance.DisplayLevelStart(() => 
            {
                Player.Instance?.EnableControls();
                ShootableManager.Instance.StartAsteroidSpawner();
                boss.CheckHealthThresholdTransitions();
                InvokeLevelStartAction();
            }));
        }
        
        private void OnOutroPhaseComplete()
        {
            CompleteLevel();
        }
        
        private void OnBossDefeated()
        {
            ShootableManager.Instance.StopAndClearAllShootables();
        }
         
        protected override void OnLevelTimerComplete()
        {
            
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            if (boss != null)
            {
                boss.OnBossDefeated -= OnBossDefeated;
                boss.OnIntroPhaseComplete -= OnIntroPhaseComplete;
                boss.OnOutroPhaseComplete -= OnOutroPhaseComplete;
            }
        }
    }
}