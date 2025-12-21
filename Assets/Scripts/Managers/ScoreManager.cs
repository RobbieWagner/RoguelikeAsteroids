using System;
using RobbieWagnerGames.Utilities;
using TMPro;
using UnityEngine;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
    public class ScoreManager : MonoBehaviourSingleton<ScoreManager>
    {
        public int Score {get; private set;}
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private Canvas canvas;

        protected override void Awake() 
        {
            base.Awake();

            GameManager.Instance.OnGameStart += EnableScoreTracking;
            GameManager.Instance.OnGameOver += DisableScoreTracking;
            
            var asteroidManager = AsteroidManager.Instance;
            if (asteroidManager != null)
                asteroidManager.AsteroidDestroyedEvent += OnAsteroidDestroyed;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStart -= EnableScoreTracking;
                GameManager.Instance.OnGameOver -= DisableScoreTracking;
            }
            
            var asteroidManager = AsteroidManager.Instance;
            if (asteroidManager != null)
                asteroidManager.AsteroidDestroyedEvent -= OnAsteroidDestroyed;
        }

        private void EnableScoreTracking()
        {
            canvas.enabled = true;
            ResetScore();
        }

        private void DisableScoreTracking()
        {
            canvas.enabled = false;
        }

        private void OnAsteroidDestroyed(Shootable asteroid, DestructionReason reason)
        {
            if (reason == DestructionReason.BULLET_HIT)
                AddScore(1);
        }

        public void ResetScore()
        {
            SetScore(0);
        }

        public void SetScore(int score)
        {
            Score = score;
            UpdateScoreDisplay();
        }

        public void AddScore(int amount)
        {
            Score += amount;
            UpdateScoreDisplay();
        }

        private void UpdateScoreDisplay()
        {
            if (scoreText != null)
                scoreText.text = $"Score: {Score}";
        }
    }
}