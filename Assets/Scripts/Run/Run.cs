using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
    [Serializable]
    public class Run
    {
        public int runSeed;
        
        // Configuration
        [Range(2,15)] public int tiers = 5;
        public float difficulty = 1f;
        public bool includeShopLevels = true;
        public bool includeBossLevels = true;
        public SerializedDictionary<ResourceType, int> startingResources = new SerializedDictionary<ResourceType, int>();
        public int startingHealth = 3;
        
        public int currentTier { get; set; }
        public LevelNode currentNode { get; set; }
        public List<List<LevelNode>> levelTree = new List<List<LevelNode>>();
        
        public bool IsComplete => currentNode == null || (levelTree.Count > 0 && 
            currentNode.tier == levelTree.Count - 1 && currentNode.connections.Count == 0);
        public Level CurrentLevel => currentNode?.level;
    }
}