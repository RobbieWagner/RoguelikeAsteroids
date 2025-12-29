using System.Collections.Generic;
using DG.Tweening;
using RobbieWagnerGames.UI;
using UnityEngine;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
    public class LevelCompleteScreen : Menu
    {
        [Header("Resource Display")]
        [SerializeField] private Transform resourceUIParent;
        [SerializeField] private ResourceUI resourceUIPrefab;
        [SerializeField] private float resourceDisplayDelay = 0.2f;
        [SerializeField] private float animationDuration = 0.5f;

        private Dictionary<ResourceType, ResourceUI> displayedResources = new Dictionary<ResourceType, ResourceUI>();
        private Sequence displaySequence;

        private void DisplayFinalResources()
        {
            Dictionary<ResourceType, int> finalResources = ResourceManager.Instance.gatheredResources;
            if (finalResources == null || finalResources.Count == 0)
            {
                Debug.LogWarning("No resources to display!");
                return;
            }
            
            foreach (Transform child in resourceUIParent)
                Destroy(child.gameObject);
            displayedResources.Clear();
            
            displaySequence?.Kill();
            displaySequence = DOTween.Sequence();
            
            float delay = 0f;
            int totalResources = 0;
            foreach (var resource in finalResources)
            {
                if (resource.Key == ResourceType.NONE || resource.Value <= 0) continue;
                
                totalResources += resource.Value;
                
                displaySequence.AppendCallback(() => DisplayResource(resource.Key, resource.Value));
                displaySequence.AppendInterval(resourceDisplayDelay);
                
                delay += resourceDisplayDelay;
            }
            
            displaySequence.OnComplete(() =>
            {
            });
        }

        private void DisplayResource(ResourceType resourceType, int amount)
        {
            ResourceUI resourceUI = Instantiate(resourceUIPrefab, resourceUIParent);
            
            resourceUI.Initialize(resourceType, amount);
            
            displayedResources[resourceType] = resourceUI;
            
            resourceUI.transform.localScale = Vector3.zero;
            resourceUI.transform.DOScale(Vector3.one, animationDuration)
                .SetEase(Ease.OutBack);
        }
    }
}