using UnityEngine;
using RobbieWagnerGames.Utilities;
using System;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
	public class PlayerManager : MonoBehaviourSingleton<PlayerManager>
	{
		public event Action<Player> OnPlayerHit;
		private Player currentPlayer;
		
		public void RegisterPlayer(Player player) => currentPlayer = player;

        protected override void Awake()
        {
			base.Awake();
            RunManager.Instance.OnStartLevel += ConfigurePlayerStats;
        }

        public void PlayerHit()
		{
			OnPlayerHit?.Invoke(currentPlayer);
		}

		private void ConfigurePlayerStats(Level level)
        {
			currentPlayer.speed -= RunManager.Instance.CurrentRun.speedModifier;
            currentPlayer.shooterCooldown -= RunManager.Instance.CurrentRun.fireCooldownModifier;
        
			currentPlayer.fireRange += RunManager.Instance.CurrentRun.fireRangeModifier;
			currentPlayer.bulletSpeed += RunManager.Instance.CurrentRun.bulletSpeedModifier;
		}
	}
}