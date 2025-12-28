using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
    [System.Serializable]
    public class Run
    {
        public int runSeed;
        
        // Configuration
        public int totalLevels = 5;
        public float difficulty = 1f;
        public bool includeShopLevels = true;
        public bool includeBossLevels = true;
        public SerializedDictionary<ResourceType, int> startingResources = new SerializedDictionary<ResourceType, int>();
        public int startingHealth = 3;
        
        // Progression
        public int currentLevelIndex {get; set;}
        public List<Level> levels = new List<Level>();
        
        // Helper properties
        public bool IsComplete => currentLevelIndex >= levels.Count;
        public Level CurrentLevel => !IsComplete && currentLevelIndex < levels.Count ? 
            levels[currentLevelIndex] : null;
    }
}