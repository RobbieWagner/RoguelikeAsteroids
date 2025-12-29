using UnityEngine;
using RobbieWagnerGames.Utilities;
using System;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
	public class AsteroidsLevelController : LevelController
	{
		[SerializeField] private Timer levelTimer;
        public Timer LevelTimer => levelTimer;

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
            CompleteLevel();
        }

        protected override void UnsubscribeEvents()
        {
            base.UnsubscribeEvents();
			levelTimer.OnTimerComplete -= OnLevelTimerComplete;
        }
	}
}