using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
    public enum LevelType
    {
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
        public Vector2 asteroidSpeedRange = new Vector2(1f, 3f);
        public float levelDuration = 60f; // Set to -1 if not a requirement
        public int requiredScore = 1000; // Set to -1 if not a requirement
        public int requiredResources = 50; // Set to -1 if not a requirement
        
        // Resource distribution
        public SerializedDictionary<ResourceType, int> resourceDistribution = new SerializedDictionary<ResourceType, int>();
    }
}