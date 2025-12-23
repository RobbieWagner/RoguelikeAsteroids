using UnityEngine;
using RobbieWagnerGames.Utilities;
using UnityEngine.UI;
using System;
using RobbieWagnerGames.RoguelikeAsteroids;
using System.Collections.Generic;
using TMPro;
using DG.Tweening;

namespace RobbieWagnerGames
{
    public class GameOverScreen : MonoBehaviour
    {
        [Header("Game Over UI")]
        [SerializeField] private Canvas gameOverScreen;
        [field: SerializeField] public Button retryButton { get; private set; }
        [field: SerializeField] public Button mainMenuButton { get; private set; }
        
        [Header("Resource Display")]
        [SerializeField] private Transform resourceUIParent;
        [SerializeField] private ResourceUI resourceUIPrefab;
        [SerializeField] private float resourceDisplayDelay = 0.2f;
        [SerializeField] private float animationDuration = 0.5f;
        
        [Header("Game Over Text")]
        [SerializeField] private TextMeshProUGUI gameOverTitle;
        [SerializeField] private string gameOverTitleText = "GAME OVER";
        
        private Dictionary<ResourceType, ResourceUI> displayedResources = new Dictionary<ResourceType, ResourceUI>();
        private Sequence displaySequence;

        private void Awake()
        {
            ToggleGameOverUI(false);
            GameManager.Instance.OnGameOver += OnGameOver;
        }

        private void OnDestroy()
        {
            GameManager.Instance.OnGameOver -= OnGameOver;
            displaySequence?.Kill();
        }

        private void OnGameOver()
        {
            DisplayGameOverScreen();
        }


        public void DisplayGameOverScreen()
        {
            ToggleGameOverUI(true);
            gameOverTitle.text = gameOverTitleText;
            DisplayFinalResources();
        }

        private void DisplayFinalResources()
        {
            Dictionary<ResourceType, int> finalResources = ResourceManager.Instance.gatheredResources;
            
            if (finalResources == null || finalResources.Count == 0)
            {
                Debug.LogWarning("No resources to display!");
                return;
            }
            
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

        public void ToggleGameOverUI(bool on)
        {
            gameOverScreen.enabled = on;
            
            if (on)
            {
                retryButton.interactable = false;
                mainMenuButton.interactable = false;
                
                retryButton.transform.localScale = Vector3.zero;
                mainMenuButton.transform.localScale = Vector3.zero;
                
                DOVirtual.DelayedCall(1f, () =>
                {
                    retryButton.interactable = true;
                    mainMenuButton.interactable = true;
                    
                    retryButton.transform.localScale = Vector3.one;
                    mainMenuButton.transform.localScale = Vector3.one;
                });
            }
        }
    }
}