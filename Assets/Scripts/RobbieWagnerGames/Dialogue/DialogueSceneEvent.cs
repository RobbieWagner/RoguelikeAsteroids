using System.Collections;
using UnityEngine;
using Ink.Runtime;

namespace RobbieWagnerGames.Dialogue
{
    /// <summary>
    /// A scene event that plays a dialogue sequence
    /// </summary>
    public class DialogueSceneEvent : SceneEvent
    {
        [Header("Dialogue Settings")]
        [SerializeField] private TextAsset inkStoryAsset;
        //[SerializeField] private bool useSimpleDialogueManager = false;
        [SerializeField] private bool waitForCompletion = true;

        public override IEnumerator RunSceneEvent()
        {
            if (inkStoryAsset == null)
            {
                Debug.LogWarning("No Ink story asset assigned", this);
                yield break;
            }

            Story story = DialogueConfigurer.CreateStory(inkStoryAsset);
            
            if (story == null)
            {
                yield break;
            }

            if (DialogueManager.Instance != null)
            {
                yield return StartCoroutine(DialogueManager.Instance.StartDialogueCo(story));
            }
            else
            {
                Debug.LogError("No available dialogue manager found");
            }

            if (waitForCompletion)
            {
                yield return StartCoroutine(base.RunSceneEvent());
            }
            else
            {
                StartCoroutine(base.RunSceneEvent());
            }
        }
    }
}