using UnityEngine;
using RobbieWagnerGames.Utilities;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
	public class GameConstants : MonoBehaviourSingleton<GameConstants>
	{
		[SerializedDictionary("Resource","Color")] public SerializedDictionary<ResourceType, Color> resourceColors = new SerializedDictionary<ResourceType,Color>();
	}
}