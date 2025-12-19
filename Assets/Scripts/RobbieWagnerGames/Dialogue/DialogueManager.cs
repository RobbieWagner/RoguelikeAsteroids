using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using Ink.Runtime;
using System.Text.RegularExpressions;
using DG.Tweening;
using RobbieWagnerGames.Utilities;
using RobbieWagnerGames.Managers;

namespace RobbieWagnerGames.Dialogue
{
    public class DialogueManager : MonoBehaviourSingleton<DialogueManager>
    {
        [Header("UI References")]
        [SerializeField] private Canvas dialogueCanvas;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private Image continueIcon;

        [Header("Left Speaker UI")]
        [SerializeField] private Image leftSpeakerImage;
        [SerializeField] private Image leftNamePlate;
        [SerializeField] private TextMeshProUGUI leftSpeakerNameText;

        [Header("Right Speaker UI")]
        [SerializeField] private Image rightSpeakerImage;
        [SerializeField] private Image rightNamePlate;
        [SerializeField] private TextMeshProUGUI rightSpeakerNameText;

        [Header("Choices")]
        [SerializeField] private Button choicePrefab;
        [SerializeField] private Transform choicesContainer;
        
        [Header("Settings")]
        [SerializeField] private bool enableCanvasOnStart = false;
        [SerializeField] private float characterDisplayDelay = 0.036f;
        [SerializeField] private float fadeDuration = 0.5f;
        [SerializeField] private AudioSource dialogueAudioSource;

        private Story currentStory;
        private Coroutine currentDialogueCoroutine;
        private Coroutine leftSpriteCoroutine;
        private Coroutine rightSpriteCoroutine;
        
        private readonly List<Button> choiceButtons = new List<Button>();
        private string currentSentence = string.Empty;
        private bool isTypingSentence = false;
        private bool skipSentenceTyping = false;
        private bool currentSpeakerIsLeft = true;
        private bool canContinueDialogue = false;
        private bool runningDialogue = false;

        private const string PlayerNameKey = "name";
        private const string DefaultPlayerName = "Morgan";
        private static readonly char[] AllowedNameChars = { ' ', '-', '\'', ',', '.' };

        protected override void Awake()
        {
            base.Awake();
            InitializeDialogueSystem();
        }

        private void OnEnable()
        {
            InputManager.Instance.Controls.DIALOGUE.Select.performed += OnNextLineInput;
        }

        private void OnDisable()
        {
            InputManager.Instance.Controls.DIALOGUE.Select.performed -= OnNextLineInput;
        }

        #region Initialization
        private void InitializeDialogueSystem()
        {
            ResetDialogueUI();
            dialogueCanvas.enabled = enableCanvasOnStart;
            canContinueDialogue = false;
            continueIcon.enabled = false;
        }

        private void ResetDialogueUI()
        {
            ClearSpeakerVisuals();
            dialogueText.text = string.Empty;
            RemoveAllChoices();
        }
        #endregion

        #region Dialogue Flow Control
        public void StartDialogue(Story story)
        {
            if (currentDialogueCoroutine != null)
            {
                StopCoroutine(currentDialogueCoroutine);
            }
            currentDialogueCoroutine = StartCoroutine(StartDialogueCo(story));
        }

        public IEnumerator StartDialogueCo(TextAsset inkJSONAsset)
        {
            if (inkJSONAsset == null)
            {
                Debug.LogWarning("Ink JSON Asset is null. Cannot start dialogue.");
                yield break;
            }

            Story story = new Story(inkJSONAsset.text);
            runningDialogue = true;
            yield return StartDialogueCo(story);
        }

        public IEnumerator StartDialogueCo(Story story)
        {   
            InputManager.Instance.EnableActionMap(ActionMapName.DIALOGUE, false);
            currentStory = story;
            dialogueCanvas.enabled = true;
            currentSpeakerIsLeft = true;

            ToggleSpeakerUI(leftNamePlate, leftSpeakerNameText, true);
            ToggleSpeakerUI(rightNamePlate, rightSpeakerNameText, true);

            yield return ProcessNextDialogueLine();

            while (currentDialogueCoroutine != null || runningDialogue)
            {
                yield return null;
            }
        }

        public IEnumerator EndDialogue()
        {
            yield return null;
            ResetDialogueUI();
            dialogueCanvas.enabled = false;
            currentDialogueCoroutine = null;
            currentStory = null;
            runningDialogue = false;
            InputManager.Instance.DisableActionMap(ActionMapName.DIALOGUE);
        }
        #endregion

