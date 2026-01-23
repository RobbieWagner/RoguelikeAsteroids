using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
    public class RunPurchaseButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI buttonText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private TextMeshProUGUI currentLevelText;
        [SerializeField] private Image purchaseIcon;
        [SerializeField] private Image backgroundImage;

        private RunPurchaseItem item;
        private Action<RunPurchaseItem> onPurchaseCallback;
        private int currentTier = 0;
        
        public RunPurchaseItem Item => item;
        public Button Button => button;

        public void Initialize(RunPurchaseItem item, Action<RunPurchaseItem> onPurchaseCallback)
        {
            this.item = item;
            this.onPurchaseCallback = onPurchaseCallback;
            
            UpdateButtonUI();
            
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnButtonClicked);
        }

        private void UpdateButtonUI()
        {
            if (item == null) return;
            
            buttonText.text = item.displayName;
            costText.text = $"{item.victoryPointCost}";

            currentLevelText.text = $"{currentTier + 1}/{item.maxTiers}";
            purchaseIcon.sprite = item.icon;
            backgroundImage.color = Color.white;
        }

        public void SetAffordable(bool canAfford)
        {
            button.interactable = canAfford;

            backgroundImage.color = canAfford ? Color.white : new Color(0.7f, 0.7f, 0.7f, 0.7f);
            costText.color = canAfford ? Color.white : Color.red;
        }

        public void SetPurchased(bool isPurchased)
        {
            button.interactable = !isPurchased;
            backgroundImage.color = isPurchased ? Color.green : Color.white;
            currentLevelText.text = "Purchased";
        }

        private void OnButtonClicked()
        {
            onPurchaseCallback?.Invoke(item);
        }

        public void IncrementTier()
        {
            currentTier++;
            UpdateButtonUI();
        }

        public bool IsAtMaxTier()
        {
            return currentTier >= Item.maxTiers-1;
        }
    }
}