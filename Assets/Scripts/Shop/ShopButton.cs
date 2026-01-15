using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using RobbieWagnerGames.Audio;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
    public class ShopButton : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private Button purchaseButton;
        [SerializeField] private Image cantAffordOverlay;
        
        private ShopItem currentItem;
        private ShopItemData currentData;
        private Action onPurchaseCallback;
        
        public void Initialize(ShopItem item, ShopItemData data, Action onPurchase)
        {
            currentItem = item;
            currentData = data;
            onPurchaseCallback = onPurchase;
            
            UpdateUI();
            
            purchaseButton.onClick.AddListener(OnButtonClicked);
        }
        
        private void UpdateUI()
        {
            if (currentItem == null || RunManager.Instance?.CurrentRun == null) return;
            
            iconImage.sprite = currentItem.icon;
            string tierValue = currentItem.maxTiers > 0 ? $" {currentData.currentTier + 1}" : "";
            nameText.text = $"{currentItem.itemName}{tierValue}";
            descriptionText.text = currentItem.description;
            
            int cost = currentData.GetCost(currentItem);
            costText.text = $"{cost} {currentItem.primaryResource}";
            
            bool canPurchase = currentData.CanPurchase(currentItem);
            bool isMaxTier = currentData.IsMaxTier(currentItem);
            
            purchaseButton.interactable = canPurchase && !isMaxTier;
            
            if (isMaxTier)
            {
                Debug.Log($"{currentData.currentTier} {currentData.IsMaxTier(currentItem)} {currentItem.maxTiers}");
                costText.text = "SOLD OUT";
            }
            else if (!canPurchase)
                cantAffordOverlay.enabled = true;
            else
                cantAffordOverlay.enabled = false;
            
            if (purchaseButton.navigation.mode == Navigation.Mode.None)
            {
                Navigation nav = purchaseButton.navigation;
                nav.mode = Navigation.Mode.Automatic;
                purchaseButton.navigation = nav;
            }
        }
        
        private void OnButtonClicked()
        {
            if (currentData.TryPurchase(currentItem))
            {
                BasicAudioManager.Instance.Play(AudioSourceName.Purchase);
                onPurchaseCallback?.Invoke();
                UpdateUI();
            }
        }
        
        private void OnDisable()
        {
            purchaseButton.onClick.RemoveListener(OnButtonClicked);
        }
        
        private void OnDestroy()
        {
            purchaseButton.onClick.RemoveListener(OnButtonClicked);
        }
        
        public void Refresh()
        {
            UpdateUI();
        }
    }
}