        #region Sentence Processing
        private IEnumerator ProcessNextDialogueLine()
        {
            canContinueDialogue = false;
            RemoveAllChoices();

            yield return null;

            if (currentStory.canContinue)
            {
                dialogueText.text = string.Empty;
                isTypingSentence = true;

                currentSentence = ProcessTextTags(currentStory.Continue());
                ConfigureDialogueDisplay();

                yield return DisplaySentenceCharacterByCharacter();

                isTypingSentence = false;
                skipSentenceTyping = false;
                canContinueDialogue = true;
                DisplayDialogueChoices();
            }
            else
            {
                yield return EndDialogue();
            }
        }

        private IEnumerator DisplaySentenceCharacterByCharacter()
        {
            for (int i = 0; i < currentSentence.Length; i++)
            {
                if (skipSentenceTyping) break;

                char currentChar = currentSentence[i];
                if (currentChar == '<')
                {
                    yield return ProcessRichTextTag(i);
                }

                dialogueText.text += currentChar;
                yield return new WaitForSeconds(characterDisplayDelay);
            }

            dialogueText.text = currentSentence;
        }

        private IEnumerator ProcessRichTextTag(int index)
        {
            while (index < currentSentence.Length && currentSentence[index] != '>')
            {
                dialogueText.text += currentSentence[index];
                index++;
            }
            yield return null;
        }
        #endregion

        #region Text Configuration
        private string ProcessTextTags(string rawText)
        {
            string processedText = rawText;
            string playerName = GetValidPlayerName();
            return processedText.Replace("^NAME^", playerName);
        }

        private string GetValidPlayerName()
        {
            string name = PlayerPrefs.GetString(PlayerNameKey, DefaultPlayerName);
            
            foreach (char c in name)
            {
                if (!char.IsLetterOrDigit(c) && !Array.Exists(AllowedNameChars, x => x == c))
                {
                    return DefaultPlayerName;
                }
            }
            return name;
        }

        private void ConfigureDialogueDisplay()
        {
            var tags = currentStory.currentTags;
            string speakerName = string.Empty;
            Sprite characterSprite = null;
            bool removeLeftSprite = false;
            bool removeRightSprite = false;
            bool? placeSpriteLeft = null;

            foreach (string rawTag in tags)
            {
                string tag = NormalizeTag(rawTag);
                ProcessDialogueTag(tag, ref speakerName, ref characterSprite, 
                                 ref removeLeftSprite, ref removeRightSprite, ref placeSpriteLeft);
            }

            UpdateSpeakerUI(speakerName);
            UpdateCharacterSprites(removeLeftSprite, removeRightSprite, placeSpriteLeft, characterSprite);
        }

        private string NormalizeTag(string rawTag)
        {
            string tag = Regex.Replace(rawTag, @"\s+", "");
            tag = tag.Replace("~", " ");
            return tag.ToUpper();
        }

        private void ProcessDialogueTag(string tag, ref string speaker, ref Sprite sprite, 
                                      ref bool removeLeft, ref bool removeRight, ref bool? placeLeft)
        {
            if (tag.Contains("SPEAKERISONLEFT")) currentSpeakerIsLeft = true;
            else if (tag.Contains("SPEAKERISONRIGHT")) currentSpeakerIsLeft = false;
            
            else if (tag.Contains("PLACESPRITEONLEFT")) 
                HandleSpritePlacement(tag, "PLACESPRITEONLEFT", ref sprite, ref placeLeft, true);
            else if (tag.Contains("PLACESPRITEONRIGHT")) 
                HandleSpritePlacement(tag, "PLACESPRITEONRIGHT", ref sprite, ref placeLeft, false);
            
            else if (tag.Contains("REMOVESPRITEONLEFT")) removeLeft = true;
            else if (tag.Contains("REMOVESPRITEONRIGHT")) removeRight = true;
            else if (tag.Contains("REMOVESPRITES")) { removeLeft = true; removeRight = true; }
            
            else if (tag.Contains("LEFTSHAKESPRITE")) ShakeSprite(leftSpeakerImage, 40f);
            else if (tag.Contains("RIGHTSHAKESPRITE")) ShakeSprite(rightSpeakerImage, 40f);
            
            else if (tag.Contains("SHAKEBACKGROUND")) ShakeSprite(backgroundImage, 15f);
            else if (tag.Contains("SHOWBACKGROUND")) ShowBackground();
            else if (tag.Contains("HIDEBACKGROUND")) HideBackground();
            else if (tag.Contains("CHANGEBACKGROUNDIMMEDIATE")) ChangeBackgroundImmediate(tag);
            else if (tag.Contains("CHANGEBACKGROUND")) StartCoroutine(ChangeBackgroundWithFade(tag));
            
            else if (tag.Contains("PLAYSOUND")) PlaySoundEffect(tag);
            else if (tag.Contains("SPEAKER")) speaker = ExtractSpeakerName(tag);
            else Debug.LogWarning($"Unrecognized tag: {tag}");
        }

