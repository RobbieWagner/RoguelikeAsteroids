using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
    public class LevelCompleteScreen : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Canvas canvas;
        [SerializeField] private HorizontalLayoutGroup resourceLayout;
        [SerializeField] private RectTransform resourceLayoutTransform;
        [SerializeField] private float displayDuration = 2f;
        [SerializeField] private float fadeDuration = 0.5f;
        [SerializeField] private float transferAnimationDuration = 1.5f;
        
        [Header("Resource Display")]
        [SerializeField] private ResourceUI resourceUIPrefab;

        [Header("Audio")]
        [SerializeField] private AudioSource victorySound;
        [SerializeField] private AudioSource resourceCountSound;
        
        private Dictionary<ResourceType, ResourceUI> displayedResources = new Dictionary<ResourceType, ResourceUI>();
        private AsteroidsLevelController levelController;

        private void Awake()
        {
            levelController = FindFirstObjectByType<AsteroidsLevelController>();
        }

        public IEnumerator DisplayScreen()
        {
            ClearResources();
            
            canvasGroup.alpha = 0;
            canvas.enabled = true;
            yield return canvasGroup.DOFade(1, fadeDuration).WaitForCompletion();

            victorySound.Play();

            yield return StartCoroutine(DisplayResourcesAnimated());
            yield return new WaitForSeconds(displayDuration);
            
            yield return StartCoroutine(AnimateResourceTransfer());
            yield return new WaitForSeconds(0.5f);
        }

        private IEnumerator DisplayResourcesAnimated()
        {
            Dictionary<ResourceType, int> gatheredResources = ResourceManager.Instance.gatheredResources;
            Dictionary<ResourceType, int> collectedResources = levelController != null ? 
                levelController.collectedResources : new Dictionary<ResourceType, int>();
            
            float resourceWidth = 150;
            int validCount = 0;
            
            foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
            {
                if (type == ResourceType.NONE) continue;
                
                int gatheredAmount = gatheredResources.ContainsKey(type) ? gatheredResources[type] : 0;
                int collectedAmount = collectedResources.ContainsKey(type) ? collectedResources[type] : 0;
                
                if (gatheredAmount > 0 || collectedAmount > 0)
                {
                    ResourceUI resourceUI = Instantiate(resourceUIPrefab, resourceLayout.transform);
                    resourceUI.Initialize(type, gatheredAmount, collectedAmount);
                    displayedResources[type] = resourceUI;
                    validCount++;
                }
            }
            
            float targetWidth = validCount * resourceWidth;
            resourceLayoutTransform.sizeDelta = new Vector2(0, resourceLayoutTransform.sizeDelta.y);
            yield return resourceLayoutTransform.DOSizeDelta(new Vector2(targetWidth, resourceLayoutTransform.sizeDelta.y), 0.5f)
                .SetEase(Ease.OutBack)
                .WaitForCompletion();
   
        }

        private IEnumerator AnimateResourceTransfer()
        {
            List<Coroutine> transferCoroutines = new List<Coroutine>();
            
            foreach (KeyValuePair<ResourceType, ResourceUI> displayedResource in displayedResources)
            {
                if (displayedResource.Value != null && displayedResource.Value.GetCollectedAmount() > 0)
                {
                    Coroutine transferCo = StartCoroutine(displayedResource.Value.TransferAnimation(transferAnimationDuration));
                    transferCoroutines.Add(transferCo);
                }
            }
            
            foreach (Coroutine coroutine in transferCoroutines)
            {
                resourceCountSound.loop = true;
                resourceCountSound.Play();
                
                yield return coroutine;

                resourceCountSound.loop = false;
            }

            foreach (KeyValuePair<ResourceType, int> resource in levelController.collectedResources)
            {
                if (resource.Key != ResourceType.NONE && resource.Value > 0)
                    ResourceManager.Instance.AddResource(resource.Key, resource.Value);
            }
        }

        private void ClearResources()
        {
            foreach (ResourceUI resourceUI in displayedResources.Values)
            {
                if (resourceUI != null && resourceUI.gameObject != null)
                    Destroy(resourceUI.gameObject);
            }
            displayedResources.Clear();
        }

        private void OnDestroy()
        {
            ClearResources();
            canvasGroup.DOKill();
        }
    }
}