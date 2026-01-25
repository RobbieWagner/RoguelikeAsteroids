using UnityEngine;
using UnityEngine.UI;
using RobbieWagnerGames.UI;
using RobbieWagnerGames.Audio;
using TMPro;
using RobbieWagnerGames.Utilities.SaveData;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
    public partial class MainMenu : Menu
    {
        [SerializeField] private Button continueButton;
        [SerializeField] private Button newRunButton;
        [SerializeField] private Button purgeDataButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button exitButton;
        public override bool IsOpen => base.IsOpen && !isSettingsOpen;

        private bool hasExistingRun = false;

        protected override void Awake() 
        {
            base.Awake();
            
            hasExistingRun = JsonDataService.Instance.LoadDataRelative<Run>(GameConstants.RunPath, null) != null;
            
            continueButton.onClick.AddListener(OnContinueButtonClicked);
            newRunButton.onClick.AddListener(OnNewRunButtonClicked);
            purgeDataButton.onClick.AddListener(OnPurgeDataButtonClicked);
            settingsButton.onClick.AddListener(OnSettingsButtonClicked);
            exitButton.onClick.AddListener(OnExitButtonClicked);
            
            continueButton.gameObject.SetActive(hasExistingRun);

            settingsMenuPanel.SetActive(false);

            firstSelected = hasExistingRun ? continueButton : newRunButton;
            
            Open();
            activeMenu = this;
        }

        private void OnContinueButtonClicked()
        {
            BasicAudioManager.Instance.Play(AudioSourceName.UISelect);
            Close();
            GameManager.Instance.StartGame();
        }

        private void OnNewRunButtonClicked()
        {
            BasicAudioManager.Instance.Play(AudioSourceName.UISelect);
            
            if (hasExistingRun)
            {
                PromptManager.Instance.ShowConfirmationPrompt(
                    "Start New Run?",
                    "This will delete your current run progress. Are you sure?",
                    () => 
                    {
                        JsonDataService.Instance.DeleteData(GameConstants.RunPath);
                        Close();
                        GameManager.Instance.StartGame();
                    },
                    null
                );
            }
            else
            {
                Close();
                GameManager.Instance.StartGame();
            }
        }

        private void OnPurgeDataButtonClicked()
        {
            BasicAudioManager.Instance.Play(AudioSourceName.UISelect);
            PromptManager.Instance.ShowConfirmationPrompt(
                "Purge All Save Data?",
                "This will delete ALL save data including victory points and run progress. Are you sure?",
                () => 
                {
                    bool success = JsonDataService.Instance.PurgeData();
                    if (success)
                    {
                        GameManager.Instance.currentSave = new RoguelikeAsteroidsSaveData();
                        GameManager.Instance.SaveGame();
                        
                        hasExistingRun = false;
                        continueButton.gameObject.SetActive(false);
                        firstSelected = newRunButton;
                        SetupNavigation();
                    }
                },
                null
            );
        }

        private void OnSettingsButtonClicked()
        {
            BasicAudioManager.Instance.Play(AudioSourceName.UISelect);
            OpenSettingsMenu();
        }

        private void OnExitButtonClicked()
        {
            BasicAudioManager.Instance.Play(AudioSourceName.UIExit);
            PromptManager.Instance.ShowConfirmationPrompt(
                "Are You Sure?",
                "Are you sure you would like to close the application?",
                () => QuitApplication(),
                null
            );
        }

        private void QuitApplication()
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }

        protected override void HandleCancel()
        {
            if (settingsMenuPanel.activeSelf)
            {
                BasicAudioManager.Instance.Play(AudioSourceName.UIExit);
                CloseSettingsMenu();
            }
        }

        protected override void OnElementSelected(Selectable element)
        {
            base.OnElementSelected(element);
            BasicAudioManager.Instance.Play(AudioSourceName.UINav, false);
        }

        protected override void OnElementSubmitted(Selectable element)
        {
            base.OnElementSubmitted(element);
            BasicAudioManager.Instance.Play(AudioSourceName.UISelect);
        }

        protected override void OnDestroy()
        {   
            base.OnDestroy();

            continueButton.onClick.RemoveAllListeners();
            newRunButton.onClick.RemoveAllListeners();
            purgeDataButton.onClick.RemoveAllListeners();
            settingsButton.onClick.RemoveAllListeners();
            exitButton.onClick.RemoveAllListeners();
            
            if (settingsMenu.backButton != null)
                settingsMenu.backButton.onClick.RemoveAllListeners();
        }
    }
}