        private void HandleSpritePlacement(string tag, string prefix, ref Sprite sprite, ref bool? placeLeft, bool isLeft)
        {
            string spriteName = tag.Remove(tag.IndexOf(prefix), prefix.Length).ToLower();
            sprite = LoadResource<Sprite>(StaticGameStats.ResourcePaths.Characters.Base + spriteName);
            placeLeft = isLeft;
        }

        private string ExtractSpeakerName(string tag)
        {
            return tag.Remove(tag.IndexOf("SPEAKER"), 7).ToLower();
        }
        #endregion

        #region UI Management
        private void UpdateSpeakerUI(string speakerName)
        {
            if (!string.IsNullOrEmpty(speakerName))
            {
                SetCurrentSpeaker(speakerName);
                ToggleSpeakerUI(leftNamePlate, leftSpeakerNameText, currentSpeakerIsLeft);
                ToggleSpeakerUI(rightNamePlate, rightSpeakerNameText, !currentSpeakerIsLeft);
            }
            else
            {
                ToggleSpeakerUI(leftNamePlate, leftSpeakerNameText, false);
                ToggleSpeakerUI(rightNamePlate, rightSpeakerNameText, false);
            }
        }

        private void SetCurrentSpeaker(string speakerName)
        {
            string formattedName = char.ToUpper(speakerName[0]) + speakerName[1..];
            
            if (currentSpeakerIsLeft)
            {
                leftSpeakerNameText.text = formattedName;
            }
            else
            {
                rightSpeakerNameText.text = formattedName;
            }
        }

        private void ToggleSpeakerUI(Image namePlate, TextMeshProUGUI nameText, bool isActive)
        {
            namePlate.gameObject.SetActive(isActive);
            nameText.gameObject.SetActive(isActive);
        }

        private void UpdateCharacterSprites(bool removeLeft, bool removeRight, bool? placeLeft, Sprite sprite)
        {
            if (removeLeft) leftSpriteCoroutine = StartCoroutine(ToggleCharacterSprite(leftSpeakerImage, false, leftSpriteCoroutine));
            if (removeRight) rightSpriteCoroutine = StartCoroutine(ToggleCharacterSprite(rightSpeakerImage, false, rightSpriteCoroutine));

            if (placeLeft == true) leftSpriteCoroutine = StartCoroutine(ToggleCharacterSprite(leftSpeakerImage, true, leftSpriteCoroutine, sprite));
            else if (placeLeft == false) rightSpriteCoroutine = StartCoroutine(ToggleCharacterSprite(rightSpeakerImage, true, rightSpriteCoroutine, sprite));
        }

        private void ClearSpeakerVisuals()
        {
            ToggleSpeakerUI(leftNamePlate, leftSpeakerNameText, false);
            ToggleSpeakerUI(rightNamePlate, rightSpeakerNameText, false);
            StartCoroutine(ToggleCharacterSprite(leftSpeakerImage, false));
            StartCoroutine(ToggleCharacterSprite(rightSpeakerImage, false));
        }
        #endregion

        #region Sprite Effects
        private IEnumerator ToggleCharacterSprite(Image image, bool show, Coroutine existingCoroutine = null, Sprite newSprite = null)
        {
            if (existingCoroutine != null)
            {
                StopCoroutine(existingCoroutine);
            }

            if (image.enabled && show)
            {
                yield return SwitchSprites(image, newSprite);
            }
            else if (show)
            {
                image.sprite = newSprite;
                yield return FadeImage(image, true);
            }
            else
            {
                yield return FadeImage(image, false);
            }

            if (image == leftSpeakerImage) leftSpriteCoroutine = null;
            else if (image == rightSpeakerImage) rightSpriteCoroutine = null;
        }

        private IEnumerator SwitchSprites(Image image, Sprite newSprite)
        {
            yield return FadeImage(image, false);
            image.sprite = newSprite;
            yield return FadeImage(image, true);
        }

        private IEnumerator FadeImage(Image image, bool fadeIn)
        {
            image.color = fadeIn ? Color.clear : Color.white;
            image.enabled = true;

            yield return image.DOColor(fadeIn ? Color.white : Color.clear, fadeDuration);

            if (!fadeIn)
            {
                image.enabled = false;
            }
        }

        private void ShakeSprite(Image image, float strength)
        {
            StartCoroutine(ShakeSpriteCoroutine(image, strength));
        }

