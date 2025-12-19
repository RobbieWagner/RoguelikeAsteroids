using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using RobbieWagnerGames.Utilities;
using System.Linq;

namespace RobbieWagnerGames.Managers
{
    /// <summary>
    /// Action maps available in the game
    /// </summary>
    public enum ActionMapName
    {
        EXPLORATION,
        BLOCK_BREAK,
        GAME,
        DIALOGUE,
        UI,
        PAUSE
    }

    /// <summary>
    /// Singleton manager for handling all game input
    /// </summary>
    public class InputManager : MonoBehaviourSingleton<InputManager>
    {
        [Header("Input Configuration")]
        [SerializeField] private bool disableAllOnStart = true;
        
        private GameControls gameControls;
        public readonly Dictionary<ActionMapName, InputActionMap> actionMaps = new Dictionary<ActionMapName, InputActionMap>();
        [SerializeField] private List<ActionMapName> currentActiveMaps = new List<ActionMapName>();
        private List<ActionMapName> reservedMaps = new List<ActionMapName>(); // For when maps need to be disabled temporarily

        public GameControls Controls => gameControls;
        public List<ActionMapName> CurrentActiveMaps => currentActiveMaps;

        public delegate void OnActionMapsUpdated(List<ActionMapName> activeMaps);
        public event OnActionMapsUpdated onActionMapsUpdated;

        [SerializeField] private List<ActionMapName> defaultActionMaps = new List<ActionMapName>();

        protected override void Awake()
        {
            base.Awake();
            InitializeInputSystem();

            if(defaultActionMaps.Any())
            {
                foreach(var map in defaultActionMaps)
                {
                    EnableActionMap(map, false); // Don't disable others, just enable each
                }
            }
        }

        private void InitializeInputSystem()
        {
            gameControls = new GameControls();
            
            // Map enum values to action maps
            foreach (ActionMapName mapName in Enum.GetValues(typeof(ActionMapName)))
            {
                string mapNameString = mapName.ToString();
                if (gameControls.asset.FindActionMap(mapNameString) != null)
                {
                    actionMaps[mapName] = gameControls.asset.FindActionMap(mapNameString);
                }
                else
                {
                    Debug.LogWarning($"Action map {mapName} not found in controls asset", this);
                }
            }

            if (disableAllOnStart)
            {
                DisableAllActionMaps();
            }
            else
            {
                gameControls.Enable();
            }
        }

        /// <summary>
        /// Enable a specific action map and optionally disable others
        /// </summary>
        public void EnableActionMap(ActionMapName mapName, bool disableOthers = true)
        {
            if (disableOthers)
            {
                DisableAllActionMaps();
            }

            if (actionMaps.TryGetValue(mapName, out var actionMap) && !currentActiveMaps.Contains(mapName))
            {
                actionMap.Enable();
                currentActiveMaps.Add(mapName);
                // If enabling EXPLORATION, lock and center the cursor
                if (mapName == ActionMapName.EXPLORATION)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    // Move cursor to center (works in editor, not always in build)
                    Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                }
            }
            else
            {
                Debug.LogWarning($"Could not enable action map {mapName}: not found", this);
            }

            onActionMapsUpdated?.Invoke(currentActiveMaps);
        }

        /// <summary>
        /// Disable a specific action map
        /// </summary>
        public void DisableActionMap(ActionMapName mapName)
        {
            if (actionMaps.TryGetValue(mapName, out var actionMap))
            {
                actionMap.Disable();
                if (currentActiveMaps.Contains(mapName))
                {
                    currentActiveMaps.Remove(mapName);
                }
                // If disabling EXPLORATION, unlock and show the cursor
                if (mapName == ActionMapName.EXPLORATION)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
            }
            else
            {
                Debug.LogWarning($"Could not disable action map {mapName}: not found", this);
            }

            onActionMapsUpdated?.Invoke(currentActiveMaps);
        }

        /// <summary>
        /// Disable all action maps
        /// </summary>
        public void DisableAllActionMaps()
        {
            foreach (var mapPair in actionMaps)
            {
                mapPair.Value.Disable();
                if (currentActiveMaps.Contains(mapPair.Key))
                {
                    currentActiveMaps.Remove(mapPair.Key);
                }
                // If disabling EXPLORATION, unlock and show the cursor
                if (mapPair.Key == ActionMapName.EXPLORATION)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
            }

            onActionMapsUpdated?.Invoke(currentActiveMaps);
        }
        
        /// <summary>
        /// Saves the currently active action maps into reservedMaps and disables all action maps.
        /// </summary>
        public void SaveAndDisableCurrentActionMaps()
        {
            reservedMaps.Clear();
            reservedMaps.AddRange(currentActiveMaps);
            DisableAllActionMaps();
        }

        /// <summary>
        /// Restores the action maps saved in reservedMaps and clears the reservedMaps list.
        /// </summary>
        public void RestoreReservedActionMaps()
        {
            foreach (var map in reservedMaps)
            {
                EnableActionMap(map, false); // Don't disable others, just enable each
            }
            reservedMaps.Clear();
        }

        /// <summary>
        /// Get a specific action map
        /// </summary>
        public InputActionMap GetActionMap(ActionMapName mapName)
        {
            return actionMaps.TryGetValue(mapName, out var actionMap) ? actionMap : null;
        }

        /// <summary>
        /// Get a specific input action by name
        /// </summary>
        public InputAction GetAction(ActionMapName mapName, string actionName)
        {
            var actionMap = GetActionMap(mapName);
            return actionMap?.FindAction(actionName);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            DisableAllActionMaps();
            gameControls?.Dispose();
        }
    }
}