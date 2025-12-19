using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RobbieWagnerGames.Utilities;

namespace RobbieWagnerGames.Dialogue
{
    /// <summary>
    /// Manages a sequence of dialogue events in a scene
    /// </summary>
    public class DialogueScene : MonoBehaviourSingleton<DialogueScene>
    {
        [Header("Scene Configuration")]
        [SerializeField] private bool playOnAwake = true;
        [SerializeField] private Transform sceneEventsParent;

        private readonly List<SceneEvent> sceneEvents = new List<SceneEvent>();

        protected override void Awake()
        {
            base.Awake();
            GatherSceneEvents();

            if (playOnAwake)
            {
                StartScene();
            }
        }

        /// <summary>
        /// Collect all scene events from parent or manual list
        /// </summary>
        private void GatherSceneEvents()
        {
            sceneEvents.Clear();

            if (sceneEventsParent != null && sceneEventsParent.childCount > 0)
            {
                sceneEvents.AddRange(sceneEventsParent.GetComponentsInChildren<SceneEvent>());
            }
        }

        /// <summary>
        /// Begin playing the dialogue scene
        /// </summary>
        public void StartScene()
        {
            StartCoroutine(PlaySceneCoroutine());
        }

        private IEnumerator PlaySceneCoroutine()
        {
            if (sceneEvents.Count == 0)
            {
                Debug.LogWarning("No scene events found to play");
                yield break;
            }

            foreach (SceneEvent sceneEvent in sceneEvents)
            {
                if (sceneEvent != null)
                {
                    yield return StartCoroutine(sceneEvent.RunSceneEvent());
                }
            }
        }

        /// <summary>
        /// Add a scene event to be played
        /// </summary>
        public void AddSceneEvent(SceneEvent newEvent)
        {
            if (newEvent != null && !sceneEvents.Contains(newEvent))
            {
                sceneEvents.Add(newEvent);
            }
        }

        /// <summary>
        /// Remove a scene event from playback
        /// </summary>
        public void RemoveSceneEvent(SceneEvent eventToRemove)
        {
            if (eventToRemove != null)
            {
                sceneEvents.Remove(eventToRemove);
            }
        }
    }
}