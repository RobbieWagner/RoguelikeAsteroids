using System.Collections;
using RobbieWagnerGames.Managers;
using RobbieWagnerGames.UI;
using UnityEngine;
using UnityEngine.UI;

namespace RobbieWagnerGames.UI
{
    public partial class PauseMenu : Menu
    {
        [Header("Settings Menu")]
        [SerializeField] private GameObject settingsMenuPanel;
        [SerializeField] private Button settingsBackButton;
        private Selectable previouslySelected = null;
        private bool isSettingsOpen = false;
        private Coroutine settingsToggleCoroutine;

        private void OpenSettings()
        {
            if (isSettingsOpen) return;
            
            if (settingsToggleCoroutine == null)
                settingsToggleCoroutine = StartCoroutine(OpenSettingsMenuCoroutine());
        }

        private IEnumerator OpenSettingsMenuCoroutine()
        {
            yield return new WaitForSecondsRealtime(.02f);

            isSettingsOpen = true;

            previouslySelected = EventSystemManager.Instance?.CurrentSelected?.GetComponent<Selectable>();

            resumeButton.gameObject.SetActive(false);
            settingsButton.gameObject.SetActive(false);
            mainMenuButton.gameObject.SetActive(false);
            quitAppButton.gameObject.SetActive(false);
            
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
            yield return new WaitForSecondsRealtime(.02f);

            Debug.Log("close");
            isSettingsOpen = false;

            settingsMenuPanel.SetActive(false);
            
            resumeButton.gameObject.SetActive(true);
            settingsButton.gameObject.SetActive(true);
            mainMenuButton.gameObject.SetActive(true);
            quitAppButton.gameObject.SetActive(true);
            
            firstSelected = previouslySelected != null ? previouslySelected : resumeButton;
            SetupNavigation();

            settingsToggleCoroutine = null;
        }
    }
}