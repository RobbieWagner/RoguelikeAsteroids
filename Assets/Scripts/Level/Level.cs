using AYellowpaper.SerializedCollections;
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
        // Level identity
        public int levelSeed;
        public LevelType levelType;
        
        // Configuration
        public float difficultyMultiplier = 1f;
        public string sceneToLoad = "AsteroidsScene";
        
        // Level parameters
        public int asteroidCount = 10;
        public float asteroidSpawnRate = 2f;
        [JsonIgnore] public Vector2 asteroidSpeedRange = new Vector2(1f, 3f);
        public float levelDuration = 20f; // Set to -1 if not a requirement
        public bool stopAtTimer => levelDuration > 0;
        public int requiredResources = 50; // Set to -1 if not a requirement
        public int tier { get; set;}
        
        // Resource distribution
        [JsonProperty] public SerializedDictionary<ResourceType, int> resourceDistribution = new SerializedDictionary<ResourceType, int>();
    }
}