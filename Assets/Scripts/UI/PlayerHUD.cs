using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using RobbieWagnerGames.Utilities;
using TMPro;
using System;
using DG.Tweening;
using System.Collections;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
    public class PlayerHUD : MonoBehaviourSingleton<PlayerHUD>
    {
        [Header("Resource UI")]
        [SerializeField] private Canvas canvas;
        [SerializeField] private LayoutGroup resourcesList;
        [SerializeField] private ResourceUI resourceUIPrefab;
        [SerializeField] private bool showAllResources = true;
        [SerializeField] private bool hideEmptyResources = true;
        [SerializeField] private Image levelStartGraphic;
        
        [Header("Level Info")]
        [SerializeField] private AsteroidsLevelController levelController;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private Slider timeSlider;
        [SerializeField] private Image timeSliderFill;

        [Header("Health Display")]
        [SerializeField] private LayoutGroup healthImageParent;
        private List<Image> healthSprites = new List<Image>();
        [SerializeField] private Image healthSpritePrefab;
        
        private Dictionary<ResourceType, ResourceUI> activeResourceUIs = new Dictionary<ResourceType, ResourceUI>();
        
        protected override void Awake()
        {
            base.Awake();
            
            InitializeResourceUI();
            
            ResourceManager.Instance.OnResourceAdded += OnResourceAdded;
            ResourceManager.Instance.OnResourceRemoved += OnResourceRemoved;
            ResourceManager.Instance.OnResourcesReset += OnResourcesReset;
            ResourceManager.Instance.OnResourcesUpdated += OnResourcesUpdated;

            levelController.OnLevelStarted += OnLevelStarted;
            levelController.OnLevelCompleted += OnLevelCompleted;
            levelController.LevelTimer.OnTimerUpdate += UpdateLevelTimer;
            levelController.LevelTimer.OnTimerComplete += CompleteLevelTimer;
            levelController.OnResourceAdded += OnLevelResourceAdded;
            levelController.OnResourcesUpdated += OnLevelResourcesUpdated;

            GameManager.Instance.OnReturnToMenu += HideHUD;
            GameManager.Instance.OnGameStart += ShowHUD;

            RunManager.Instance.CurrentRun.OnUpdateHealth += UpdateHealthUI;
            // update health graphic OnHealthUpdated (current run)
        }

        private void InitializeResourceUI()
        {
            ClearResourceUIs();
            
            if (showAllResources)
            {
                foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
                {
                    if (type != ResourceType.NONE)
                        CreateResourceUI(type, GetResourceAmount(type), 0);
                }
            }
        }
        
        private void ClearResourceUIs()
        {
            foreach (ResourceUI ui in activeResourceUIs.Values)
            {
                if (ui != null && ui.gameObject != null)
                    Destroy(ui.gameObject);
            }
            activeResourceUIs.Clear();
        }
        
        private void CreateResourceUI(ResourceType resourceType, int gatheredAmount, int collectedAmount)
        {
            if (resourceUIPrefab == null || resourcesList == null)
            {
                Debug.LogError("Resource UI prefab or resources list not assigned!");
                return;
            }
            
            ResourceUI newUI = Instantiate(resourceUIPrefab, resourcesList.transform);
            newUI.Initialize(resourceType, gatheredAmount, collectedAmount);
            activeResourceUIs[resourceType] = newUI;
            
            if (hideEmptyResources && gatheredAmount <= 0 && collectedAmount <= 0)
                newUI.gameObject.SetActive(false);
        }
        
        private void UpdateResourceUI(ResourceType resourceType, int gatheredAmount, int collectedAmount)
        {
            if (!activeResourceUIs.ContainsKey(resourceType) || activeResourceUIs[resourceType] == null)
                CreateResourceUI(resourceType, gatheredAmount, collectedAmount);
            else
            {
                activeResourceUIs[resourceType].UpdateAmount(gatheredAmount, collectedAmount);
                
                if (hideEmptyResources)
                {
                    if (gatheredAmount <= 0 && collectedAmount <= 0)
                        activeResourceUIs[resourceType].gameObject.SetActive(false);
                    else if (!activeResourceUIs[resourceType].gameObject.activeSelf)
                        activeResourceUIs[resourceType].gameObject.SetActive(true);
                }
            }
        }
        
        private void OnResourceAdded(ResourceType resourceType, int amount)
        {
            int gatheredAmount = GetResourceAmount(resourceType);
            int collectedAmount = GetCollectedResourceAmount(resourceType);
            UpdateResourceUI(resourceType, gatheredAmount, collectedAmount);
        }
        
        private void OnResourceRemoved(ResourceType resourceType, int amount)
        {
            int gatheredAmount = GetResourceAmount(resourceType);
            int collectedAmount = GetCollectedResourceAmount(resourceType);
            UpdateResourceUI(resourceType, gatheredAmount, collectedAmount);
        }
        
        private void OnLevelResourceAdded(ResourceType resourceType, int amount)
        {
            int gatheredAmount = GetResourceAmount(resourceType);
            int collectedAmount = GetCollectedResourceAmount(resourceType);
            UpdateResourceUI(resourceType, gatheredAmount, collectedAmount);
        }
        
        private void OnResourcesReset()
        {
            ClearResourceUIs();
            InitializeResourceUI();
        }
        
        private void OnResourcesUpdated(Dictionary<ResourceType, int> resources)
        {
            if (resources == null) return;
            
            if (!showAllResources)
            {
                foreach (KeyValuePair<ResourceType, int> resource in resources)
                {
                    if (resource.Value > 0 || HasCollectedResources(resource.Key))
                    {
                        int gatheredAmount = resource.Value;
                        int collectedAmount = GetCollectedResourceAmount(resource.Key);
                        UpdateResourceUI(resource.Key, gatheredAmount, collectedAmount);
                    }
                }
            }
            else
            {
                foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
                {
                    if (type != ResourceType.NONE)
                    {
                        int gatheredAmount = resources.ContainsKey(type) ? resources[type] : 0;
                        int collectedAmount = GetCollectedResourceAmount(type);
                        UpdateResourceUI(type, gatheredAmount, collectedAmount);
                    }
                }
            }
        }
        
        private void OnLevelResourcesUpdated(Dictionary<ResourceType, int> collectedResources)
        {
            if (collectedResources == null) return;
            
            foreach (KeyValuePair<ResourceType, int> resource in collectedResources)
            {
                if (resource.Key != ResourceType.NONE)
                {
                    int gatheredAmount = GetResourceAmount(resource.Key);
                    UpdateResourceUI(resource.Key, gatheredAmount, resource.Value);
                }
            }
        }
        
        private int GetResourceAmount(ResourceType resourceType)
        {
            if (ResourceManager.Instance != null && ResourceManager.Instance.gatheredResources.ContainsKey(resourceType))
                return ResourceManager.Instance.gatheredResources[resourceType];
            return 0;
        }
        
        private int GetCollectedResourceAmount(ResourceType resourceType)
        {
            if (levelController != null && levelController.collectedResources.ContainsKey(resourceType))
                return levelController.collectedResources[resourceType];
            return 0;
        }
        
        private bool HasCollectedResources(ResourceType resourceType)
        {
            return levelController != null && levelController.collectedResources.ContainsKey(resourceType) && 
                   levelController.collectedResources[resourceType] > 0;
        }

        private void UpdateLevelTimer(float time)
        {
            timeSlider.value = time;
        }

        private void CompleteLevelTimer()
        {
            timeSliderFill.color = Color.white;
        }
        
        private void OnLevelStarted()
        {
            Level level = levelController.levelDetails;

            if (level.levelDuration > 0)
            {
                timeSlider.minValue = 0;
                timeSlider.maxValue = level.levelDuration;
                timeSlider.value = 0;
                timeSlider.gameObject.SetActive(true);
            }
            else
                timeSlider.gameObject.SetActive(false);

            levelText.text = level.levelType.ToString();
            
            foreach (ResourceUI ui in activeResourceUIs.Values)
            {
                if (ui != null)
                {
                    int gatheredAmount = GetResourceAmount(ui.GetResourceType());
                    ui.UpdateAmount(gatheredAmount, 0, false);
                }
            }

            RectTransform rectTransform = healthImageParent.GetComponent<RectTransform>();
            int hearts = RunManager.Instance.CurrentRun.startingHealth;
            float heartWidth = healthSpritePrefab.rectTransform.rect.width;
            float spacing = 5f;
            
            float totalWidth = heartWidth * hearts + spacing * (hearts - 1);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, totalWidth);
            UpdateHealthUI(RunManager.Instance.CurrentRun.health);
        }

        public IEnumerator DisplayLevelStart(Action callback = null)
        {
            levelStartGraphic.rectTransform.DOAnchorPos(Vector2.zero, .5f);
            yield return levelStartGraphic.DOColor(Color.white, .5f).WaitForCompletion();
            yield return new WaitForSeconds(.5f);
            levelStartGraphic.rectTransform.DOAnchorPos(Vector2.left * 550, .5f);
            yield return levelStartGraphic.DOColor(Color.clear, .5f).WaitForCompletion();

            callback?.Invoke();
        }

        private void UpdateHealthUI(int health)
        {
            foreach(Image sprite in healthSprites)
            {
                if(sprite != null)
                    Destroy(sprite.gameObject);
            }
            healthSprites.Clear();

            for(int i = 0; i < health; i++)
            {
                Image sprite = Instantiate(healthSpritePrefab, healthImageParent.transform);
                healthSprites.Add(sprite);
            }
        }
        
        private void OnLevelCompleted()
        {
            HideHUD();
        }
        
        public void ShowHUD()
        {
            if(canvas != null)
                canvas.enabled = true;
        }
        
        public void HideHUD()
        {
            if(canvas != null)
                canvas.enabled = false;
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
 
                ResourceManager.Instance.OnResourceAdded -= OnResourceAdded;
                ResourceManager.Instance.OnResourceRemoved -= OnResourceRemoved;
                ResourceManager.Instance.OnResourcesReset -= OnResourcesReset;
                ResourceManager.Instance.OnResourcesUpdated -= OnResourcesUpdated;

                levelController.OnLevelStarted -= OnLevelStarted;
                levelController.OnLevelCompleted -= OnLevelCompleted;
                levelController.LevelTimer.OnTimerUpdate -= UpdateLevelTimer;
                levelController.LevelTimer.OnTimerComplete -= CompleteLevelTimer;
                levelController.OnResourceAdded -= OnLevelResourceAdded;
                levelController.OnResourcesUpdated -= OnLevelResourcesUpdated;

                GameManager.Instance.OnReturnToMenu -= HideHUD;
                GameManager.Instance.OnGameStart -= ShowHUD;

                RunManager.Instance.CurrentRun.OnUpdateHealth -= UpdateHealthUI;
        }
    }
}