using UnityEngine;
using RobbieWagnerGames;
using System;
using AYellowpaper.SerializedCollections;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
	public enum TrackLayer
	{
		NONE = -1,
		BASE,
		MIDDLE,
		TOP
	}

	public enum SongName
	{
		NONE = -1,
		MENU_THEME,
		SHOP_THEME,
		BOSS_THEME,
		LEVEL_THEME_1
	}

	[Serializable]
	public class Song
	{
		[SerializedDictionary("Layer","Audio Clip")]public SerializedDictionary<TrackLayer, AudioClip> tracks = new SerializedDictionary<TrackLayer, AudioClip>();
		public float duration;
	}
}