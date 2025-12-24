using System;
using System.Collections.Generic;
using RobbieWagnerGames.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
    public class ResourceManager : MonoBehaviourSingleton<ResourceManager>
    {
        public Dictionary<ResourceType, int> gatheredResources = new Dictionary<ResourceType, int>();
        
        [Header("Resource UI")]
        [SerializeField] private LayoutGroup resourcesList;
        [SerializeField] private ResourceUI resourceUIPrefab;
        private Dictionary<ResourceType, ResourceUI> activeResourceUIs = new Dictionary<ResourceType, ResourceUI>();
        
        [Header("Resource Pips")]
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
            ClearResourceUIs();

            foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
            {
                if (type != ResourceType.NONE)
                {
                    gatheredResources[type] = 0;
                    CreateOrUpdateResourceUI(type, 0);
                }
            }
        }

        private void ClearResourceUIs()
        {
            foreach (var ui in activeResourceUIs.Values)
            {
                if (ui != null && ui.gameObject != null)
                    Destroy(ui.gameObject);
            }
            activeResourceUIs.Clear();
        }

        private void CreateOrUpdateResourceUI(ResourceType resourceType, int amount)
        {
            if (!activeResourceUIs.ContainsKey(resourceType) || activeResourceUIs[resourceType] == null)
            {
                if (resourceUIPrefab == null || resourcesList == null)
                {
                    Debug.LogError("Resource UI prefab or resources list not assigned!");
                    return;
                }
                
                ResourceUI newUI = Instantiate(resourceUIPrefab, resourcesList.transform);
                newUI.Initialize(resourceType, amount);
                activeResourceUIs[resourceType] = newUI;
            }
            else
            {
                activeResourceUIs[resourceType].UpdateAmount(amount);
                
                if (amount <= 0)
                    activeResourceUIs[resourceType].gameObject.SetActive(false);
                else if (!activeResourceUIs[resourceType].gameObject.activeSelf)
                    activeResourceUIs[resourceType].gameObject.SetActive(true);
            }
        }

        private void EnableResourceTracking()
        {
            ResetResources();
            resourcesList.gameObject.SetActive(true);
        }

        private void DisableResourceTracking()
        {
            resourcesList.gameObject.SetActive(false);
        }

        public void ResetResources()
        {
            InitializeResourceDictionary();
        }

        public void AddResource(ResourceType resourceType, int amount)
        {
            if (resourceType == ResourceType.NONE || amount <= 0) return;
            
            if (gatheredResources.ContainsKey(resourceType))
                gatheredResources[resourceType] += amount;
            else
                gatheredResources[resourceType] = amount;
            
            UpdateResourceUI(resourceType);
        }
        
        public void RemoveResource(ResourceType resourceType, int amount)
        {
            if (resourceType == ResourceType.NONE || amount <= 0 || 
                !gatheredResources.ContainsKey(resourceType)) return;
            
            gatheredResources[resourceType] = Mathf.Max(0, gatheredResources[resourceType] - amount);
            UpdateResourceUI(resourceType);
        }
        
        private void UpdateResourceUI(ResourceType resourceType)
        {
            if (gatheredResources.TryGetValue(resourceType, out int amount))
                CreateOrUpdateResourceUI(resourceType, amount);
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
    }
}