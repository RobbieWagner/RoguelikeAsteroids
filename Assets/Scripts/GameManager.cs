using System;
using System.Collections;
using RobbieWagnerGames.Utilities;
using UnityEngine;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
    public class GameManager : MonoBehaviourSingleton<GameManager>
    {
        private Coroutine gameStartCo = null;

        protected override void Awake()
        {
            base.Awake();
            StartCoroutine(SceneLoadManager.Instance.LoadSceneAdditive("MenuScene"));
        }

        public void PlayGameFromMenu()
        {
            if(gameStartCo == null)
                gameStartCo = StartCoroutine(PlayGameCo("MenuScene"));
        }

        public IEnumerator PlayGameCo(string sceneFrom)
        {
            yield return SceneLoadManager.Instance.UnloadScene(sceneFrom);
            ScoreManager.Instance.ResetScore();
            ScoreManager.Instance.ToggleScoreTracking(true);
            yield return SceneLoadManager.Instance.LoadSceneAdditive("GameScene");

            AsteroidManager.Instance.StartAsteroidSpawner();

            gameStartCo = null;
        }

        public void OnPlayerKilled()
        {
            AsteroidManager.Instance.StopAsteroidSpawner();
            AsteroidManager.Instance.DestroyAllAsteroids();

            ReloadGame(); // TODO prompt player
        }

        private void ReloadGame()
        {
            if(gameStartCo == null)
                StartCoroutine(PlayGameCo("GameScene"));
        }

        public void OnAsteroidShot(Shootable shootable)
        {
            ScoreManager.Instance.SetScore(ScoreManager.Instance.Score + 1);
        }
    }
}