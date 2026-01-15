using UnityEngine;
using RobbieWagnerGames;
using System;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
	public class ShopLevelController : LevelController
	{
        protected override void ConfigureLevel(Level level)
        {
            base.ConfigureLevel(level);
        }

        public override void CompleteLevel()
        {
            base.CompleteLevel();
            RunManager.Instance.CompleteCurrentLevel();
        }
	}
}