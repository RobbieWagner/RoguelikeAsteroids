using UnityEngine;
using RobbieWagnerGames.Utilities;
using System.Collections;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
	public class AsteroidsLevelController : LevelController
	{
		[SerializeField] private Timer levelTimer;
        public Timer LevelTimer => levelTimer;

		[SerializeField] private LevelCompleteScreen levelCompleteScreen;

		protected override void Awake() 
		{
            base.Awake();
			levelTimer.OnTimerComplete += OnLevelTimerComplete;
		}

        protected override void StartLevel(Level level)
        {
            base.StartLevel(level);

			if(level.stopAtTimer)
				levelTimer.StartTimer(level.levelDuration);
        }
		
		private void OnLevelTimerComplete()
        {
			if(levelDetails.stopAtTimer)
            	CompleteLevel();
        }

        protected override void CompleteLevel()
        {
            base.CompleteLevel();
            StartCoroutine(DisplayLevelCompletionScreen());
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
	}
}