using UnityEngine;
using RobbieWagnerGames.Utilities;
using UnityEngine.UI;
using System;

namespace RobbieWagnerGames
{
	public class GameOverScreen : MonoBehaviour
	{
		[Header("Game Over")]
        [SerializeField] private Canvas gameOverScreen;
        [field: SerializeField] public Button retryButton { get; private set; }
		[field: SerializeField] public Button mainMenuButton { get; private set; }

        public void ToggleGameOverUI(bool on)
        {
            gameOverScreen.enabled = on;
        }
    }
}