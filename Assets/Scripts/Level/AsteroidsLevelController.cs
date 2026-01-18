using UnityEngine;
using RobbieWagnerGames.Utilities;
using System.Collections;
using System.Collections.Generic;
using System;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
	public class AsteroidsLevelController : LevelController
	{
		[SerializeField] private Timer levelTimer;
        public Timer LevelTimer => levelTimer;

		[SerializeField] private LevelCompleteScreen levelCompleteScreen;
        
        public Dictionary<ResourceType, int> collectedResources = new Dictionary<ResourceType, int>();

        [SerializeField] private Transform parallaxFollow;
        [SerializeField] private float parallaxSpeed;

        public event Action<ResourceType, int> OnResourceAdded;
        public event Action<Dictionary<ResourceType, int>> OnResourcesUpdated;

		protected override void Awake() 
		{
            base.Awake();
			levelTimer.OnTimerComplete += OnLevelTimerComplete;
		}

        protected override void Update()
        {
            base.Update();
            parallaxFollow.position = new Vector2(parallaxFollow.position.x, levelTimer.timerValue * -parallaxSpeed); 
        }

        protected override void StartLevel(Level level)
        {
            base.StartLevel(level);
			collectedResources.Clear();

			if(level.stopAtTimer)
				levelTimer.StartTimer(level.levelDuration);
        }
		
		protected virtual void OnLevelTimerComplete()
        {
			if(levelDetails.stopAtTimer)
            	CompleteLevel();
        }

        public override void CompleteLevel()
        {
            base.CompleteLevel();
            StartCoroutine(DisplayLevelCompletionScreen());
        }
		
		public override void FailLevel()
        {
            collectedResources.Clear();
            base.FailLevel();
        }

        private IEnumerator DisplayLevelCompletionScreen()
        {
			yield return levelCompleteScreen.DisplayScreen();
            RunManager.Instance.CompleteCurrentLevel();
        }

        protected override void UnsubscribeEvents()
        {
            base.UnsubscribeEvents();
			levelTimer.OnTimerComplete -= OnLevelTimerComplete;
        }

        public void OnResourceCollected(ResourceType resourceType, int amount)
        {
            if (resourceType == ResourceType.NONE || amount <= 0) return;
            
            if (collectedResources.ContainsKey(resourceType))
                collectedResources[resourceType] += amount;
            else
                collectedResources[resourceType] = amount;
            
            OnResourceAdded?.Invoke(resourceType, amount);
            OnResourcesUpdated?.Invoke(new Dictionary<ResourceType, int>(collectedResources));
        }
    }
}