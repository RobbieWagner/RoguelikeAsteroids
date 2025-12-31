using UnityEngine;
using RobbieWagnerGames.Utilities;
using System.Collections.Generic;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
	public partial class RunManager : MonoBehaviourSingleton<RunManager>
	{
		private void GenerateLevelTree(int tiers, float difficulty, bool includeShops, bool includeBosses)
        {
            currentRun.levelTree.Clear();
            
            for (int i = 0; i < tiers; i++)
                currentRun.levelTree.Add(new List<LevelNode>());
            
            GenerateRootLevel(difficulty);
            GenerateMiddleTiers(tiers, difficulty, includeShops, includeBosses);
            GenerateBossLevel(tiers, difficulty, includeBosses);

            RunTreeBuilder.ConnectLevelNodes(currentRun.levelTree);
        }

        private void GenerateRootLevel(float baseDifficulty)
        {
            if (currentRun.levelTree.Count == 0) return;
            
            Level rootLevel = CreateLevel(LevelType.ASTEROIDS, 0, baseDifficulty);
            LevelNode rootNode = new LevelNode
            {
                level = rootLevel,
                tier = 0,
                positionInTier = 0
            };
            
            currentRun.levelTree[0].Add(rootNode);
        }

        private void GenerateMiddleTiers(int totalTiers, float baseDifficulty, bool includeShops, bool includeBosses)
        {
            for (int tier = 1; tier < totalTiers - 1; tier++)
            {
                int levelCount = Random.Range(2, 4);
                
                for (int position = 0; position < levelCount; position++)
                {
                    LevelType levelType = LevelType.ASTEROIDS;
                    
                    if (includeShops && ShouldBeShopLevel(tier, position))
                        levelType = LevelType.SHOP;
                    
                    Level level = CreateLevel(levelType, tier, baseDifficulty);
                    LevelNode node = new LevelNode
                    {
                        level = level,
                        tier = tier,
                        positionInTier = position
                    };
                    
                    currentRun.levelTree[tier].Add(node);
                }
            }
        }

        private void GenerateBossLevel(int totalTiers, float baseDifficulty, bool includeBosses)
        {
            if (totalTiers <= 1) return;
            
            int lastTier = totalTiers - 1;
            
            LevelType bossType = includeBosses ? LevelType.BOSS : LevelType.ASTEROIDS;
            Level bossLevel = CreateLevel(bossType, lastTier, baseDifficulty);
            
            LevelNode bossNode = new LevelNode
            {
                level = bossLevel,
                tier = lastTier,
                positionInTier = 0
            };
            
            currentRun.levelTree[lastTier].Add(bossNode);
        }

        private bool ShouldBeShopLevel(int tier, int position)
        {
            int totalPositionInTree = tier * 3 + position;
            return (totalPositionInTree + 1) % 3 == 0;
        }

        private Level CreateLevel(LevelType type, int tier, float baseDifficulty)
        {
            float difficultyMultiplier = CalculateLevelDifficulty(tier, baseDifficulty);
            
            Level level = new Level
            {
                levelType = type,
                difficultyMultiplier = difficultyMultiplier,
                sceneToLoad = GetSceneForLevelType(type),
                tier = tier
            };
            
            ConfigureLevelParameters(level);
            return level;
        }

        private void DebugLogLevelTree()
        {
            if (currentRun == null || currentRun.levelTree == null)
            {
                Debug.Log("No run or level tree to debug.");
                return;
            }
            
            Debug.Log("=== LEVEL TREE STRUCTURE ===");
            Debug.Log($"Total tiers: {currentRun.tiers}");
            Debug.Log($"Starting node: {currentRun.currentNode}");
            
            int totalLevels = 0;
            for (int tier = 0; tier < currentRun.levelTree.Count; tier++)
            {
                totalLevels += currentRun.levelTree[tier].Count;
                Debug.Log($"Tier {tier} ({currentRun.levelTree[tier].Count} nodes):");
                
                foreach (LevelNode node in currentRun.levelTree[tier])
                {
                    string connections = "Connects to: ";
                    if (node.connections.Count == 0)
                        connections += "None (end node)";
                    else
                    {
                        foreach (LevelNode conn in node.connections)
                            connections += $"[T{conn.tier}P{conn.positionInTier}:{conn.level.levelType}] ";
                    }
                    
                    Debug.Log($"  - Node {node.positionInTier}: {node.level.levelType} | Diff: {node.level.difficultyMultiplier:F2} | {connections}");
                }
            }
            
            Debug.Log($"Total levels in tree: {totalLevels}");
            Debug.Log("=== END TREE DEBUG ===");
        }
	}
}