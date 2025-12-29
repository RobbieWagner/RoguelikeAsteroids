using System;
using System.Collections;
using UnityEngine;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
    public class LevelController : MonoBehaviour
    {
        [SerializeField] protected bool failOnDeath;
        public Level levelDetails {get; protected set;}

        public event Action OnLevelStarted;
        public event Action OnLevelFailed;
        public event Action OnLevelCompleted;

        protected virtual void Awake()
        {
            RunManager.Instance.OnStartLevel += StartLevel;
            PlayerManager.Instance.OnPlayerDied += OnPlayerDied;
        }

        protected virtual void StartLevel(Level level)
        {
            ConfigureLevel(level);
            OnLevelStarted?.Invoke();
        }

        protected virtual void ConfigureLevel(Level level)
        {
            levelDetails = level;
        }

        protected virtual void CompleteLevel()
        {
            OnLevelCompleted?.Invoke();
            RunManager.Instance.CompleteCurrentLevel();
        }

        protected virtual void FailLevel()
        {
            OnLevelFailed?.Invoke();
            RunManager.Instance.FailRun();
        }

        public void OnPlayerDied(Player player)
        {
			if (failOnDeath)
				FailLevel();
			else
				CompleteLevel();
        }

        protected virtual void OnDisable()
        {
            UnsubscribeEvents();
        }

        protected virtual void OnDestroy()
        {
            UnsubscribeEvents();
        }

        protected virtual void UnsubscribeEvents()
        {
            RunManager.Instance.OnStartLevel -= ConfigureLevel;
        }
    }
}