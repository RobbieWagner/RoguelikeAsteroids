using UnityEngine;
using UnityEngine.UI;
using RobbieWagnerGames.UI;
using TMPro;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
    public class RunStartMenu : Menu
    {
        [Header("Run Menu UI")]
        [SerializeField] private Button startRunButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI creditsText;
        
        private int currentLevel = 1;
        private int currentCredits = 100; // TODO: Credits will be a gamemanager level property when creating gamesavedata

        protected override void Awake()
        {
            base.Awake();
            
            startRunButton.onClick.AddListener(() => RunManager.Instance.StartRun());
            mainMenuButton.onClick.AddListener(() => RunManager.Instance.ReturnToMainMenu());
            
            firstSelected = startRunButton;
            
            RunManager.Instance.OnShowRunMenu += OnRunMenuRequested;
            RunManager.Instance.OnHideRunMenu += OnRunMenuHideRequested;
            
            Close();
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