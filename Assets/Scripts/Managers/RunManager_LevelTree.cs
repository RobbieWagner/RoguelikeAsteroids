using UnityEngine;
using RobbieWagnerGames.Utilities;
using System.Collections.Generic;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
	public partial class RunManager : MonoBehaviourSingleton<RunManager>
	{
		private void GenerateLevelTree(int tiers, float difficulty, float shopRatio, bool includeBosses)
        {
            currentRun.levelTree.Clear();
            
            for (int i = 0; i < tiers; i++)
                currentRun.levelTree.Add(new List<LevelNode>());
            
            GenerateRootLevel(difficulty);
            GenerateMiddleTiers(tiers, difficulty, shopRatio, includeBosses);
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

        private void GenerateMiddleTiers(int totalTiers, float baseDifficulty, float shopRatio = .1f, bool includeBosses = true)
        {
            List<int> levelCountsPerTier = new List<int>();
            int totalLevels = 0;
            
            for (int tier = 1; tier < totalTiers - 1; tier++)
            {
                int levelCount = Random.Range(2, 5);
                levelCountsPerTier.Add(levelCount);
                totalLevels += levelCount;
            }
            
            int shopCount = Mathf.RoundToInt(totalLevels * shopRatio);
            
            List<(int tier, int position)> allPositions = new List<(int tier, int position)>();
            int currentTier = 1;
            
            foreach (int levelCount in levelCountsPerTier)
            {
                for (int position = 0; position < levelCount; position++)
                {
                    allPositions.Add((currentTier, position));
                }
                currentTier++;
            }
            
            List<int> shopIndices = new List<int>();
           
            for (int i = 0; i < shopCount; i++)
            {
                int randomIndex;
                do
                {
                    randomIndex = Random.Range(0, allPositions.Count);
                } while (shopIndices.Contains(randomIndex));
                
                shopIndices.Add(randomIndex);
            }
            
            int positionIndex = 0;
            for (int tierIndex = 0; tierIndex < levelCountsPerTier.Count; tierIndex++)
            {
                int tier = tierIndex + 1;
                int levelCount = levelCountsPerTier[tierIndex];
                
                for (int position = 0; position < levelCount; position++)
                {
                    bool isShop = shopIndices.Contains(positionIndex);
                    LevelType levelType = isShop ? LevelType.SHOP : LevelType.ASTEROIDS;
                    
                    Level level = CreateLevel(levelType, tier, baseDifficulty);
                    LevelNode node = new LevelNode
                    {
                        level = level,
                        tier = tier,
                        positionInTier = position
                    };
                    
                    currentRun.levelTree[tier].Add(node);
                    positionIndex++;
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
	}
}