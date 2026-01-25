using UnityEngine;
using RobbieWagnerGames.Utilities;
using System.Collections.Generic;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
	public partial class RunManager : MonoBehaviourSingleton<RunManager>
	{
		private void GenerateLevelTree(Run run)
        {
            run.levelTree.Clear();
            
            for (int i = 0; i < run.tiers; i++)
                run.levelTree.Add(new List<LevelNode>());
            
            GenerateRootLevel(run);
            GenerateMiddleTiers(run);
            GenerateBossLevel(run);

            RunTreeBuilder.ConnectLevelNodes(run.levelTree);
        }

        private void GenerateRootLevel(Run run)
        {
            if (run.levelTree.Count == 0) return;
            
            Level rootLevel = CreateLevel(LevelType.ASTEROIDS, 0, run.difficulty, 20f);
            LevelNode rootNode = new LevelNode
            {
                level = rootLevel,
                tier = 0,
                positionInTier = 0
            };
            
            run.levelTree[0].Add(rootNode);
        }

        private void GenerateMiddleTiers(Run run)
        {
            List<int> levelCountsPerTier = new List<int>();
            int totalLevels = 0;
            
            for (int tier = 1; tier < run.tiers - 1; tier++)
            {
                int levelCount = Random.Range(2, 5);
                levelCountsPerTier.Add(levelCount);
                totalLevels += levelCount;
            }
            
            int shopCount = Mathf.RoundToInt(totalLevels * run.shopRatio);
            
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
                    
                    Level level = CreateLevel(levelType, tier, run.difficulty, 20 + tier * 1.5f);
                    LevelNode node = new LevelNode
                    {
                        level = level,
                        tier = tier,
                        positionInTier = position
                    };
                    
                    run.levelTree[tier].Add(node);
                    positionIndex++;
                }
            }
        }

        private void GenerateBossLevel(Run run)
        {
            if (run.tiers <= 1) return;
            
            int lastTier = run.tiers  - 1;
            
            LevelType bossType = run.includeBossLevels ? LevelType.BOSS : LevelType.ASTEROIDS;
            Level bossLevel = CreateLevel(bossType, lastTier, run.difficulty, -1);
            
            LevelNode bossNode = new LevelNode
            {
                level = bossLevel,
                tier = lastTier,
                positionInTier = 0
            };
            
            run.levelTree[lastTier].Add(bossNode);
        }

        private Level CreateLevel(LevelType type, int tier, float baseDifficulty, float levelDuration)
        {
            float difficultyMultiplier = CalculateLevelDifficulty(tier, baseDifficulty);
            
            Level level = new Level
            {
                levelType = type,
                difficultyMultiplier = difficultyMultiplier,
                sceneToLoad = GetSceneForLevelType(type),
                tier = tier,
                levelDuration = levelDuration
            };
            
            ConfigureLevelParameters(level);
            return level;
        }
	}
}