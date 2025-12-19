using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace RobbieWagnerGames.UI
{
    public class SoundSlider : MonoBehaviour
    {
        [HideInInspector] public Slider volumeSlider;
        private float volume;
        [SerializeField] private float defaultValue = -5f;
        [SerializeField] private string exposedParameterName;
        [HideInInspector] private AudioMixer audioMixer;

        private void Awake()
        {
            volumeSlider.onValueChanged.AddListener(AdjustVolume);
            LoadVolume();
        }

        public void AdjustVolume(float value)
        {
            if (value < -40) value = -80;
                volume = value;
            audioMixer.SetFloat(exposedParameterName, volume);
            SaveVolume();
        }

        public void SaveVolume()
        {
            PlayerPrefs.SetFloat(exposedParameterName, volume);
        }

        public void LoadVolume()
        {
            volume = PlayerPrefs.GetFloat(exposedParameterName, defaultValue);
            if (volume < -40) volume = -80;
                audioMixer.SetFloat(exposedParameterName, volume);
        }
    }
}