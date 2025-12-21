using System;
using RobbieWagnerGames.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
    public class RunController : MonoBehaviourSingleton<RunController>
    {
        [SerializeField] private GameOverScreen gameOverScreen;

        protected override void Awake()
        {
            base.Awake();
            
            gameOverScreen.retryButton.onClick.AddListener(HandleRetry);
            gameOverScreen.mainMenuButton.onClick.AddListener(HandleMainMenu);
            
            GameManager.Instance.OnGameOver += ShowGameOverScreen;
            GameManager.Instance.OnGameStart += HideGameOverScreen;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        
            GameManager.Instance.OnGameOver -= ShowGameOverScreen;
            GameManager.Instance.OnGameStart -= HideGameOverScreen;
        
        }

        private void HandleRetry()
        {
            GameManager.Instance.ReloadGame();
        }

        private void HandleMainMenu()
        {
            GameManager.Instance.ReturnToMenu();
        }

        private void ShowGameOverScreen()
        {
            gameOverScreen.ToggleGameOverUI(true);
        }

        private void HideGameOverScreen()
        {
            gameOverScreen.ToggleGameOverUI(false);
        }
    }
}