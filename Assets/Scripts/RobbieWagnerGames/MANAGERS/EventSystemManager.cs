using UnityEngine;
using UnityEngine.EventSystems;
using RobbieWagnerGames.Utilities;

namespace RobbieWagnerGames.Managers
{
    /// <summary>
    /// Singleton manager for handling Unity's EventSystem
    /// </summary>
    public class EventSystemManager : MonoBehaviourSingleton<EventSystemManager>
    {
        [Header("Event System Configuration")]
        public EventSystem eventSystem;
        [SerializeField] private bool autoInitialize = true;

        /// <summary>
        /// Currently selected game object in the event system
        /// </summary>
        public GameObject CurrentSelected => eventSystem?.currentSelectedGameObject;

        protected override void Awake()
        {
            base.Awake();
            
            if (autoInitialize && eventSystem == null)
            {
                eventSystem = FindFirstObjectByType<EventSystem>();
                if (eventSystem == null)
                {
                    Debug.LogWarning("No EventSystem found in scene", this);
                }
            }
        }

        /// <summary>
        /// Set the currently selected UI game object
        /// </summary>
        public void SetSelected(GameObject gameObject)
        {
            if (eventSystem == null)
            {
                Debug.LogWarning("Cannot select object - EventSystem reference is null", this);
                return;
            }

            if (gameObject == null)
            {
                Debug.LogWarning("Cannot select null game object", this);
                return;
            }

            eventSystem.SetSelectedGameObject(gameObject);
        }

        /// <summary>
        /// Clear the current selection
        /// </summary>
        public void ClearSelection()
        {
            if (eventSystem != null)
            {
                eventSystem.SetSelectedGameObject(null);
            }
        }

        /// <summary>
        /// Check if a game object is currently selected
        /// </summary>
        public bool HasSelection()
        {
            return eventSystem != null && eventSystem.currentSelectedGameObject != null;
        }
    }
}