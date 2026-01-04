using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using Newtonsoft.Json;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
    [Serializable]
    public class Run
    {
        public int runSeed;
        
        // Configuration
        [Range(2,15)] public int tiers = 5;
        public float difficulty = 1f;
        public float shopRatio = .1f;
        public bool includeBossLevels = true;
        public int startingHealth = 3;
        private int _health;
        public int health 
        { 
            get 
            {
                return _health;
            } 
            set
            {
                Debug.Log($"{value} {_health}");
                if (_health == value)
                    return;
                _health = value;
                Debug.Log("hi");
                OnUpdateHealth?.Invoke(_health);
            } 
        }
        public event Action<int> OnUpdateHealth;

        [JsonProperty] [SerializedDictionary] public SerializedDictionary<ResourceType, int> runResources = new SerializedDictionary<ResourceType, int>();
        
        public LevelNode currentNode { get; set; }
        [JsonIgnore] public int currentTier => currentNode != null ? currentNode.tier : -1;
        public List<List<LevelNode>> levelTree = new List<List<LevelNode>>();
        
        [JsonIgnore] private Dictionary<string, LevelNode> _nodeLookup = new Dictionary<string, LevelNode>();
        [JsonIgnore] public bool IsComplete => currentNode == null ? false : (levelTree.Count > 0 && 
            currentNode.tier == levelTree.Count - 1 && currentNode.connections.Count == 0);
        [JsonIgnore] public Level CurrentLevel => currentNode?.level;

        public void PrepForSerialization()
        {
            foreach (List<LevelNode> tierList in levelTree)
            {
                foreach (LevelNode node in tierList)
                    node.PrepForSerialization();
            }
            
            RebuildNodeLookup();
        }
        
        public void DeserializeNodeTree()
        {
            RebuildNodeLookup();
            
            foreach (List<LevelNode> tierList in levelTree)
            {
                foreach (LevelNode node in tierList)
                    node.DeserializeConnections(_nodeLookup);
            }

            if (currentNode != null)
                currentNode.DeserializeConnections(_nodeLookup);
        }
        
        private void RebuildNodeLookup()
        {
            _nodeLookup.Clear();
            foreach (List<LevelNode> tierList in levelTree)
            {
                foreach (LevelNode node in tierList)
                {
                    if (!string.IsNullOrEmpty(node.nodeID))
                        _nodeLookup[node.nodeID] = node;
                }
            }
        }
        
        public LevelNode GetNodeById(string nodeId)
        {
            _nodeLookup.TryGetValue(nodeId, out LevelNode node);
            return node;
        }
        
        public void AddNodeToTree(LevelNode node, int tier)
        {
            while (levelTree.Count <= tier)
                levelTree.Add(new List<LevelNode>());
            
            node.tier = tier;
            node.positionInTier = levelTree[tier].Count;
            levelTree[tier].Add(node);
            
            if (!string.IsNullOrEmpty(node.nodeID))
                _nodeLookup[node.nodeID] = node;
        }
    }
}