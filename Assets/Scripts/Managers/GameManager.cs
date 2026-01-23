using System;
using System.Collections;
using System.Collections.Generic;
using RobbieWagnerGames.Audio;
using RobbieWagnerGames.Managers;
using RobbieWagnerGames.UI;
using RobbieWagnerGames.Utilities;
using RobbieWagnerGames.Utilities.SaveData;
using UnityEngine;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
    public class RoguelikeAsteroidsSaveData
    {
        public int victoryPoints = 1;
    }

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

        [HideInInspector] public RoguelikeAsteroidsSaveData currentSave; 

        protected override void Awake()
        {
            base.Awake();
            currentSave = JsonDataService.Instance.LoadDataRelative(GameConstants.GameData, new RoguelikeAsteroidsSaveData());
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
            SaveGame();
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
            JsonDataService.Instance.SaveData(GameConstants.GameData, currentSave);
            OnGameSaved?.Invoke();
        }
    }
}