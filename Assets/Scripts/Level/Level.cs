using UnityEngine;
using Newtonsoft.Json;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
    public enum LevelType
    {
        NONE = -1,
        ASTEROIDS,
        SHOP,
        BOSS,
        COMBAT,
        SURVIVAL
    }
    
    [System.Serializable]
    public class Level
    {
        #region general info 
        public int levelSeed;
        public LevelType levelType;
        public string sceneToLoad = "AsteroidsScene";
        public float difficultyMultiplier = 1f;
        public int tier { get; set;}
        #endregion
        
        #region level completion
        public float levelDuration = 20f; // Set to -1 if not a requirement
        public bool stopAtTimer => levelDuration > 0;
        public int requiredResources = 50; // Set to -1 if not a requirement
        public bool requiresResources => requiredResources > 0;
        #endregion
    }
}