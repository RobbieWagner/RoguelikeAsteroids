using System;
using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using RobbieWagnerGames.Managers;
using RobbieWagnerGames.Utilities;
using UnityEngine;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
    public class RunManager : MonoBehaviourSingleton<RunManager>
    {   
        [SerializeField] private Run defaultRun = new Run();
        private Run currentRun;
        
        public event Action<Run> OnRunStarted;
        public event Action<Run> OnRunEnded;
        public event Action<Run> OnRunFailed;
        public event Action<Level> OnStartLevel;
        
        public event Action OnShowRunMenu;
        public event Action OnHideRunMenu;
        
        public bool IsRunActive => currentRun != null && !currentRun.IsComplete;
        public Run CurrentRun => currentRun;

        protected override void Awake()
        {
            base.Awake();
            
            GameManager.Instance.OnGameStart += OnGameStarted;
        }

        private void OnGameStarted()
        {
            currentRun = null;
            ShowRunMenu();
        }

        public void CreateNewRun(int levelCount, float difficulty, bool includeShops, bool includeBosses)
        {
            currentRun = new Run
            {
                totalLevels = levelCount,
                difficulty = difficulty,
                includeShopLevels = includeShops,
                includeBossLevels = includeBosses,
                startingResources = CalculateStartingResources(),
                startingHealth = 3, //TODO: pull from game save data
                levels = GenerateLevels(levelCount, difficulty, includeShops, includeBosses)
            };
        }

        private List<Level> GenerateLevels(int count, float difficulty, bool includeShops, bool includeBosses)
        {
            List<Level> levels = new List<Level>();
            
            for (int i = 0; i < count; i++)
            {
                LevelType levelType = DetermineLevelType(i, count, includeShops, includeBosses);
                
                Level level = new Level
                {
                    levelType = levelType,
                    difficultyMultiplier = CalculateLevelDifficulty(i, difficulty),
                    sceneToLoad = GetSceneForLevelType(levelType)
                };
                
                ConfigureLevelParameters(level);
                
                levels.Add(level);
            }
            
            return levels;
        }

        private LevelType DetermineLevelType(int index, int total, bool includeShops, bool includeBosses)
        {
            if (includeBosses && index == total - 1)
                return LevelType.BOSS;
            
            if (includeShops && (index + 1) % 3 == 0)
                return LevelType.SHOP;
            
            return LevelType.ASTEROIDS;
        }

        private float CalculateLevelDifficulty(int levelIndex, float baseDifficulty)
        {
            return baseDifficulty * (1 + (levelIndex * 0.1f));
        }

        private SerializedDictionary<ResourceType, int> CalculateStartingResources()
        {
            SerializedDictionary<ResourceType, int> resources = new SerializedDictionary<ResourceType, int>
            {
                // TODO: Set based on game save data (can buy between runs)
                { ResourceType.TITANIUM, 100 },
                { ResourceType.PLATINUM, 0 },
                { ResourceType.IRIDIUM, 0 }
            };

            return resources;
        }

        private string GetSceneForLevelType(LevelType type)
        {
            switch (type)
            {
                case LevelType.SHOP:
                    return "ShopScene";
                case LevelType.BOSS:
                    return "BossScene";
                case LevelType.ASTEROIDS:
                case LevelType.COMBAT:
                case LevelType.SURVIVAL:
                default:
                    return "AsteroidsScene";
            }
        }

        private void ConfigureLevelParameters(Level level)
        {
            //TODO: create resource distribution for level
        }

        public void StartRun()
        {
            if (currentRun == null)
                CreateNewRun(defaultRun.totalLevels, defaultRun.difficulty, defaultRun.includeShopLevels, defaultRun.includeBossLevels);

            OnHideRunMenu?.Invoke();
            OnRunStarted?.Invoke(currentRun);
            
            StartCoroutine(StartCurrentLevelCo());
        }

        public IEnumerator StartCurrentLevelCo()
        {
            if (currentRun.IsComplete)
            {
                Debug.Log($"{currentRun.currentLevelIndex} {currentRun.levels.Count} ");
                yield break;
            }
                
            Level level = currentRun.CurrentLevel;
        
            yield return SceneLoadManager.Instance.UnloadScenes(new () {"AsteroidsScene", "ShopScene", "BossScene"}, false, null, false);
            yield return SceneLoadManager.Instance.LoadSceneAdditive(level.sceneToLoad, true, () => {OnStartLevel?.Invoke(level);});
        }

        public void CompleteCurrentLevel()
        {
            InputManager.Instance.DisableActionMap(ActionMapName.GAME);

            if (currentRun == null || currentRun.IsComplete) return;
            
            Level level = currentRun.CurrentLevel;
            if (level == null) return;
            
            currentRun.currentLevelIndex++;
            
            StartCoroutine(SceneLoadManager.Instance.UnloadScenes(new () {"AsteroidsScene", "ShopScene", "BossScene"}, true, () => { ContinueRun(); } ));           
        }

        public void ContinueRun()
        {
            if (currentRun.IsComplete)
                EndRun(true);
            else
                StartCoroutine(StartCurrentLevelCo()); // TODO: Show a run tree menu
        }

        public void FailRun()
        {
            if (currentRun == null) return;

            StartCoroutine(SceneLoadManager.Instance.UnloadScenes(new () {"AsteroidsScene", "ShopScene", "BossScene"},true,() => {RunManager.Instance.FailRun();},false));
            
            OnRunFailed?.Invoke(currentRun);
            EndRun(false);
        }

        private void EndRun(bool success)
        {
            Run completedRun = currentRun;
            currentRun = null;

            OnRunEnded?.Invoke(completedRun);
        }

        private void ShowRunMenu()
        {
            OnShowRunMenu?.Invoke();
        }

        private Coroutine returnToMenuCo = null;
        public void ReturnToMainMenu()
        {
            if(returnToMenuCo != null) return;
            OnHideRunMenu?.Invoke();
            returnToMenuCo = StartCoroutine(SceneLoadManager.Instance.UnloadScenes(new () {"AsteroidsScene", "ShopScene", "BossScene"}, true, () => {GameManager.Instance.ReturnToMenu();}, false));
        }

        public void RestartGame()
        {
            if(returnToMenuCo != null) return;
            returnToMenuCo = StartCoroutine(SceneLoadManager.Instance.UnloadScenes(new () {"AsteroidsScene", "ShopScene", "BossScene"}, true, () => {GameManager.Instance.RestartGame();}, false));
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            GameManager.Instance.OnGameStart -= OnGameStarted;
        }
    }
}