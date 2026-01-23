using UnityEngine;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
	public class ShopLevelController : LevelController
	{
        [SerializeField] private ShopMenu menu;

        protected override void ConfigureLevel(Level level)
        {
            base.ConfigureLevel(level);
            menu.Open();
        }

        public override void CompleteLevel()
        {
            base.CompleteLevel();
            RunManager.Instance.CompleteCurrentLevel();
        }
	}
}