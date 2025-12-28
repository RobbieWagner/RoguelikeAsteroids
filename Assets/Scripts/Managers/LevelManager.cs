using UnityEngine;
using RobbieWagnerGames.Utilities;
using System;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
    public class LevelManager : MonoBehaviourSingleton<LevelManager>
    {
        private Level level = null;
        public Level Level => level;

        [SerializeField] private GameOverScreen gameOverScreen;
        
        private bool isLevelActive => level != null;

        public event Action<Level> OnLevelStarted;
        public event Action<Level> OnLevelCompleted;
        public event Action<Level> OnLevelFailed;
        
        protected override void Awake()
        {
            base.Awake();
            
            RunManager.Instance.OnStartNextLevel += StartNextLevel;
            RunManager.Instance.OnRunFailed += OnRunFailed;
            
            GameManager.Instance.OnReturnToMenu += OnReturnedToMenu;
        }

        private void OnReturnedToMenu()
        {
            level = null;
        }

        private void StartNextLevel(Level level)
        {
            this.level = level;
            
            ConfigureLevel(level);
            OnLevelStarted?.Invoke(level);
        }

        public void CompleteLevel(Level level)
        {
            //TODO: call from somewhere
            level = null;
            OnLevelCompleted?.Invoke(level);
            RunManager.Instance.CompleteCurrentLevel();
        }

        private void OnRunFailed(Run run)
        {
            level = null;
            
            gameOverScreen.ToggleGameOverUI(true);
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
            if (ShootableManager.Instance != null)
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

        public void LevelComplete(int score, int resources)
        {
            if (!isLevelActive) return;
            
            level = null;
            RunManager.Instance.CompleteCurrentLevel();
        }

        public void NotifyPlayerDeath()
        {
            if (!isLevelActive) return;
            
            level = null;
            OnLevelFailed?.Invoke(level);
            RunManager.Instance.FailCurrentLevel();
        }

        public void HandleRetry()
        {
            if (gameOverScreen != null)
                gameOverScreen.ToggleGameOverUI(false);
            RunManager.Instance.RestartGame();
        }

        public void HandleMainMenu()
        {
            if (gameOverScreen != null)
                gameOverScreen.ToggleGameOverUI(false);
            RunManager.Instance.ReturnToMainMenu();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            if (RunManager.Instance != null)
            {
                RunManager.Instance.OnStartNextLevel -= StartNextLevel;
                RunManager.Instance.OnRunFailed -= OnRunFailed;
            }
            
            if (GameManager.Instance != null)
                GameManager.Instance.OnReturnToMenu -= OnReturnedToMenu;
        }
    }
}