using System;
using System.Collections.Generic;
using RobbieWagnerGames.Utilities;
using UnityEngine;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
    public class ResourceManager : MonoBehaviourSingleton<ResourceManager>
    {
        private Dictionary<ResourceType, int> _gatheredResources = new Dictionary<ResourceType, int>();
        public Dictionary<ResourceType, int> gatheredResources => _gatheredResources; 
        
        public event Action<ResourceType, int> OnResourceAdded;
        public event Action<ResourceType, int> OnResourceRemoved;
        public event Action OnResourcesReset;
        public event Action<Dictionary<ResourceType, int>> OnResourcesUpdated;

        protected override void Awake() 
        {
            base.Awake();
            
            GameManager.Instance.OnReturnToMenu += ResetResources;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            GameManager.Instance.OnReturnToMenu -= ResetResources;
        }
        
        public void InitializeResourceDictionary(Dictionary<ResourceType, int> initialResources)
        {
            _gatheredResources.Clear();
            
            foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
            {
                if (type != ResourceType.NONE)
                {
                    if (initialResources.TryGetValue(type, out int resourceAmount))
                        _gatheredResources.Add(type, resourceAmount);
                    else
                        _gatheredResources.Add(type, 0);

                }
            }
            
            OnResourcesReset?.Invoke();
            OnResourcesUpdated?.Invoke(new Dictionary<ResourceType, int>(_gatheredResources));
        }

        public void ResetResources()
        {
            InitializeResourceDictionary(new Dictionary<ResourceType, int>());
        }

        public void AddResource(ResourceType resourceType, int amount)
        {
            if (resourceType == ResourceType.NONE || amount <= 0) return;
            
            if (_gatheredResources.ContainsKey(resourceType))
                _gatheredResources[resourceType] += amount;
            else
                _gatheredResources[resourceType] = amount;
            
            OnResourceAdded?.Invoke(resourceType, amount);
            OnResourcesUpdated?.Invoke(new Dictionary<ResourceType, int>(_gatheredResources));
        }
        
        public void AddResources(Dictionary<ResourceType, int> resources)
        {
            if (resources == null) return;
            
            foreach (KeyValuePair<ResourceType, int> resource in resources)
                AddResource(resource.Key, resource.Value);
        }
        
        public void RemoveResource(ResourceType resourceType, int amount)
        {
            if (resourceType == ResourceType.NONE || amount <= 0 || 
                !_gatheredResources.ContainsKey(resourceType)) return;
            
            _gatheredResources[resourceType] = Mathf.Max(0, _gatheredResources[resourceType] - amount);
            
            OnResourceRemoved?.Invoke(resourceType, amount);
            OnResourcesUpdated?.Invoke(new Dictionary<ResourceType, int>(_gatheredResources));
        }
        
        public bool HasResource(ResourceType resourceType, int amount)
        {
            return _gatheredResources.ContainsKey(resourceType) && _gatheredResources[resourceType] >= amount;
        }
        
        public bool HasResources(Dictionary<ResourceType, int> requiredResources)
        {
            if (requiredResources == null) return true;
            
            foreach (KeyValuePair<ResourceType, int> resource in requiredResources)
            {
                if (!HasResource(resource.Key, resource.Value))
                    return false;
            }
            
            return true;
        }
        
        public bool SpendResource(ResourceType resourceType, int amount)
        {
            if (!HasResource(resourceType, amount)) return false;
            
            RemoveResource(resourceType, amount);
            return true;
        }
        
        public bool SpendResources(Dictionary<ResourceType, int> costDetails)
        {
            if (!HasResources(costDetails)) return false;
            
            foreach (KeyValuePair<ResourceType, int> costDetail in costDetails)
                _gatheredResources[costDetail.Key] -= costDetail.Value;
            
            OnResourcesUpdated?.Invoke(new Dictionary<ResourceType, int>(_gatheredResources));
            return true;
        }
        
        public Dictionary<ResourceType, int> GetAllResources()
        {
            return new Dictionary<ResourceType, int>(_gatheredResources);
        }
    }
}