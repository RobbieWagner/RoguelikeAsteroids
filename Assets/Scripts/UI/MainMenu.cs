using UnityEngine;
using UnityEngine.UI;
using RobbieWagnerGames.UI;
using RobbieWagnerGames.Audio;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
    public partial class MainMenu : Menu
    {
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button exitButton;

        protected override void Awake() 
        {
            base.Awake();
            
            playButton.onClick.AddListener(OnPlayButtonClicked);
            settingsButton.onClick.AddListener(OnSettingsButtonClicked);
            exitButton.onClick.AddListener(OnExitButtonClicked);
        
            settingsMenuPanel.SetActive(false);
        
            firstSelected = playButton;

            Open();
        }

        private void OnPlayButtonClicked()
        {
            BasicAudioManager.Instance.Play(AudioSourceName.UISelect);
            Close();
            GameManager.Instance.StartGame();
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
            BasicAudioManager.Instance.Play(AudioSourceName.UINav);
        }

        protected override void OnElementSubmitted(Selectable element)
        {
            base.OnElementSubmitted(element);
            BasicAudioManager.Instance.Play(AudioSourceName.UISelect);
        }

        protected override void OnDestroy()
        {   
            base.OnDestroy();

            playButton.onClick.RemoveAllListeners();
            settingsButton.onClick.RemoveAllListeners();
            exitButton.onClick.RemoveAllListeners();
            
            if (settingsBackButton != null)
                settingsBackButton.onClick.RemoveAllListeners();
        }
    }
}