using UnityEngine;
using UnityEngine.UI;
using RobbieWagnerGames.UI;
using TMPro;
using RobbieWagnerGames.Managers;
using System.Collections.Generic;
using System.Linq;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
    public class RunStartMenu : Menu
    {
        [Header("Run Start Menu UI")]
        [SerializeField] private Button startRunButton;
        [SerializeField] private VerticalLayoutGroup buttonContainer;
        [SerializeField] private RunPurchaseButton buttonPrefab;
        [SerializeField] private TextMeshProUGUI victoryPointsText;
        
        [SerializeField] private List<RunPurchaseItem> availableItems;
        private List<RunPurchaseButton> purchaseButtons = new List<RunPurchaseButton>();
        private Dictionary<RunPurchaseItem, int> purchasedItems = new Dictionary<RunPurchaseItem, int>();
    
        private int totalVictoryPoints => GameManager.Instance.currentSave.victoryPoints;
        private int usedVictoryPoints = 0;
        private int availableVictoryPoints => totalVictoryPoints - usedVictoryPoints;

        protected override void Awake()
        {
            base.Awake();
            
            startRunButton.onClick.AddListener(() => HandleStartRunButtonPressed());
            
            RunManager.Instance.OnShowRunMenu += OnRunMenuRequested;
            RunManager.Instance.OnHideRunMenu += OnRunMenuHideRequested;
            
            Close();
        }

        private void HandleStartRunButtonPressed()
        {
            ApplyPurchasesToRun();
            
            RunManager.Instance.CreateNewRun(
                tiers: 5,
                difficulty: 1.0f,
                shopRatio: 0.1f,
                includeBosses: true
            );
            
            RunManager.Instance.LoadRun();
        }

        private void ApplyPurchasesToRun()
        {
            if (RunManager.Instance.CurrentRun == null) return;
            
            Run run = RunManager.Instance.CurrentRun;
            
            foreach (KeyValuePair<RunPurchaseItem, int> kvp in purchasedItems)
                ApplyItemEffectToRun(kvp.Key, kvp.Value, run);
        }

        private void ApplyItemEffectToRun(RunPurchaseItem item, int tier, Run run)
        {
            // TODO: Implement actual effects based on item type
            // This is where you'd modify run properties based on purchases
            // Example:
            // if (item.name.Contains("Health"))
            //     run.startingHealth += 1;
            // else if (item.name.Contains("Speed"))
            //     run.speedModifier += 0.1f;
            // etc.
        }

        private void OnRunMenuRequested()
        {
            activeMenu = this;
            usedVictoryPoints = 0;
            UpdateUI();
            Open();
        }

        private void OnRunMenuHideRequested()
        {
            activeMenu = null;
            Close();
        }

        private void UpdateUI()
        {
            victoryPointsText.text = $"{availableVictoryPoints}/{totalVictoryPoints}";
        }

        public override void Open()
        {
            PopulatePurchaseMenu();
            SetupNavigation();
            
            if (disableGameControlsWhenOpen)
            {
                previousActiveMaps.Clear();
                previousActiveMaps.AddRange(InputManager.Instance.CurrentActiveMaps);
            }

            InputManager.Instance.EnableActionMap(ActionMapName.UI);
            
            canvas.enabled = true;
            
            lastMouseActivityTime = Time.time - mouseInactivityTimeout;
            
            StartSelectionMaintenance();

            OnOpened();
        }

        protected override void OnOpened()
        {
            base.OnOpened();
            
            RefreshSelectableElements();
            
            if (purchaseButtons.Count == 0 && startRunButton != null)
                firstSelected = startRunButton;
            else if (purchaseButtons.Count > 0)
                firstSelected = purchaseButtons[0].Button;
        }

        private void PopulatePurchaseMenu()
        {
            foreach (Transform child in buttonContainer.transform)
                Destroy(child.gameObject);
            
            purchaseButtons.Clear();
            purchasedItems.Clear();
            
            foreach (RunPurchaseItem item in availableItems)
            {
                RunPurchaseButton button = Instantiate(buttonPrefab, buttonContainer.transform);
                button.Initialize(item, OnItemPurchased);
                purchaseButtons.Add(button);
            }
            
            RefreshPurchaseButtons();
        }

        public void RefreshPurchaseButtons()
        {
            foreach (RunPurchaseButton button in purchaseButtons)
            {
                if (button.Item == null) continue;
                
                bool canAfford = availableVictoryPoints >= button.Item.victoryPointCost;
                
                bool isPurchased = purchasedItems.ContainsKey(button.Item);
                
                if (isPurchased && button.IsAtMaxTier())
                    button.SetPurchased(true);
                else
                    button.SetAffordable(canAfford && !isPurchased);
            }
            
            UpdateUI();
            
            RefreshSelectableElements();
        }

        private void OnItemPurchased(RunPurchaseItem item)
        {
            if (availableVictoryPoints < item.victoryPointCost) return;
            
            usedVictoryPoints += item.victoryPointCost;
            
            if (purchasedItems.ContainsKey(item))
            {
                purchasedItems[item]++;
                var button = purchaseButtons.FirstOrDefault(b => b.Item == item);
                    button.IncrementTier();
            }
            else
                purchasedItems[item] = 1;
            
            RefreshPurchaseButtons();
            
            // Play purchase sound
            // BasicAudioManager.Instance.Play(AudioSourceName.UISelect);
        }

        public override void RefreshSelectableElements()
        {
            base.RefreshSelectableElements();
            
            selectableElements.Clear();
            
            foreach (RunPurchaseButton purchaseButton in purchaseButtons)
            {
                if (purchaseButton.Button != null && purchaseButton.Button.gameObject.activeInHierarchy)
                    selectableElements.Add(purchaseButton.Button);
            }
            
            if (startRunButton != null && startRunButton.gameObject.activeInHierarchy)
                selectableElements.Add(startRunButton);
            
            selectableElements.Sort((a, b) =>
            {
                Vector3 aPos = a.transform.position;
                Vector3 bPos = b.transform.position;
                return bPos.y.CompareTo(aPos.y);
            });
        }

        protected override void HandleCancel()
        {
            // No back button, so cancel doesn't do anything in this menu
            // Could optionally make it close the menu or return to main menu
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            RunManager.Instance.OnShowRunMenu -= OnRunMenuRequested;
            RunManager.Instance.OnHideRunMenu -= OnRunMenuHideRequested;
        }
    }
}