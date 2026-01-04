using UnityEngine;
using UnityEngine.UI;
using RobbieWagnerGames.UI;
using TMPro;
using RobbieWagnerGames.Utilities.SaveData;
using System;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
    public class RunStartMenu : Menu
    {
        [Header("Run Menu UI")]
        [SerializeField] private Button newRunButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI creditsText;
        
        private int currentLevel = 1;
        private int currentCredits = 100; // TODO: Credits will be a gamemanager level property when creating gamesavedata

        private bool hasExistingRun = false;

        protected override void Awake()
        {
            base.Awake();
            
            newRunButton.onClick.AddListener(() => HandleNewRunButtonPressed());
            continueButton.onClick.AddListener(() => RunManager.Instance.StartRun());
            mainMenuButton.onClick.AddListener(() => RunManager.Instance.ReturnToMainMenu());
            
            hasExistingRun = JsonDataService.Instance.LoadDataRelative<Run>(GameConstants.RunPath, null) != null;
            firstSelected = hasExistingRun ? continueButton : newRunButton;
            if (!hasExistingRun)
                continueButton.gameObject.SetActive(false);
            
            RunManager.Instance.OnShowRunMenu += OnRunMenuRequested;
            RunManager.Instance.OnHideRunMenu += OnRunMenuHideRequested;
            
            Close();
        }

        private void HandleNewRunButtonPressed()
        {
            if (hasExistingRun)
                ShowNewRunPrompt();
            else RunManager.Instance.StartRun();
        }

        public void ShowNewRunPrompt()
        {
            PromptManager.Instance.ShowConfirmationPrompt(
                "Start New Run?",
                "This will delete your current progress and start a new run. Are you sure?",
                () => 
                {
                    JsonDataService.Instance.DeleteData(GameConstants.RunPath);
                    RunManager.Instance.StartRun();
                },
                null
            );
        }

        private void OnRunMenuRequested()
        {
            Open();
            UpdateUI();
        }

        private void OnRunMenuHideRequested()
        {
            Close();
        }

        private void UpdateUI()
        {
            levelText.text = $"Level: {currentLevel}";
            creditsText.text = $"Credits: {currentCredits}";
        }

        public void AddCredits(int amount)
        {
            currentCredits += amount;
            UpdateUI();
        }

        public bool SpendCredits(int amount)
        {
            if (currentCredits >= amount)
            {
                currentCredits -= amount;
                UpdateUI();
                return true;
            }
            return false;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            RunManager.Instance.OnShowRunMenu -= OnRunMenuRequested;
            RunManager.Instance.OnHideRunMenu -= OnRunMenuHideRequested;
        }
    }
}