using System;
using System.Collections.Generic;
using RobbieWagnerGames.Utilities;
using TMPro;
using UnityEngine;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
    public class ResourceManager : MonoBehaviourSingleton<ResourceManager>
    {
        public Dictionary<ResourceType, int> gatheredResources = new Dictionary<ResourceType, int>();
        [SerializeField] private TextMeshProUGUI resourceText;
        [SerializeField] private Canvas canvas;
        
        [Header("Resource Pip Prefab")]
        [SerializeField] private ResourcePip resourcePipPrefab;

        protected override void Awake() 
        {
            base.Awake();
            
            InitializeResourceDictionary();
            
            GameManager.Instance.OnGameStart += EnableResourceTracking;
            GameManager.Instance.OnGameOver += DisableResourceTracking;
            GameManager.Instance.OnReturnToMenu += ResetResources;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStart -= EnableResourceTracking;
                GameManager.Instance.OnGameOver -= DisableResourceTracking;
                GameManager.Instance.OnReturnToMenu -= ResetResources;
            }
        }
        
        private void InitializeResourceDictionary()
        {
            gatheredResources.Clear();

            foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
            {
                if (type != ResourceType.NONE)
                    gatheredResources[type] = 0;
            }
        }

        private void EnableResourceTracking()
        {
            canvas.enabled = true;
            ResetResources();
        }

        private void DisableResourceTracking()
        {
            canvas.enabled = false;
        }

        public void ResetResources()
        {
            InitializeResourceDictionary();
            UpdateResourceDisplay();
        }

        public void AddResource(ResourceType resourceType, int amount)
        {
            if (resourceType == ResourceType.NONE || amount <= 0) return;
            
            if (gatheredResources.ContainsKey(resourceType))
                gatheredResources[resourceType] += amount;
            else
                gatheredResources[resourceType] = amount;
            
            UpdateResourceDisplay();
        }
        
        public void SpawnResourcePips(Vector2 position, ResourceGatherData resourceData, int pipCount = 5)
        {
            if (resourcePipPrefab == null || resourceData == null) return;
            
            foreach (var resource in resourceData.resources)
            {
                if (resource.Key == ResourceType.NONE || resource.Value <= 0) continue;
                
                int remainingAmount = resource.Value;

                while (remainingAmount >= 10)
                {
                    SpawnPip(position, resource.Key, 10);
                    remainingAmount -= 10;
                }
                
                while (remainingAmount >= 5)
                {
                    SpawnPip(position, resource.Key, 5);
                    remainingAmount -= 5;
                }
                
                while (remainingAmount > 0)
                {
                    SpawnPip(position, resource.Key, 1);
                    remainingAmount -= 1;
                }
            }
        }

        private void SpawnPip(Vector2 position, ResourceType resourceType, int amount)
        {
            ResourcePip pip = Instantiate(resourcePipPrefab, position, Quaternion.identity);
            
            if (pip != null)
            {
                pip.Initialize(resourceType, amount);
                pip.AddRandomForce(UnityEngine.Random.Range(1f, 3f));
            }
        }

        private void UpdateResourceDisplay()
        {
            if (resourceText == null) return;
            
            string displayText = "Resources:\n";
            
            foreach (var resource in gatheredResources)
            {
                if (resource.Key != ResourceType.NONE)
                    displayText += $"{resource.Key}: {resource.Value}\n";
            }
            
            resourceText.text = displayText;
        }
        
        public int GetTotalResources()
        {
            int total = 0;
            foreach (var resource in gatheredResources)
            {
                if (resource.Key != ResourceType.NONE)
                    total += resource.Value;
            }
            return total;
        }
        
        public bool HasResource(ResourceType type, int amount)
        {
            return gatheredResources.ContainsKey(type) && gatheredResources[type] >= amount;
        }
        
        public bool SpendResource(ResourceType type, int amount)
        {
            if (!HasResource(type, amount)) return false;
            
            gatheredResources[type] -= amount;
            UpdateResourceDisplay();
            return true;
        }
    }
}