using RobbieWagnerGames.Utilities.SaveData;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RobbieWagnerGames.UI
{
    public class MainMenuManager : Menu
    {
        [SerializeField] private Button continueButton;
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button controlsButton;
        [SerializeField] private Button creditsButton;
        [SerializeField] private Button quitButton;

        [SerializeField] private string sceneToGoTo;

        [SerializeField] private Menu settings;
        [SerializeField] private Menu controls;
        [SerializeField] private Menu credits;

        protected override void Awake()
        {
            base.Awake();
            Cursor.lockState = CursorLockMode.None;

            if (JsonDataService.Instance == null)
                new JsonDataService();
        }

        protected override void OnEnable()
        {
            newGameButton.onClick.AddListener(StartNewGame);
            continueButton.onClick.AddListener(StartGame);
            settingsButton.onClick.AddListener(OpenSettings);
            //controlsButton.onClick.AddListener(OpenControls);
            //creditsButton.onClick.AddListener(OpenCredits);
            quitButton.onClick.AddListener(QuitGame);

            //if(JsonDataService.Instance.LoadData())

            base.OnEnable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            newGameButton.onClick.RemoveListener(StartNewGame);
            continueButton.onClick.RemoveListener(StartGame);
            settingsButton.onClick.RemoveListener(OpenSettings);
            //controlsButton.onClick.RemoveListener(OpenControls);
            //creditsButton.onClick.RemoveListener(OpenCredits);
            quitButton.onClick.RemoveListener(QuitGame);
        }

        private void StartNewGame()
        {    
            if(TryStartGame())
                JsonDataService.Instance.PurgeData();
        }

        public void StartGame()
        {
            if (menuCoroutine == null)
                menuCoroutine = StartCoroutine(StartGameCo());
        }

        public bool TryStartGame()
        {
            if (menuCoroutine == null)
            {
                menuCoroutine = StartCoroutine(StartGameCo());
                return true;
            }
            return false;
        }

        public IEnumerator StartGameCo()
        {
            ToggleButtonInteractibility(false);

            yield return StartCoroutine(ScreenCover.Instance.FadeCoverIn());
            menuCoroutine = null;

            SceneManager.LoadScene(sceneToGoTo);
        }

        private void OpenSettings()
        {
            StartCoroutine(SwapMenu(this, settings));
        }

        //private void OpenControls()
        //{
        //    StartCoroutine(SwapCanvases(thisCanvas, controls));
        //}

        //private void OpenCredits()
        //{
        //    StartCoroutine(SwapCanvases(thisCanvas, credits));
        //}

        private void QuitGame()
        {
            ToggleButtonInteractibility(false);

            //save any new save data
            Application.Quit();
        }

        protected override void ToggleButtonInteractibility(bool toggleOn)
        {
            base.ToggleButtonInteractibility(toggleOn);

            continueButton.interactable = toggleOn;
            settingsButton.interactable = toggleOn;
            //controlsButton.interactable = toggleOn;
            //creditsButton.interactable = toggleOn;
            quitButton.interactable = toggleOn;
        }

        protected override IEnumerator SwapMenu(Menu active, Menu next, bool setAsLastMenu = true)
        {
            yield return StartCoroutine(base.SwapMenu(active, next));

            StopCoroutine(SwapMenu(active, next));
        }
    }
}