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
		
		public void PlayerHit()
		{
			OnPlayerHit?.Invoke(currentPlayer);
		}
	}
}