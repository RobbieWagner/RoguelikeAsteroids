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
            yield return new WaitForSecondsRealtime(.02f);

            isSettingsOpen = true;
            previouslySelected = EventSystemManager.Instance?.CurrentSelected?.GetComponent<Selectable>();

            resumeButton.gameObject.SetActive(false);
            settingsButton.gameObject.SetActive(false);
            mainMenuButton.gameObject.SetActive(false);
            
            settingsMenuPanel.SetActive(true);
 
            settingsMenu.Open();
        
            settingsMenu.RefreshSelectableElements();

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

            if (settingsMenu != null)
                settingsMenu.Close();
            
            settingsMenuPanel.SetActive(false);
            
            resumeButton.gameObject.SetActive(true);
            settingsButton.gameObject.SetActive(true);
            mainMenuButton.gameObject.SetActive(true);
            
            firstSelected = previouslySelected != null ? previouslySelected : resumeButton;
            RefreshSelectableElements();
            SetupNavigation();

            settingsToggleCoroutine = null;
        }
    }
}