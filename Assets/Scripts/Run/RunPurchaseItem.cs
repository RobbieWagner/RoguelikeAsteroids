using UnityEngine;

namespace RobbieWagnerGames.RoguelikeAsteroids
{

    public enum PurchaseType
    {
        MAX_HEALTH,
        FIRE_RATE,
        MINI_LASER,
        ROCKET_LAUNCHER,
        BUCKSHOT,
        STARTING_NOVA_BURSTS
    }

    [CreateAssetMenu(fileName = "NewRunPurchaseItem", menuName = "RoguelikeAsteroids/Run Purchase Item")]
    public class RunPurchaseItem : ScriptableObject
    {
        public string displayName;
        public string description;
        public int victoryPointCost;
        public Sprite icon;
        [Tooltip("for single purchase, >1 for tiered upgrades")] public int maxTiers = 1;
        [Tooltip("The amount this item modifies a stat")] public float effectValue = 1f;
        
        public PurchaseType purchaseType;
    }

    [System.Serializable]
    public class RunPurchaseItemData
    {
        public string itemName;
        public int currentTier = 0;
        public bool IsPurchased => currentTier > 0;
        public bool IsAtMaxTier(int maxTiers) => currentTier >= maxTiers;
        
        public bool TryPurchase(RunPurchaseItem item, ref int victoryPoints)
        {
            if (IsAtMaxTier(item.maxTiers) || victoryPoints < item.victoryPointCost)
                return false;
                
            victoryPoints -= item.victoryPointCost;
            currentTier++;
            return true;
        }
    }
}