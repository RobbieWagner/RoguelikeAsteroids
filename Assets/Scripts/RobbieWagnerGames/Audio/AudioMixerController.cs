using System.Collections.Generic;
using RobbieWagnerGames.Utilities;
using UnityEngine;
using UnityEngine.Audio;

namespace RobbieWagnerGames.Audio
{
    public enum AudioMixerGroupName
    {
        NONE = -1,
        MAIN,
        PLAYER,
        UI,
        MUSIC,
        HAZARD,

    }

    public class AudioMixerController : MonoBehaviourSingleton<AudioMixerController>
    {
        [SerializeField] private AudioMixer audioMixer; 
        
        public void UpdateAudioMixer(Dictionary<string, float> audioMixerVolumes)
        {
            foreach (KeyValuePair<string, float> volume in audioMixerVolumes)
                audioMixer.SetFloat(volume.Key, Mathf.Lerp(-40, 0, volume.Value));
        }
    }
}