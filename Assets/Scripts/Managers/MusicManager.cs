using UnityEngine;
using RobbieWagnerGames.Utilities;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
	public enum TrackNames
	{
		NONE = -1,
		MENU,
		GAME,
		GAME_OVER
	}
	public class MusicManager : MonoBehaviourSingleton<MusicManager>
	{
		protected override void Awake()
		{
			base.Awake();
		}
	}
}