using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using RobbieWagnerGames.UI;
using System;
using UnityEngine.EventSystems;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
    public class PromptMenu : Menu
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private TextMeshProUGUI confirmButtonText;
        [SerializeField] private TextMeshProUGUI cancelButtonText;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform containerTransform;
        
        [Header("Animation Settings")]
        [SerializeField] private float fadeDuration = 0.2f;
        [SerializeField] private float scaleDuration = 0.2f;
        [SerializeField] private float startingScale = 0.8f;
        
        private PromptData currentPromptData;
        private Tween fadeTween;
        private Tween scaleTween;
        
        protected override void Awake()
        {
            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            
            confirmButton.onClick.AddListener(OnConfirmClicked);
            cancelButton.onClick.AddListener(OnCancelClicked);

			base.Awake();
        }
        
        public void Show(PromptData promptData)
        {
            currentPromptData = promptData;
            
            titleText.text = promptData.title;
            descriptionText.text = promptData.description;
            confirmButtonText.text = promptData.confirmButtonText;
            cancelButtonText.text = promptData.cancelButtonText;
            
            if (promptData.confirmButtonColor != Color.clear)
                confirmButtonText.color = promptData.confirmButtonColor;
            
            if (promptData.cancelButtonColor != Color.clear)
                cancelButtonText.color = promptData.cancelButtonColor;
				
            cancelButton.gameObject.SetActive(!string.IsNullOrEmpty(promptData.cancelButtonText));

            Open();
        }
        
        public override void Open()
        {
            fadeTween?.Kill();
            scaleTween?.Kill();

            firstSelected = cancelButton.gameObject.activeSelf ? cancelButton : confirmButton;
            
            base.Open();
            
            canvas.enabled = true;
            
            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            
            if (containerTransform != null)
                containerTransform.localScale = Vector3.one * startingScale;
            
            fadeTween = canvasGroup.DOFade(1f, fadeDuration)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true) 
                .OnComplete(() => {
                    canvasGroup.blocksRaycasts = true;
                    canvasGroup.interactable = true;
                    OnFadeInComplete();
                });
            
            if (containerTransform != null)
            {
                scaleTween = containerTransform.DOScale(Vector3.one, scaleDuration)
                    .SetEase(Ease.OutBack)
                    .SetUpdate(true);
            }
        }
        
        private void OnFadeInComplete()
        {
            SetupNavigation();
            
            if (confirmButton.interactable)
                ForceSelectElement(confirmButton);
        }
        
        protected override void SetupNavigation()
        {
            RefreshSelectableElements();
            
            Navigation confirmNav = confirmButton.navigation;
            Navigation cancelNav = cancelButton.navigation;
            
            if (cancelButton.gameObject.activeSelf)
            {
                confirmNav.mode = Navigation.Mode.Explicit;
                cancelNav.mode = Navigation.Mode.Explicit;
                
                confirmNav.selectOnRight = cancelButton;
                
                cancelNav.selectOnLeft = confirmButton;
                
                confirmButton.navigation = confirmNav;
                cancelButton.navigation = cancelNav;
            }
            
            lastMouseActivityTime = Time.time - mouseInactivityTimeout;
        }
        
        private void OnConfirmClicked()
        {
            if (IsValidSelection())
                HideWithAnimation(() => currentPromptData.OnConfirm?.Invoke());
        }
        
        private void OnCancelClicked()
        {
            if (IsValidSelection())
                HideWithAnimation(() => currentPromptData.OnCancel?.Invoke());
        }
        
        private bool IsValidSelection()
        {
            GameObject selected = EventSystem.current.currentSelectedGameObject;
            return selected == confirmButton.gameObject || selected == cancelButton.gameObject;
        }
        
        private void HideWithAnimation(Action onComplete)
        {
            if (!IsOpen) return;

            fadeTween?.Kill();
            scaleTween?.Kill();
            
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            
            fadeTween = canvasGroup.DOFade(0f, fadeDuration)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true) 
                .OnComplete(() => {
                    OnHideComplete();
                    onComplete?.Invoke();
                });
            
            if (containerTransform != null)
            {
                scaleTween = containerTransform.DOScale(Vector3.one * startingScale, scaleDuration)
                    .SetEase(Ease.OutQuad)
                    .SetUpdate(true); 
            }
        }
        
        private void OnHideComplete()
        {
            base.Close();

            fadeTween = null;
            scaleTween = null;
        }
        
        protected override void OnDestroy()
        {
            fadeTween?.Kill();
            scaleTween?.Kill();
            base.OnDestroy();
        }
    }
}