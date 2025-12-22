using UnityEngine;
using AYellowpaper.SerializedCollections;
using RobbieWagnerGames.Utilities;

namespace RobbieWagnerGames.Audio
{
    public enum AudioSourceName
    {
        UINav = 0,
        UISelect = 1,
        UIExit = 2,
        AsteroidDestroyed = 3,
        PointGain = 4,
        LeafGain = 5,
        FlowerGain = 6,
        Purchase = 7,
        Music = 8,
        UIFail = 9,
        ResourceCollected = 10
    }

    /// <summary>
    /// Manages audio playback for the game with support for multiple named audio sources
    /// </summary>
    public class BasicAudioManager : MonoBehaviourSingleton<BasicAudioManager>
    {
        [Header("Audio Sources")]
        [Tooltip("Mapping of audio source names to their respective AudioSource components")]
        [SerializeField][SerializedDictionary("Source Type", "Audio Source")] 
        private SerializedDictionary<AudioSourceName, AudioSource> audioSources = 
            new SerializedDictionary<AudioSourceName, AudioSource>();

        [Header("Settings")]
        [SerializeField] private bool persistBetweenScenes = true;
        [SerializeField] private float globalVolume = 1f;

        protected override void Awake()
        {
            base.Awake();
            
            if (persistBetweenScenes)
            {
                //DontDestroyOnLoad(gameObject);
            }

            UpdateGlobalVolume();
        }

        #region Audio Control
        /// <summary>
        /// Plays the specified audio source if it exists
        /// </summary>
        public void Play(AudioSourceName name)
        {
            if (TryGetAudioSource(name, out AudioSource source))
            {
                source.Play();
            }
        }

        /// <summary>
        /// Stops the specified audio source if it exists
        /// </summary>
        public void Stop(AudioSourceName name)
        {
            if (TryGetAudioSource(name, out AudioSource source))
            {
                source.Stop();
            }
        }

        /// <summary>
        /// Pauses the specified audio source if it exists
        /// </summary>
        public void Pause(AudioSourceName name)
        {
            if (TryGetAudioSource(name, out AudioSource source))
            {
                source.Pause();
            }
        }

        /// <summary>
        /// Sets whether the audio source should loop
        /// </summary>
        public void SetLooping(AudioSourceName name, bool shouldLoop)
        {
            if (TryGetAudioSource(name, out AudioSource source))
            {
                source.loop = shouldLoop;
            }
        }

        /// <summary>
        /// Sets the volume for a specific audio source
        /// </summary>
        /// <param name="volume">Volume between 0 and 1</param>
        public void SetSourceVolume(AudioSourceName name, float volume)
        {
            if (TryGetAudioSource(name, out AudioSource source))
            {
                source.volume = Mathf.Clamp01(volume) * globalVolume;
            }
        }

        /// <summary>
        /// Sets the global volume multiplier for all audio sources
        /// </summary>
        /// <param name="volume">Volume between 0 and 1</param>
        public void SetGlobalVolume(float volume)
        {
            globalVolume = Mathf.Clamp01(volume);
            UpdateGlobalVolume();
        }

        private void UpdateGlobalVolume()
        {
            foreach (var pair in audioSources)
            {
                if (pair.Value != null)
                {
                    pair.Value.volume = Mathf.Clamp01(pair.Value.volume) * globalVolume;
                }
            }
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Attempts to get an audio source by name
        /// </summary>
        /// <returns>True if the audio source exists and is valid</returns>
        public bool TryGetAudioSource(AudioSourceName name, out AudioSource source)
        {
            if (audioSources.TryGetValue(name, out source) && source != null)
            {
                return true;
            }

            Debug.LogWarning($"Audio source '{name}' not found or is null", this);
            return false;
        }

        /// <summary>
        /// Checks if an audio source exists and is valid
        /// </summary>
        public bool HasAudioSource(AudioSourceName name)
        {
            return audioSources.ContainsKey(name) && audioSources[name] != null;
        }
        #endregion
    }
}