using UnityEngine;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
	public enum ShopItemType
    {
        HEALTH,
        SHIP_SPEED,
        FIRE_RATE,
        BULLET_SPEED,
        FIRE_RANGE,
        NOVA_BLAST,
        RESOURCE_CONVERSION
    }

    [CreateAssetMenu(fileName = "ShopItem", menuName = "RoguelikeAsteroids/ShopItem")]
    public class ShopItem : ScriptableObject
    {
        public string itemName;
        public string description;
        public Sprite icon;
		public Color color;
        public ShopItemType itemType;
        
        [Header("Pricing")]
        public ResourceType primaryResource;
        public int primaryCost;
        
        [Header("Tiered Values")]
        public int maxTiers = -1;
        public float[] tieredValues;
        public int[] tieredCosts;
        
        [Header("Purchase Conditions")]
        public bool requirePreviousTier = false;
    }
}