using System.Collections;
using RobbieWagnerGames.Managers;
using RobbieWagnerGames.UI;
using UnityEngine;
using UnityEngine.UI;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
    public partial class MainMenu : Menu
    {
        [Header("Settings Menu")]
        [SerializeField] private GameObject settingsMenuPanel;
        [SerializeField] private Button settingsBackButton;
        private Selectable previouslySelected = null;
        private bool isSettingsOpen = false;
        private Coroutine settingsToggleCoroutine;

        private void OpenSettingsMenu()
        {
            if (isSettingsOpen) return;
            
            if (settingsToggleCoroutine == null)
                settingsToggleCoroutine = StartCoroutine(OpenSettingsMenuCoroutine());
        }

        private IEnumerator OpenSettingsMenuCoroutine()
        {
            yield return null;

            isSettingsOpen = true;

            previouslySelected = EventSystemManager.Instance?.CurrentSelected?.GetComponent<Selectable>();

            playButton.gameObject.SetActive(false);
            settingsButton.gameObject.SetActive(false);
            exitButton.gameObject.SetActive(false);
            
            settingsMenuPanel.SetActive(true);
            
            if (settingsBackButton != null)
            {
                settingsBackButton.onClick.RemoveAllListeners();
                settingsBackButton.onClick.AddListener(CloseSettingsMenu);

                firstSelected = settingsBackButton;
                RefreshSelectableElements();
                SetupNavigation();
            }

            settingsToggleCoroutine = null;
        }

        private void CloseSettingsMenu()
        {
            if (!isSettingsOpen) return;

            if (settingsToggleCoroutine == null)
                settingsToggleCoroutine = StartCoroutine(CloseSettingsMenuCoroutine());
        }

        private IEnumerator CloseSettingsMenuCoroutine()
        {
            yield return null;

            Debug.Log("close");
            isSettingsOpen = false;

            settingsMenuPanel.SetActive(false);
            
            playButton.gameObject.SetActive(true);
            settingsButton.gameObject.SetActive(true);
            exitButton.gameObject.SetActive(true);
            
            firstSelected = previouslySelected != null ? previouslySelected : playButton;
            SetupNavigation();

            settingsToggleCoroutine = null;
        }
    }
}