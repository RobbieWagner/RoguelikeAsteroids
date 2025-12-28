using System;
using System.Collections;
using RobbieWagnerGames.Managers;
using RobbieWagnerGames.UI;
using RobbieWagnerGames.Utilities;
using UnityEngine;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
    public class GameManager : MonoBehaviourSingleton<GameManager>
    {
        public event Action OnGameStart;
        public event Action OnReturnToMenu;

        private bool isGamePaused = false;
        public bool IsGamePaused => isGamePaused;
        
        public event Action OnGamePaused;
        public event Action OnGameResumed;
        
        private Coroutine sceneTransitionCo = null;

        protected override void Awake()
        {
            base.Awake();
            LoadMenu();
        }

        private void LoadMenu()
        {
            StartCoroutine(SceneLoadManager.Instance.LoadSceneAdditive("MenuScene"));
        }

        public void StartGame()
        {
            if(sceneTransitionCo == null)
                sceneTransitionCo = StartCoroutine(StartGameCo());
        }

        private IEnumerator StartGameCo()
        {
            yield return SceneLoadManager.Instance.UnloadScene("MenuScene");
            yield return SceneLoadManager.Instance.LoadSceneAdditive("RunScene", () => { InvokeGameStartEvent(); });
            sceneTransitionCo = null;
        }

        private void InvokeGameStartEvent()
        {
            InputManager.Instance.EnableActionMap(ActionMapName.PAUSE);
            OnGameStart?.Invoke();
        }

        public void RestartGame()
        {
            if(sceneTransitionCo == null)
                sceneTransitionCo = StartCoroutine(RestartGameCo());
        }

        private IEnumerator RestartGameCo()
        {
            yield return SceneLoadManager.Instance.UnloadScene("RunScene");
            yield return SceneLoadManager.Instance.LoadSceneAdditive("RunScene", () => { InvokeGameStartEvent(); });
            sceneTransitionCo = null;
        }

        public void ReturnToMenu()
        {
            if(sceneTransitionCo == null)
                sceneTransitionCo = StartCoroutine(ReturnToMenuCo());
        }

        private IEnumerator ReturnToMenuCo()
        {
            yield return SceneLoadManager.Instance.UnloadScene("RunScene", false);
            yield return SceneLoadManager.Instance.UnloadScenes(new () {"AsteroidsScene", "ShopScene", "BossScene"}, false, null, false);
            OnReturnToMenu?.Invoke();
            
            yield return SceneLoadManager.Instance.LoadSceneAdditive("MenuScene", () => {InputManager.Instance.DisableActionMap(ActionMapName.PAUSE);});
            sceneTransitionCo = null;
        }

        public void PauseGame()
        {
            if (isGamePaused) return;
            
            isGamePaused = true;
            OnGamePaused?.Invoke();
            
            if (PauseMenu.Instance != null)
                PauseMenu.Instance.PauseGame();
        }
        
        public void ResumeGame()
        {
            if (!isGamePaused) return;
            
            isGamePaused = false;
            OnGameResumed?.Invoke();
            
            PauseMenu.Instance.ResumeGame();
        }
        
        public void TogglePause()
        {
            if (isGamePaused)
                ResumeGame();
            else
                PauseGame();
        }
    }
}