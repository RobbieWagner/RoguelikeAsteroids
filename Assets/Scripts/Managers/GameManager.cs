using System;
using System.Collections;
using RobbieWagnerGames.Utilities;
using UnityEngine;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
    public class GameManager : MonoBehaviourSingleton<GameManager>
    {
        public event Action OnGameStart;
        public event Action OnGameOver;
        public event Action OnReturnToMenu;
        
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
            yield return SceneLoadManager.Instance.LoadSceneAdditive("GameScene");
            OnGameStart?.Invoke();

            sceneTransitionCo = null;
        }

        public void RestartGame()
        {
            if(sceneTransitionCo == null)
                sceneTransitionCo = StartCoroutine(RestartGameCo());
        }

        private IEnumerator RestartGameCo()
        {
            yield return SceneLoadManager.Instance.UnloadScene("GameScene");
            yield return SceneLoadManager.Instance.LoadSceneAdditive("GameScene");
            OnGameStart?.Invoke();
            
            sceneTransitionCo = null;
        }

        public void ReturnToMenu()
        {
            if(sceneTransitionCo == null)
                sceneTransitionCo = StartCoroutine(ReturnToMenuCo());
        }

        private IEnumerator ReturnToMenuCo()
        {
            yield return SceneLoadManager.Instance.UnloadScene("GameScene");
            
            OnReturnToMenu?.Invoke();
            
            yield return SceneLoadManager.Instance.LoadSceneAdditive("MenuScene");
            
            sceneTransitionCo = null;
        }

        public void ReloadGame()
        {
            RestartGame();
        }

        public void NotifyGameOver()
        {
            OnGameOver?.Invoke();
        }
    }
}