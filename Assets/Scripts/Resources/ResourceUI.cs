using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
using UnityEngine.Serialization;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
     public class ResourceUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI resourceText;
        [SerializeField] private TextMeshProUGUI collectedText;
        [SerializeField] private Image background;
        
        private ResourceType resourceType;
        private int gatheredAmount;
        private int collectedAmount;
        private Color resourceColor;
        private Sequence highlightSequence;
        private Sequence updateSequence;
        
        public void Initialize(ResourceType type, int initialGatheredAmount, int initialCollectedAmount = 0)
        {
            resourceType = type;
            resourceColor = GameConstants.Instance.resourceColors[type];
            resourceText.color = resourceColor;
            background.color = new Color(resourceColor.r, resourceColor.g, resourceColor.b, 0f);
            
            gatheredAmount = initialGatheredAmount;
            collectedAmount = initialCollectedAmount;
            UpdateText();
        }
        
        public void UpdateAmount(int newGatheredAmount, int newCollectedAmount = 0, bool highlight = true)
        {
            int oldGatheredAmount = gatheredAmount;
            gatheredAmount = newGatheredAmount;
            collectedAmount = newCollectedAmount;
            UpdateText();
            
            if (newGatheredAmount > oldGatheredAmount && highlight)
                Highlight();
        }
        
        public void SetCollectedAmount(int amount)
        {
            collectedAmount = amount;
            UpdateText();
        }
        
        private void UpdateText()
        {
            if (collectedAmount > 0)
            {
                resourceText.text = $"{resourceType}: {gatheredAmount}";
                collectedText.text = $"+ {collectedAmount}";
                collectedText.color = resourceColor;
            }
            else
            {
                resourceText.text = $"{resourceType}: {gatheredAmount}";
                collectedText.text = "";
            }
        }
        
        public IEnumerator TransferAnimation(float duration = 1f)
        {
            if (collectedAmount <= 0) yield break;
            
            int startGathered = gatheredAmount;
            int startCollected = collectedAmount;
            int transferAmount = startCollected;
            
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                int transferred = Mathf.RoundToInt(transferAmount * t);
                
                gatheredAmount = startGathered + transferred;
                collectedAmount = startCollected - transferred;
                UpdateText();
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            gatheredAmount = startGathered + transferAmount;
            collectedAmount = 0;
            UpdateText();
        }
        
        public void Highlight(float duration = 1f)
        {
            PlayHighlightAnimation(duration);
        }
        
        private void PlayHighlightAnimation(float duration = 1f)
        {
            if (background == null) return;
            highlightSequence?.Kill();
            highlightSequence = DOTween.Sequence();
            
            highlightSequence.Append(background.DOFade(1f, duration * 0.3f));
            highlightSequence.AppendInterval(duration * 0.2f);
            highlightSequence.Append(background.DOFade(0f, duration * 0.5f));
            highlightSequence.SetEase(Ease.InOutSine);
            highlightSequence.SetAutoKill(true);
        }
        
        private void OnEnable()
        {
            if (background != null && background.color.a > 0)
                background.color = new Color(resourceColor.r, resourceColor.g, resourceColor.b, 0f);
        }
        
        private void OnDisable()
        {
            highlightSequence?.Kill();
            updateSequence?.Kill();
        }
        
        private void OnDestroy()
        {
            highlightSequence?.Kill();
            updateSequence?.Kill();
        }
        
        public ResourceType GetResourceType() => resourceType;
        
        public int GetGatheredAmount() => gatheredAmount;
        public int GetCollectedAmount() => collectedAmount;
    }
}