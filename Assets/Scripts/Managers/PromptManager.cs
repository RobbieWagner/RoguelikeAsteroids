using UnityEngine;
using System;
using RobbieWagnerGames.Utilities;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
    public class PromptManager : MonoBehaviourSingleton<PromptManager>
    {
        [Header("UI Reference")]
        [SerializeField] private PromptMenu promptMenu;
        
        [Header("Default Settings")]
        [SerializeField] private string defaultConfirmText = "Confirm";
        [SerializeField] private string defaultCancelText = "Cancel";
        [SerializeField] private Color defaultConfirmColor = new Color(0.2f, 0.8f, 0.2f); // Green
        [SerializeField] private Color defaultCancelColor = new Color(0.8f, 0.2f, 0.2f); // Red
        [SerializeField] private bool defaultPauseGame = true;
        [SerializeField] private bool defaultCanCloseWithEscape = true;
        
        public void ShowPrompt(PromptData promptData)
        {
            if (promptMenu.IsOpen)
            {
                Debug.LogWarning("A prompt is already showing. Cannot show another until current is dismissed.");
                return;
            }
            
            if (string.IsNullOrEmpty(promptData.confirmButtonText))
                promptData.confirmButtonText = defaultConfirmText;
            
            if (string.IsNullOrEmpty(promptData.cancelButtonText))
                promptData.cancelButtonText = defaultCancelText;
            
            if (promptData.confirmButtonColor == Color.clear)
                promptData.confirmButtonColor = defaultConfirmColor;
            
            if (promptData.cancelButtonColor == Color.clear)
                promptData.cancelButtonColor = defaultCancelColor;
             
            promptMenu.Show(promptData);
        }
        
        public void ShowConfirmationPrompt(string title, string description, Action onConfirm, Action onCancel = null)
        {
            PromptData promptData = new PromptData
            {
                title = title,
                description = description,
                confirmButtonText = "Yes",
                cancelButtonText = "No",
                confirmButtonColor = defaultConfirmColor,
                cancelButtonColor = defaultCancelColor,
                OnConfirm = onConfirm,
                OnCancel = onCancel ?? null
            };
            
            ShowPrompt(promptData);
        }
        
        public void ShowInfoPrompt(string title, string description, Action onConfirm = null)
        {
            PromptData promptData = new PromptData
            {
                title = title,
                description = description,
                confirmButtonText = "OK",
                cancelButtonText = "",
                confirmButtonColor = defaultConfirmColor,
                OnConfirm = onConfirm ?? null,
                OnCancel = null
            };
            
            ShowPrompt(promptData);
        }
        
        public void ShowErrorPrompt(string title, string description, Action onConfirm = null)
        {
            PromptData promptData = new PromptData
            {
                title = title,
                description = description,
                confirmButtonText = "OK",
                cancelButtonText = "", 
                confirmButtonColor = new Color(0.8f, 0.2f, 0.2f),
                OnConfirm = onConfirm ?? null,
                OnCancel = null
            };
            
            ShowPrompt(promptData);
		}
    }
}