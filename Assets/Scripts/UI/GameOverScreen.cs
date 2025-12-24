using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using DG.Tweening;
using RobbieWagnerGames.UI;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
    public class GameOverScreen : Menu
    {
        [Header("Game Over UI")]
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

        protected override void Awake()
        {
            base.Awake();
            GameManager.Instance.OnGameOver += OnGameOver;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            GameManager.Instance.OnGameOver -= OnGameOver;
            displaySequence?.Kill();
        }

        private void OnGameOver()
        {
            DisplayGameOverScreen();
        }

        public void DisplayGameOverScreen()
        {
            Open();
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
                retryButton.interactable = true;
                mainMenuButton.interactable = true;
                
                RefreshSelectableElements();
                SetupNavigation();
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

        public override void Open()
        {
            base.Open();
            
            retryButton.interactable = false;
            mainMenuButton.interactable = false;
        }

        public override void Close()
        {
            base.Close();
            displaySequence?.Kill();
        }

        protected override void SetupNavigation()
        {
            if (!retryButton.interactable)
            {
                Invoke(nameof(SetupNavigation), 0.1f);
                return;
            }
            
            base.SetupNavigation();
        }

        public void ToggleGameOverUI(bool on)
        {
            if (on)
                Open();
            else
                Close();
        }
    }
}