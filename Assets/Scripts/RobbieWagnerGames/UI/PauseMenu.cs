using UnityEngine;
using UnityEngine.UI;
using RobbieWagnerGames.Managers;
using System.Collections;
using RobbieWagnerGames.Audio;
using UnityEngine.InputSystem;
using RobbieWagnerGames.RoguelikeAsteroids;

namespace RobbieWagnerGames.UI
{
    public partial class PauseMenu : Menu
    {
        private static PauseMenu _instance;
        public static PauseMenu Instance => _instance;

        [SerializeField] private Button resumeButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button mainMenuButton;
        
        [SerializeField] private bool pauseTime = true;
        
        private bool isPaused = false;

        protected override void Awake()
        {
            if (_instance != null && _instance != this)
            {
				Debug.Log("destroyed");
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            base.Awake();
            
            resumeButton.onClick.AddListener(ResumeGame);
            settingsButton.onClick.AddListener(OpenSettings);
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
            
            firstSelected = resumeButton;
            
            Close();
        }
        
        protected override void OnEnable()
        {
            base.OnEnable();
            
            InputManager.Instance.Controls.PAUSE.PauseGame.performed += OnPauseGamePerformed;
            
            InputManager.Instance.Controls.PAUSE.UnpauseGame.performed += OnUnpauseGamePerformed;
        }
        
        protected override void OnDisable()
        {
            base.OnDisable();

            InputManager.Instance.Controls.PAUSE.PauseGame.performed -= OnPauseGamePerformed;
            
            InputManager.Instance.Controls.PAUSE.UnpauseGame.performed -= OnUnpauseGamePerformed;
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            if (_instance == this)
                _instance = null;
            
            resumeButton.onClick.RemoveAllListeners();
            settingsButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.RemoveAllListeners();
        }
        
        private void OnPauseGamePerformed(InputAction.CallbackContext context)
        {
            if (!isPaused)
                PauseGame();
        }
        
        private void OnUnpauseGamePerformed(InputAction.CallbackContext context)
        {
            if (isPaused)
                ResumeGame();
        }
        
        public void PauseGame()
        {
            if (isPaused) return;

            isPaused = true;
            
            if (pauseTime)
                Time.timeScale = 0f;
            
            InputManager.Instance.SaveAndDisableCurrentActionMaps();
            InputManager.Instance.EnableActionMap(ActionMapName.UI);
            
            Open();
        }
        
        public void ResumeGame()
        {
            if (!isPaused) return;

            isPaused = false;
            
            Close();
            
            if (pauseTime)
                Time.timeScale = 1f;
            
            InputManager.Instance.RestoreReservedActionMaps();
            EventSystemManager.Instance.ClearSelection();
        }
        
        private void ReturnToMainMenu()
        {
            Debug.Log("Returning to main menu");
            
            if (pauseTime)
                Time.timeScale = 1f;
            
            Close();
            isPaused = false;

            PromptManager.Instance.ShowConfirmationPrompt(
                "Are you sure?",
                "Any unsaved progress will be lost. Are you sure you would like to quit?",
                () => { RunManager.Instance.ReturnToMainMenu(); },
                null
            );
        }
        
        public override void Open()
        {
            base.Open();
            
            resumeButton.interactable = true;
            settingsButton.interactable = true;
            mainMenuButton.interactable = true;
            
            if (firstSelected != null)
                StartCoroutine(DelayedForceSelect(firstSelected));
        }
        
        public override void Close()
        {
            base.Close();
            EventSystemManager.Instance.ClearSelection();
        }
        
        protected override void HandleCancel()
        {
            if (IsOpen)
                ResumeGame();
        }
        
        protected override void OnElementSelected(Selectable element)
        {
            base.OnElementSelected(element);
            BasicAudioManager.Instance.Play(AudioSourceName.UINav);
        }
        
        protected override void OnElementSubmitted(Selectable element)
        {
            base.OnElementSubmitted(element);
            BasicAudioManager.Instance.Play(AudioSourceName.UISelect);
        }

        private IEnumerator DelayedForceSelect(Selectable element)
        {
            yield return new WaitForSecondsRealtime(.01f);
            ForceSelectElement(element);
        }
        
        public bool IsGamePaused => isPaused;
        
        public static void TogglePause()
        {
            if (Instance.isPaused)
                Instance.ResumeGame();
            else
                Instance.PauseGame();
        }
    }
}