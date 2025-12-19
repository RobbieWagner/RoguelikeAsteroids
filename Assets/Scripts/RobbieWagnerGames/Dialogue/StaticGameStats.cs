using UnityEngine;

namespace RobbieWagnerGames
{
    /// <summary>
    /// Centralized storage for global game paths and constants
    /// </summary>
    public static class StaticGameStats
    {
        #region Resource Paths
        public static class ResourcePaths
        {
            public const string CombatActions = "Unit/";
            public const string Sprites = "Sprites/";
            
            public static class Characters
            {
                public const string Base = "Sprites/Characters/";
                public const string Survivors = "Sprites/Characters/Survivors/";
            }
            
            public const string Backgrounds = "Sprites/Backgrounds/";
            public const string Heads = "Sprites/Heads/";
            public const string MapButtons = "Sprites/UI/Map/";
            
            public static class Animation
            {
                public const string Combat = "Animation/CombatAnimation/";
                public const string DefaultCombat = "Animation/CombatAnimation/Player/Player";
            }
            
            public static class Audio
            {
                public const string Sounds = "Sounds/";
                
                public static class Dialogue
                {
                    public const string Music = "Sounds/Dialogue/Music/";
                    public const string SoundEffects = "Sounds/Dialogue/SoundEffects/";
                }
                
                public static class Combat
                {
                    public const string Music = "Sounds/Combat/Music/";
                    public const string SoundEffects = "Sounds/Combat/SoundEffects/";
                }
            }
            
            public const string Scenes = "Assets/Scenes/Combat/";
            public const string DialogueSaves = "Exploration/DialogueInteractions/";
        }
        #endregion

        #region Save Data
        public static class SaveData
        {
            private static string _persistentDataPath;
            public static string PersistentDataPath
            {
                get
                {
                    if (string.IsNullOrWhiteSpace(_persistentDataPath))
                        _persistentDataPath = Application.persistentDataPath;
                    return _persistentDataPath;
                }
            }

            public const string Party = "player_party.json";
            public const string Inventory = "player_inventory.json";
            public const string Exploration = "exploration_data.json";
            public const string GameData = "game_data.json";
            public const string MapConfiguration = "map_configuration.json";
        }
        #endregion

        #region Deprecated Members (for compatibility)
        [System.Obsolete("Use ResourcePaths.CombatActions instead")]
        public static string combatActionFilePath = ResourcePaths.CombatActions;
        
        [System.Obsolete("Use ResourcePaths.Sprites instead")]
        public static string spritesFilePath = ResourcePaths.Sprites;

        public const string playerSaveDataFilePath = "playerSaveData.json";
        
        // ... (other obsolete declarations for compatibility)
        #endregion
    }
}