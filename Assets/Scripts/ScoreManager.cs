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
        }

        public void ToggleScoreTracking(bool on)
        {
            canvas.enabled = on;
            ResetScore();
        }

        public void ResetScore()
        {
            SetScore(0);
        }

        public void SetScore(int score)
        {
            Score = score;
            scoreText.text = $"Score: {Score}";
        }
    }
}