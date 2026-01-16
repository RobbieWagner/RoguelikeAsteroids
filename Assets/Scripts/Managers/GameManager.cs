using System;
using System.Collections;
using System.Collections.Generic;
using RobbieWagnerGames.Audio;
using RobbieWagnerGames.Managers;
using RobbieWagnerGames.UI;
using RobbieWagnerGames.Utilities;
using UnityEngine;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
    public class GameManager : MonoBehaviourSingleton<GameManager>
    {
        public event Action OnGameStart;
        public event Action OnReturnToMenu;
        public event Action OnGameSaved;

        private bool isGamePaused = false;
        public bool IsGamePaused => isGamePaused;
        
        public event Action OnGamePaused;
        public event Action OnGameResumed;
        
        private Coroutine sceneTransitionCo = null;
        [SerializeField] private SettingsMenu settingsMenu = null;

        protected override void Awake()
        {
            base.Awake();
            LoadMenu();
        }

        private void LoadMenu()
        {
            Dictionary<string, float> audioMixerVolumes = new Dictionary<string, float>
            {
                { "main", PlayerPrefs.GetFloat("MasterVolume", 1f)},
                { "music", PlayerPrefs.GetFloat("MusicVolume", 1f)},
                { "ui", PlayerPrefs.GetFloat("UIVolume", 1f) },
                { "hazard", PlayerPrefs.GetFloat("HazardVolume", 1f) },
                { "player", PlayerPrefs.GetFloat("PlayerVolume", 1f) }  
            };
            AudioMixerController.Instance.UpdateAudioMixer(audioMixerVolumes);
            

            StartCoroutine(SceneLoadManager.Instance.LoadSceneAdditive("MenuScene", false));
        }

        public void StartGame()
        {
            if(sceneTransitionCo == null)
                sceneTransitionCo = StartCoroutine(StartGameCo());
        }

        private IEnumerator StartGameCo()
        {
            yield return SceneLoadManager.Instance.UnloadScene("MenuScene", false);
            yield return SceneLoadManager.Instance.LoadSceneAdditive("RunScene", true, () => { InvokeGameStartEvent(); });
            sceneTransitionCo = null;
        }

        private void InvokeGameStartEvent()
        {
            InputManager.Instance.EnableActionMap(ActionMapName.PAUSE);
            OnGameStart?.Invoke();
        }

        public void RestartGame()
        {
            if(sceneTransitionCo == null)
                sceneTransitionCo = StartCoroutine(RestartGameCo());
        }

        private IEnumerator RestartGameCo()
        {
            yield return SceneLoadManager.Instance.UnloadScene("RunScene", false);
            yield return SceneLoadManager.Instance.LoadSceneAdditive("RunScene", true, () => { InvokeGameStartEvent(); });
            sceneTransitionCo = null;
        }

        public void ReturnToMenu()
        {
            if(sceneTransitionCo == null)
                sceneTransitionCo = StartCoroutine(ReturnToMenuCo());
        }

        private IEnumerator ReturnToMenuCo()
        {
            yield return SceneLoadManager.Instance.UnloadScene("RunScene", false);
            yield return SceneLoadManager.Instance.LoadSceneAdditive("MenuScene", true, () => { InputManager.Instance.DisableActionMap(ActionMapName.PAUSE); });
            OnReturnToMenu?.Invoke();
            sceneTransitionCo = null;
        }

        public void PauseGame()
        {
            if (isGamePaused) return;
            
            isGamePaused = true;
            OnGamePaused?.Invoke();
            
            if (PauseMenu.Instance != null)
                PauseMenu.Instance.PauseGame();
        }
        
        public void ResumeGame()
        {
            if (!isGamePaused) return;
            
            isGamePaused = false;
            OnGameResumed?.Invoke();
            
            PauseMenu.Instance.ResumeGame();
        }
        
        public void TogglePause()
        {
            if (isGamePaused)
                ResumeGame();
            else
                PauseGame();
        }

        public void SaveGame()
        {
            OnGameSaved?.Invoke();
        }
    }
}