using System.Collections;
using RobbieWagnerGames.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace RobbieWagnerGames.UI
{
    public partial class PauseMenu : Menu
    {
        [Header("Settings Menu")]
        [SerializeField] private GameObject settingsMenuPanel;
        [SerializeField] private SettingsMenu settingsMenu;
        [SerializeField] private Button settingsBackButton;
        private Selectable previouslySelected = null;
        private bool isSettingsOpen = false;
        private Coroutine settingsToggleCoroutine;

        private void OpenSettings()
        {
            if (isSettingsOpen) return;
            
            if (settingsToggleCoroutine == null)
                settingsToggleCoroutine = StartCoroutine(OpenSettingsMenuCoroutine());

            settingsBackButton.onClick.RemoveAllListeners();
            settingsBackButton.onClick.AddListener(CloseSettingsMenu);
        }

        private IEnumerator OpenSettingsMenuCoroutine()
        {
            yield return null;

            isSettingsOpen = true;

            previouslySelected = EventSystemManager.Instance?.CurrentSelected?.GetComponent<Selectable>();

            resumeButton.gameObject.SetActive(false);
            settingsButton.gameObject.SetActive(false);
            mainMenuButton.gameObject.SetActive(false);
            
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
            
            settingsMenuPanel.SetActive(false);
            
            resumeButton.gameObject.SetActive(true);
            settingsButton.gameObject.SetActive(true);
            mainMenuButton.gameObject.SetActive(true);
            
            activeMenu = this;
            
            firstSelected = previouslySelected != null ? previouslySelected : resumeButton;
            RefreshSelectableElements();
            SetupNavigation();

            settingsToggleCoroutine = null;

            if (isUsingController) selectionIcon.gameObject.SetActive(true);
        }
    }
}