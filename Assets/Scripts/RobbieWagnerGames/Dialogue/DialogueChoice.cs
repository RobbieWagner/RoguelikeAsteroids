using UnityEngine;
using TMPro;
using Ink.Runtime;
using UnityEngine.UI;
using DG.Tweening;

namespace RobbieWagnerGames.Dialogue
{
    /// <summary>
    /// Represents a single choice in a dialogue system
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class DialogueChoice : MonoBehaviour
    {
        [Header("Visual Settings")]
        [SerializeField] private Color inactiveColor = Color.gray;
        [SerializeField] private Color activeColor = Color.white;
        [SerializeField] private float fadeDuration = 0.2f;

        [Header("References")]
        [SerializeField] private TextMeshProUGUI choiceText;
        [SerializeField] private Button choiceButton;

        public Choice InkChoice { get; private set; }

        private void Awake()
        {
            if (choiceButton == null) choiceButton = GetComponent<Button>();
            SetInactive();
        }

        /// <summary>
        /// Initialize the choice with Ink story data
        /// </summary>
        public void Initialize(Choice inkChoice)
        {
            InkChoice = inkChoice;
            choiceText.text = inkChoice.text;
        }

        /// <summary>
        /// Visual feedback when choice is selected
        /// </summary>
        public void SetActive()
        {
            choiceText.DOColor(activeColor, fadeDuration);
        }

        /// <summary>
        /// Visual feedback when choice is not selected
        /// </summary>
        public void SetInactive()
        {
            choiceText.DOColor(inactiveColor, fadeDuration);
        }
    }
}