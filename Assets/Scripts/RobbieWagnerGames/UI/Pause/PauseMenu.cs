using RobbieWagnerGames.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RobbieWagnerGames.UI
{
    public class PauseMenu : Menu
    {
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button controlsButton;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button quitButton;

        [SerializeField] private Menu settings;
        [SerializeField] private Menu controls;

        [HideInInspector] public bool canPause;
        [HideInInspector] public bool paused;

        private List<InputActionMap> pausedActionMaps;

        public static PauseMenu Instance {get; private set;}

        protected override void Awake()
        {
            if (Instance != null && Instance != this) 
            { 
                Destroy(gameObject); 
            } 
            else 
            { 
                Instance = this; 
            } 

            canPause = true;
            paused = false;

            pausedActionMaps = new List<InputActionMap>();
        } 

        protected override void OnEnable()
        {
            base.OnEnable();

            paused = true;
            Time.timeScale = 0;

            resumeButton.onClick.AddListener(ResumeGame);
            if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
            //controlsButton.onClick.AddListener(OpenControls);
            saveButton.onClick.AddListener(SaveGame);
            quitButton.onClick.AddListener(QuitToMainMenu);

            InputManager.Instance.Controls.PAUSE.UnpauseGame.performed += PauseMenuWatch.Instance.DisablePauseMenu;
            InputManager.Instance.Controls.PAUSE.PauseGame.performed += PauseMenuWatch.Instance.DisablePauseMenu;
            InputManager.Instance.EnableActionMap(ActionMapName.PAUSE);

            canvas.enabled = true;

            foreach (InputActionMap actionMap in InputManager.Instance.Controls.asset.actionMaps)
            {
                if (actionMap.enabled && !actionMap.name.Equals(ActionMapName.PAUSE.ToString(), StringComparison.CurrentCultureIgnoreCase) && !actionMap.name.Equals(ActionMapName.UI.ToString(), StringComparison.CurrentCultureIgnoreCase))
                {
                    bool success = Enum.TryParse(actionMap.name, out ActionMapName actionMapName);
                    if (success)
                    {
                        InputManager.Instance.DisableActionMap(actionMapName);
                    }
                }
            }

            OnGamePaused?.Invoke();
        }

        public delegate void OnGamePausedDelegate();
        public event OnGamePausedDelegate OnGamePaused;

        protected override void OnDisable()
        {
            base.OnDisable();

            if(!paused) 
            {
                Time.timeScale = 1;

                OnGameUnpaused?.Invoke();
            }

            resumeButton.onClick.RemoveListener(ResumeGame);
            if (settingsButton != null) settingsButton.onClick.RemoveListener(OpenSettings);
            //controlsButton.onClick.RemoveListener(OpenControls);
            saveButton.onClick.RemoveListener(SaveGame);
            quitButton.onClick.RemoveListener(QuitToMainMenu);

            InputManager.Instance.Controls.PAUSE.UnpauseGame.performed -= PauseMenuWatch.Instance.DisablePauseMenu;
            InputManager.Instance.Controls.PAUSE.PauseGame.performed -= PauseMenuWatch.Instance.DisablePauseMenu;

            foreach (string map in pausedActionMaps.Select(x => x.name))
            {
                if (!map.Equals(ActionMapName.PAUSE.ToString()))
                {
                    bool success = Enum.TryParse(map, out ActionMapName actionMap);
                    if (success) 
                    {
                        InputManager.Instance.EnableActionMap(actionMap);
                    }
                }
            }
            pausedActionMaps.Clear();

            canvas.enabled = false;
        }

        public delegate void OnGameUnpausedDelegate();
        public event OnGameUnpausedDelegate OnGameUnpaused;

        public void ResumeGame()
        {
            paused = false;
            enabled = false;
        }

        private void OpenSettings()
        {
            StartCoroutine(SwapMenu(this, settings));
        }

        private void OpenControls()
        {
            StartCoroutine(SwapMenu(this, controls));
        }

        protected virtual void SaveGame()
        {
            //GameManager.Instance.SaveGameData();
        }

        protected virtual void OnSaveButtonComplete()
        {

        }

        private void QuitToMainMenu()
        {
            ToggleButtonInteractibility(false);

            StartCoroutine(QuitToMainMenuCo());
        }

        protected override void ToggleButtonInteractibility(bool toggleOn)
        {
            base.ToggleButtonInteractibility(toggleOn);

            resumeButton.interactable = toggleOn;
            if (settingsButton != null) settingsButton.interactable = toggleOn;
            //controlsButton.interactable = toggleOn;
            saveButton.interactable = toggleOn;
            quitButton.interactable = toggleOn;
        }

        private IEnumerator QuitToMainMenuCo()
        {
            yield return new WaitForSecondsRealtime(.1f);
            Time.timeScale = 1;
            SceneManager.LoadScene("MainMenu");

            StopCoroutine(QuitToMainMenuCo());
        }

        protected override IEnumerator SwapMenu(Menu active, Menu next, bool setAsLastMenu = true)
        {
            InputManager.Instance.DisableActionMap(ActionMapName.PAUSE);

            yield return StartCoroutine(base.SwapMenu(active, next));

            StopCoroutine(SwapMenu(active, next));
        }
    }
}