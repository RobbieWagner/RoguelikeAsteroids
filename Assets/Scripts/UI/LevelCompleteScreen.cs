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
        
        [Header("Resource Display")]
        [SerializeField] private ResourceUI resourceUIPrefab;
        
        private List<ResourceUI> displayedResources = new List<ResourceUI>();

        public IEnumerator DisplayScreen()
        {
            ClearResources();
            
            canvasGroup.alpha = 0;
            canvas.enabled = true;
            yield return canvasGroup.DOFade(1, fadeDuration).WaitForCompletion();

            yield return StartCoroutine(DisplayResourcesAnimated());
            yield return new WaitForSeconds(displayDuration);
        }

        private IEnumerator DisplayResourcesAnimated()
        {
            Dictionary<ResourceType, int> resources = ResourceManager.Instance.gatheredResources;
            
            float resourceWidth = 100; 
            int validCount = 0;
            
            foreach (KeyValuePair<ResourceType, int> resource in resources)
            {
                if (resource.Key == ResourceType.NONE || resource.Value <= 0) 
                    continue;
                
                ResourceUI resourceUI = Instantiate(resourceUIPrefab, resourceLayout.transform);
                resourceUI.Initialize(resource.Key, resource.Value);
                displayedResources.Add(resourceUI);
                validCount++;
            }
            
            float targetWidth = validCount * resourceWidth;
            resourceLayoutTransform.sizeDelta = new Vector2(0, resourceLayoutTransform.sizeDelta.y);
            yield return resourceLayoutTransform.DOSizeDelta(new Vector2(targetWidth, resourceLayoutTransform.sizeDelta.y), 0.5f)
                .SetEase(Ease.OutBack)
                .WaitForCompletion();
        }

        private void ClearResources()
        {
            foreach (ResourceUI resourceUI in displayedResources)
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