using System;
using UnityEngine;
using UnityEngine.UI;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button exitButton;

        private void Awake() {
            playButton.onClick.AddListener(GameManager.Instance.PlayGameFromMenu);
            settingsButton.onClick.AddListener(OpenSettingsMenu);
            exitButton.onClick.AddListener(QuitApplication);
        }

        private void OpenSettingsMenu()
        {
            throw new NotImplementedException();
        }

        private void QuitApplication()
        {
            Application.Quit();
        }

        private void OnDestroy()
        {
            playButton.onClick.RemoveAllListeners();
            settingsButton.onClick.RemoveAllListeners();
            exitButton.onClick.RemoveAllListeners();
        }
    }
}