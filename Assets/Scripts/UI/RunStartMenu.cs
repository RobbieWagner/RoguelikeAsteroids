using UnityEngine;
using UnityEngine.UI;
using RobbieWagnerGames.UI;
using TMPro;
using RobbieWagnerGames.Managers;
using System.Collections.Generic;
using System.Linq;
using RobbieWagnerGames.Audio;

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
            Run run = CreateNewRun();
            RunManager.Instance.StartNewRun(run);
            Close();
        }

        private Run CreateNewRun()
        {
            Run newRun = RunManager.Instance.defaultRun;
            
            foreach (KeyValuePair<RunPurchaseItem, int> kvp in purchasedItems)
                ApplyItemEffectToRun(kvp.Key, kvp.Value, newRun);

            return newRun;
        }

        private void ApplyItemEffectToRun(RunPurchaseItem item, int tier, Run run)
        {
            switch (item.purchaseType)
            {
                case PurchaseType.MAX_HEALTH:
                    run.startingHealth = 3 + tier;
                    break;
                case PurchaseType.FIRE_RATE:
                    run.fireCooldownModifier = tier * .25f;
                    break;
                case PurchaseType.STARTING_NOVA_BURSTS:
                    run.novaBlasts = tier;
                    break;
                case PurchaseType.BUCKSHOT:
                    run.hasBuckshot = true;
                    break;
                case PurchaseType.ROCKET_LAUNCHER:
                    run.hasRocketLauncher = true;
                    break;
                case PurchaseType.MINI_LASER:
                    run.hasMiniLaser = true;
                    break;
                default:
                    break;
            }
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
            base.Open();

            PopulatePurchaseMenu();
            SetupNavigation();
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
                purchasedItems[item]++;
            else
                purchasedItems[item] = 1;
            
            RunPurchaseButton button = purchaseButtons.FirstOrDefault(b => b.Item == item);
                button.IncrementTier();

            RefreshPurchaseButtons();

            BasicAudioManager.Instance.Play(AudioSourceName.Purchase);
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