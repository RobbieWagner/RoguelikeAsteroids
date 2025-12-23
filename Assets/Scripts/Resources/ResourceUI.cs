using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
    public class ResourceUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private Image background;
        
        private ResourceType resourceType;
        private int currentAmount;
        private Color resourceColor;
        private Sequence highlightSequence;
        private Sequence updateSequence;
        
        public void Initialize(ResourceType type, int initialAmount)
        {
            resourceType = type;
            
            if (GameConstants.Instance.resourceColors.TryGetValue(type, out Color color))
                resourceColor = color;
            else
                resourceColor = Color.white;
            if (text != null)
                text.color = resourceColor;
            
            if (background != null)
                background.color = new Color(resourceColor.r, resourceColor.g, resourceColor.b, 0f);
            
            UpdateAmount(initialAmount, false);
        }
        
        public void UpdateAmount(int newAmount, bool highlight = true)
        {
            int oldAmount = currentAmount;
            currentAmount = newAmount;
            UpdateText();
            
            if (newAmount > oldAmount && highlight)
                Highlight();
        }
        
        private void UpdateText()
        {
            if (text != null)
                text.text = $"{resourceType}: {currentAmount}";
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
        
        public int GetCurrentAmount() => currentAmount;
    }
}