using UnityEngine;
using RobbieWagnerGames.Utilities;
using AYellowpaper.SerializedCollections;
using System.Collections.Generic;
using System;
using System.Linq;
using DG.Tweening;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
	public class MusicManager : MonoBehaviourSingleton<MusicManager>
	{
		[SerializeField][SerializedDictionary("Song Name","Song")] private SerializedDictionary<SongName, Song> songs = new SerializedDictionary<SongName, Song>();
		[SerializeField][SerializedDictionary("Layer","Audio Source")] private SerializedDictionary<TrackLayer, AudioSource> layeredAudioSources = new SerializedDictionary<TrackLayer, AudioSource>();
		private Song currentSong = null;

		private Sequence activeTrackSettingSequence;

		protected override void Awake()
		{
			base.Awake();
		}

		public void SwitchSong(SongName songName, List<TrackLayer> activeTracks, bool playImmediately = false)
		{
			currentSong = songs[songName];

			foreach (KeyValuePair<TrackLayer, AudioClip> track in currentSong.tracks)
				layeredAudioSources[track.Key].clip = track.Value;

			if (playImmediately)
				PlayCurrentSong();
			
			SetActiveTracks(activeTracks);
		}

		public void SetActiveTracks(List<TrackLayer> tracks, float transitionTime = -1)
		{
			List<TrackLayer> tracksToMute = Enum.GetValues(typeof(TrackLayer))
					.Cast<TrackLayer>()
					.Where(e => !tracks.Contains(e) && e != TrackLayer.NONE)
					.ToList();
				
				List<TrackLayer> tracksToUnmute = tracks
					.Where(t => layeredAudioSources.Keys.Contains(t))
					.ToList();
			
			if (transitionTime > 0)
			{
				activeTrackSettingSequence?.Kill();
				activeTrackSettingSequence = DOTween.Sequence();
				foreach(TrackLayer track in tracksToMute)
				{
					activeTrackSettingSequence.Join(layeredAudioSources[track].DOFade(0, transitionTime));
					activeTrackSettingSequence.AppendCallback(() => layeredAudioSources[track].mute = true);
				}
				foreach(TrackLayer track in tracksToUnmute)
				{
					activeTrackSettingSequence.Join(layeredAudioSources[track].DOFade(1, transitionTime));
					activeTrackSettingSequence.AppendCallback(() => layeredAudioSources[track].mute = false);
				}
			}
			else
			{
				foreach(TrackLayer track in tracksToMute)
				{
					layeredAudioSources[track].mute = true;
					layeredAudioSources[track].volume = 0;
				}
				foreach(TrackLayer track in tracksToUnmute)
				{
					layeredAudioSources[track].mute = false;
					layeredAudioSources[track].volume = 1;
				}
			}
		}

		public void PlayCurrentSong()
		{
			foreach(AudioSource audioSource in layeredAudioSources.Values)
				audioSource.Play();
		}

		public void ResumeCurrentSong()
		{
			foreach(AudioSource audioSource in layeredAudioSources.Values)
				audioSource.Play((ulong) audioSource.time);
		}

		public void PauseCurrentSong()
		{
			foreach(AudioSource audioSource in layeredAudioSources.Values)
				audioSource.Pause();
		}

		public void StopCurrentSong()
		{
			foreach(AudioSource audioSource in layeredAudioSources.Values)
			{
				audioSource.Stop();
				audioSource.clip = null;
			}
		}
	}
}