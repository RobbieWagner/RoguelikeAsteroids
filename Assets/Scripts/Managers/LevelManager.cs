using UnityEngine;
using RobbieWagnerGames.Utilities;
using System;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
    public class LevelManager : MonoBehaviourSingleton<LevelManager>
    {
        private Level level = null;
        public Level Level => level;
        
        private bool isLevelActive => level != null;

        public event Action<Level> OnLevelStarted;
        public event Action<Level> OnLevelCompleted;
        public event Action<Level> OnLevelFailed;

        [SerializeField] private Timer levelTimer;
        public Timer LevelTimer => levelTimer;
        
        protected override void Awake()
        {
            base.Awake();
            
            RunManager.Instance.OnStartNextLevel += StartNextLevel;
            
            GameManager.Instance.OnReturnToMenu += OnReturnedToMenu;

            levelTimer.OnTimerComplete += OnLevelTimerComplete;
        }

        private void OnReturnedToMenu()
        {
            level = null;
        }

        private void StartNextLevel(Level level)
        {
            this.level = level;
            
            ConfigureLevel(level);
            if (level.levelDuration > 0)
                levelTimer.StartTimer(level.levelDuration);
            OnLevelStarted?.Invoke(level);
        }

        private void FailLevel()
        {
            StartCoroutine(SceneLoadManager.Instance.UnloadScenes(new () {"AsteroidsScene", "ShopScene", "BossScene"},true,() => {RunManager.Instance.FailRun();},false));
            OnLevelFailed?.Invoke(level);
        }

        private void ConfigureLevel(Level level)
        {
            switch (level.levelType)
            {
                case LevelType.ASTEROIDS:
                    ConfigureAsteroidLevel(level);
                    break;
                case LevelType.SHOP:
                    ConfigureShopLevel();
                    break;
                case LevelType.BOSS:
                    ConfigureBossLevel();
                    break;
            }
        }

        private void ConfigureAsteroidLevel(Level level)
        {
            ShootableManager.Instance.ConfigureForLevel(level);
        }

        private void ConfigureShopLevel()
        {
            throw new NotImplementedException(); // TODO: add shop level implementation
        }

        private void ConfigureBossLevel()
        {            
            throw new NotImplementedException(); // TODO: add boss level implementation
        }

        private void OnLevelTimerComplete()
        {
            LevelComplete();
        }

        public void LevelComplete()
        {
            if (!isLevelActive) return;
            
            OnLevelCompleted?.Invoke(level);
            level = null;
            RunManager.Instance.CompleteCurrentLevel();
        }

        public void NotifyPlayerDeath()
        {
            if (!isLevelActive) return;
            
            level = null;
            levelTimer.StopTimer();
            FailLevel();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            RunManager.Instance.OnStartNextLevel -= StartNextLevel;
            GameManager.Instance.OnReturnToMenu -= OnReturnedToMenu;
        }
    }
}