using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using RobbieWagnerGames.Utilities;
using TMPro;
using UnityEngine.InputSystem;
using System;

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
        
        [Header("Level Info")]
        [SerializeField] private AsteroidsLevelController levelController;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private Slider timeSlider;
        [SerializeField] private Image timeSliderFill;
        
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
            levelController.OnLevelFailed += OnLevelFailed;
            levelController.LevelTimer.OnTimerUpdate += UpdateLevelTimer;
            levelController.LevelTimer.OnTimerComplete += CompleteLevelTimer;

            GameManager.Instance.OnReturnToMenu += HideHUD;
            GameManager.Instance.OnGameStart += ShowHUD;
        }
        
        private void InitializeResourceUI()
        {
            ClearResourceUIs();
            
            if (showAllResources)
            {
                foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
                {
                    if (type != ResourceType.NONE)
                        CreateResourceUI(type, 0);
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
        
        private void CreateResourceUI(ResourceType resourceType, int amount)
        {
            if (resourceUIPrefab == null || resourcesList == null)
            {
                Debug.LogError("Resource UI prefab or resources list not assigned!");
                return;
            }
            
            ResourceUI newUI = Instantiate(resourceUIPrefab, resourcesList.transform);
            newUI.Initialize(resourceType, amount);
            activeResourceUIs[resourceType] = newUI;
            
            if (hideEmptyResources && amount <= 0)
                newUI.gameObject.SetActive(false);
        }
        
        private void UpdateResourceUI(ResourceType resourceType, int amount)
        {
            if (!activeResourceUIs.ContainsKey(resourceType) || activeResourceUIs[resourceType] == null)
                CreateResourceUI(resourceType, amount);
            else
            {
                activeResourceUIs[resourceType].UpdateAmount(amount);
                
                if (hideEmptyResources)
                {
                    if (amount <= 0)
                        activeResourceUIs[resourceType].gameObject.SetActive(false);
                    else if (!activeResourceUIs[resourceType].gameObject.activeSelf)
                        activeResourceUIs[resourceType].gameObject.SetActive(true);
                }
            }
        }
        
        private void OnResourceAdded(ResourceType resourceType, int amount)
        {
            UpdateResourceUI(resourceType, GetResourceAmount(resourceType));
        }
        
        private void OnResourceRemoved(ResourceType resourceType, int amount)
        {
            UpdateResourceUI(resourceType, GetResourceAmount(resourceType));
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
                    if (resource.Value > 0)
                        UpdateResourceUI(resource.Key, resource.Value);
                }
            }
            else
            {
                foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
                {
                    if (type != ResourceType.NONE)
                    {
                        int amount = resources.ContainsKey(type) ? resources[type] : 0;
                        UpdateResourceUI(type, amount);
                    }
                }
            }
        }
        
        private int GetResourceAmount(ResourceType resourceType)
        {
            if (ResourceManager.Instance != null && ResourceManager.Instance.gatheredResources.ContainsKey(resourceType))
                return ResourceManager.Instance.gatheredResources[resourceType];
            return 0;
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
			{
				timeSlider.gameObject.SetActive(false);
			}

            levelText.text = level.levelType.ToString();
        }

        private void OnLevelFailed()
        {
            HideHUD();
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
            levelController.OnLevelFailed -= OnLevelFailed;
            levelController.LevelTimer.OnTimerUpdate -= UpdateLevelTimer;
            levelController.LevelTimer.OnTimerComplete -= CompleteLevelTimer;

            GameManager.Instance.OnReturnToMenu -= HideHUD;
            GameManager.Instance.OnGameStart -= ShowHUD;
        }
    }
}