        private IEnumerator ShakeSpriteCoroutine(Image image, float strength)
        {
            yield return image.rectTransform.DOShakeAnchorPos(0.5f, strength).WaitForCompletion();
        }
        #endregion

        #region Background Effects
        private void ShowBackground()
        {
            backgroundImage.color = Color.clear;
            backgroundImage.enabled = true;
            backgroundImage.DOColor(Color.white, fadeDuration);
        }

        private void HideBackground()
        {
            backgroundImage.DOColor(Color.clear, fadeDuration).OnComplete(() => backgroundImage.enabled = false);
        }

        private void ChangeBackgroundImmediate(string tag)
        {
            string backgroundName = tag.Remove(tag.IndexOf("CHANGEBACKGROUND"), 16).ToLower();
            Sprite background = LoadResource<Sprite>(StaticGameStats.ResourcePaths.Backgrounds + backgroundName);
            
            if (background != null)
            {
                backgroundImage.sprite = background;
                backgroundImage.enabled = true;
            }
        }

        private IEnumerator ChangeBackgroundWithFade(string tag)
        {
            yield return backgroundImage.DOColor(Color.black, fadeDuration).WaitForCompletion();
            
            string backgroundName = tag.Remove(tag.IndexOf("CHANGEBACKGROUND"), 16).ToLower();
            Sprite background = LoadResource<Sprite>(StaticGameStats.ResourcePaths.Backgrounds + backgroundName);
            
            if (background != null)
            {
                backgroundImage.sprite = background;
                yield return backgroundImage.DOColor(Color.white, fadeDuration).WaitForCompletion();
            }
        }
        #endregion

        #region Audio
        private void PlaySoundEffect(string tag)
        {
            string soundName = tag.Remove(tag.IndexOf("PLAYSOUND"), 9).ToLower();
            AudioClip clip = LoadResource<AudioClip>(StaticGameStats.ResourcePaths.Audio.Sounds + soundName);
            
            if (clip != null)
            {
                dialogueAudioSource.clip = clip;
                dialogueAudioSource.Play();
            }
        }
        #endregion

        #region Choice Management
        private void OnNextLineInput(InputAction.CallbackContext context)
        {
            if (canContinueDialogue && !DialogueHasChoices())
            {
                continueIcon.enabled = false;
                StartCoroutine(ProcessNextDialogueLine());
            }
            else if (isTypingSentence)
            {
                skipSentenceTyping = true;
            }
        }

        private bool DialogueHasChoices()
        {
            return currentStory?.currentChoices.Count > 0;
        }

        private void DisplayDialogueChoices()
        {
            if (!DialogueHasChoices())
            {
                continueIcon.enabled = true;
                return;
            }

            var choices = currentStory.currentChoices;
            for (int i = 0; i < choices.Count; i++)
            {
                CreateChoiceButton(choices[i], i);
            }

            SetupChoiceNavigation();
            EventSystemManager.Instance.eventSystem.SetSelectedGameObject(choiceButtons[0].gameObject);
        }

        private void CreateChoiceButton(Choice choice, int index)
        {
            Button button = Instantiate(choicePrefab, choicesContainer);
            button.GetComponentInChildren<TextMeshProUGUI>().text = choice.text;
            button.onClick.AddListener(() => OnChoiceSelected(choice.index));
            choiceButtons.Add(button);
        }

        private void SetupChoiceNavigation()
        {
            for (int i = 0; i < choiceButtons.Count; i++)
            {
                var navigation = new Navigation
                {
                    mode = Navigation.Mode.Explicit,
                    selectOnUp = i == 0 ? choiceButtons[^1] : choiceButtons[i - 1],
                    selectOnDown = i == choiceButtons.Count - 1 ? choiceButtons[0] : choiceButtons[i + 1]
                };
                choiceButtons[i].navigation = navigation;
            }
        }

        private void OnChoiceSelected(int choiceIndex)
        {
            currentStory.ChooseChoiceIndex(choiceIndex);
            continueIcon.enabled = false;
            StartCoroutine(ProcessNextDialogueLine());
        }

        private void RemoveAllChoices()
        {
            foreach (Button button in choiceButtons)
            {
                Destroy(button.gameObject);
            }
            choiceButtons.Clear();
        }
        #endregion

        #region Utility Methods
        private T LoadResource<T>(string path) where T : UnityEngine.Object
        {
            T resource = Resources.Load<T>(path);
            if (resource == null)
            {
                Debug.LogWarning($"Resource not found at path: {path}");
            }
            return resource;
        }
        #endregion
    }
}