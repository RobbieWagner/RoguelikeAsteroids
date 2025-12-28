using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using RobbieWagnerGames.Utilities;
using TMPro;
using UnityEngine.InputSystem;

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
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI timerText;
        
        private Dictionary<ResourceType, ResourceUI> activeResourceUIs = new Dictionary<ResourceType, ResourceUI>();
        private float levelTimer = 0f;
        private bool isTimerRunning = false;
        
        protected override void Awake()
        {
            base.Awake();
            
            InitializeResourceUI();
            
            ResourceManager.Instance.OnResourceAdded += OnResourceAdded;
            ResourceManager.Instance.OnResourceRemoved += OnResourceRemoved;
            ResourceManager.Instance.OnResourcesReset += OnResourcesReset;
            ResourceManager.Instance.OnResourcesUpdated += OnResourcesUpdated;

            RunManager.Instance.OnStartNextLevel += OnLevelStarted;
            LevelManager.Instance.OnLevelCompleted += OnLevelCompleted;
            LevelManager.Instance.OnLevelFailed += OnLevelFailed;

            GameManager.Instance.OnReturnToMenu += HideHUD;
            GameManager.Instance.OnGameStart += ShowHUD;
        }

        private void Update()
        {
            if (isTimerRunning)
            {
                levelTimer -= Time.deltaTime;
                UpdateTimerDisplay();
                
                if (levelTimer <= 0f)
                {
                    isTimerRunning = false;
                    levelTimer = 0f;
                    UpdateTimerDisplay();
                }
            }
        }
        
        private void InitializeResourceUI()
        {
            ClearResourceUIs();
            
            if (showAllResources)
            {
                foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
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
                foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
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
        
        private void OnLevelStarted(Level level)
        {
			if (level.levelDuration > 0)
			{
				levelTimer = level.levelDuration;
				isTimerRunning = true;
			}
			else
			{
				isTimerRunning = false;
				timerText.text = "--:--";
			}
        }

        private void OnLevelFailed(Level level)
        {
            isTimerRunning = false;
            HideHUD();
        }
        
        private void OnLevelCompleted(Level level)
        {
            isTimerRunning = false;
            HideHUD();
        }
        
        private void UpdateTimerDisplay()
        {
            if (timerText != null)
            {
                int minutes = Mathf.FloorToInt(levelTimer / 60);
                int seconds = Mathf.FloorToInt(levelTimer % 60);
                timerText.text = $"{minutes:00}:{seconds:00}";
            }
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

            RunManager.Instance.OnStartNextLevel -= OnLevelStarted;
            LevelManager.Instance.OnLevelCompleted -= OnLevelCompleted;

            GameManager.Instance.OnReturnToMenu -= HideHUD;
            GameManager.Instance.OnGameStart -= ShowHUD;
        }
    }
}