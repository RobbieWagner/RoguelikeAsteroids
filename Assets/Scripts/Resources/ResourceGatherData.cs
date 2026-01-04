using System.Collections.Generic;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
    public enum ResourceType
    {
        NONE = -1,
        TITANIUM,
        PLATINUM,
        IRIDIUM
    }

    public class ResourceGatherData
    {
        public Dictionary<ResourceType,int> resources;

        public ResourceGatherData()
        {
            resources = new Dictionary<ResourceType, int>();
            foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
            {
                if (type != ResourceType.NONE)
                    resources[type] = 0;
            }
        }
        
        public ResourceGatherData(Dictionary<ResourceType,int> resources)
        {
            this.resources = new Dictionary<ResourceType,int>(resources);
        }
        
        public void AddResource(ResourceType type, int amount)
        {
            if (type == ResourceType.NONE || amount <= 0) return;
            
            if (resources.ContainsKey(type))
                resources[type] += amount;
            else
                resources[type] = amount;
        }
        
        public int GetTotalResources()
        {
            int total = 0;
            foreach (KeyValuePair<ResourceType, int> resource in resources)
            {
                if (resource.Key != ResourceType.NONE)
                    total += resource.Value;
            }
            return total;
        }
    }
}