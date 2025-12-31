using UnityEngine;
using RobbieWagnerGames.Utilities;
using AYellowpaper.SerializedCollections;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
	public class GameConstants : MonoBehaviourSingleton<GameConstants>
	{
		[SerializedDictionary("Resource","Color")] public SerializedDictionary<ResourceType, Color> resourceColors = new SerializedDictionary<ResourceType,Color>();
		[SerializedDictionary("Level Type","Color")] public SerializedDictionary<LevelType, Color> levelColors = new SerializedDictionary<LevelType,Color>();
		[SerializedDictionary("Level Type","Icon")] public SerializedDictionary<LevelType, Sprite> levelIcons = new SerializedDictionary<LevelType,Sprite>();
	}
}