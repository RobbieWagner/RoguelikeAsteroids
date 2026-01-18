using System;
using System.Collections;
using DG.Tweening;
using RobbieWagnerGames.Audio;
using RobbieWagnerGames.Managers;
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

        public Sequence screenShakeSequence;

        protected virtual void Awake()
        {
            RunManager.Instance.OnStartLevel += StartLevel;
            PlayerManager.Instance.OnPlayerHit += OnPlayerHit;
        }

        protected void OnPlayerHit(Player player)
        {
            RunManager.Instance.CurrentRun.health--;
            if (RunManager.Instance.CurrentRun.health == 0)
                OnPlayerDied();
            else
            {
                BasicAudioManager.Instance.Play(AudioSourceName.PlayerHit);
                player.DisableColliderTemporarily(2f);
                ShakeScreen();
            }
            
        }

        protected virtual void ShakeScreen()
        {
            screenShakeSequence?.Kill();
            screenShakeSequence = DOTween.Sequence();
            screenShakeSequence.Append(Camera.main.DOShakePosition(1, Vector3.right/2));
        }

        protected virtual void Update()
        {
            
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

        public virtual void CompleteLevel()
        {
            InputManager.Instance.DisableActionMap(ActionMapName.GAME);
            OnLevelCompleted?.Invoke();
        }

        public virtual void FailLevel()
        {
            OnLevelFailed?.Invoke();
            InputManager.Instance.DisableActionMap(ActionMapName.GAME);
            StartCoroutine(FailLevelCo());
        }

        protected virtual IEnumerator FailLevelCo()
        {
            yield return new WaitForSeconds(1f);
            RunManager.Instance.FailRun();
        }

        public void OnPlayerDied()
        {
            BasicAudioManager.Instance.Play(AudioSourceName.PlayerDestroyed);
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