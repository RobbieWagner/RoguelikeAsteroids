using System;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
	[Serializable]
    public class ShopItemData
    {
		public string itemName;
        public int currentTier = 0;
        
        public bool IsMaxTier(ShopItem item) => item.maxTiers > 0 && currentTier >= item.maxTiers;
        public bool CanPurchase(ShopItem item) => !IsMaxTier(item) && ResourceManager.Instance.HasResource(item.primaryResource, GetCost(item));
        public int GetCost(ShopItem item) => item.primaryCost + (currentTier < item.tieredCosts?.Length ? item.tieredCosts[currentTier] : 0);
        
        public bool Equals(ShopItemData other)
        {
            if (ReferenceEquals(null, other)) 
                return false;
            if (ReferenceEquals(this, other)) 
                return true;
            return itemName == other.itemName;
        }
        
        public override int GetHashCode() => itemName?.GetHashCode() ?? 0;
        
        public bool TryPurchase(ShopItem item)
        {
            if (!CanPurchase(item))
                return false;
            
            int cost = item.primaryCost + (currentTier < item.tieredCosts?.Length ? item.tieredCosts[currentTier] : 0);
            ResourceManager.Instance.SpendResource(item.primaryResource, cost);
            
            ApplyEffect(item);
            
            currentTier++;
                
            return true;
        }
        
        private void ApplyEffect(ShopItem item)
        {
            float value = currentTier < item.tieredValues?.Length ? item.tieredValues[currentTier] : 1f;
            
            switch (item.itemType)
            {
                case ShopItemType.HEALTH:
                    RunManager.Instance.CurrentRun.health += (int) value;
                    break;
                case ShopItemType.SHIP_SPEED:
                    RunManager.Instance.CurrentRun.speedModifier += value;
                    break;
                case ShopItemType.FIRE_RATE:
                    RunManager.Instance.CurrentRun.fireCooldownModifier += value;
                    break;
                case ShopItemType.BULLET_SPEED:
                    RunManager.Instance.CurrentRun.bulletSpeedModifier += value;
                    break;
                case ShopItemType.FIRE_RANGE:
                    RunManager.Instance.CurrentRun.fireRangeModifier += value;
                    break;
                case ShopItemType.NOVA_BLAST:
                    RunManager.Instance.CurrentRun.novaBlasts += (int) value;
                    break;
                case ShopItemType.RESOURCE_CONVERSION:
                    break;
            }
        }
	}
}