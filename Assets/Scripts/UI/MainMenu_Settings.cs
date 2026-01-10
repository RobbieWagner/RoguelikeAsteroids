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
        [SerializeField] private SettingsMenu settingsMenu;
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
            
            // Set settings menu as active
            activeMenu = settingsMenu;
            
            settingsMenu.backButton.onClick.RemoveAllListeners();
            settingsMenu.backButton.onClick.AddListener(CloseSettingsMenu);
            
            selectionIcon.gameObject.SetActive(false);

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
            isSettingsOpen = false;
            settingsMenuPanel.SetActive(false);
            settingsMenu.Close();
            
            playButton.gameObject.SetActive(true);
            settingsButton.gameObject.SetActive(true);
            exitButton.gameObject.SetActive(true);
            
            activeMenu = this;
            
            firstSelected = previouslySelected != null ? previouslySelected : playButton;
            RefreshSelectableElements();
            SetupNavigation();

            settingsToggleCoroutine = null;

            if (isUsingController) selectionIcon.gameObject.SetActive(true);
        }
    }
}