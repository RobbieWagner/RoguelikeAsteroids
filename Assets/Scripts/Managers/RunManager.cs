using System;
using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using RobbieWagnerGames.Managers;
using RobbieWagnerGames.Utilities;
using RobbieWagnerGames.Utilities.SaveData;
using UnityEngine;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
    public partial class RunManager : MonoBehaviourSingleton<RunManager>
    {   
        [SerializeField] private Run defaultRun = new Run();
        private Run currentRun;
        
        public event Action<Run> OnRunStarted;
        public event Action<Run> OnNewRunStarted;
        public event Action<Run> OnRunContinued;
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
            GameManager.Instance.OnGameSaved += SaveRunData;
        }

        private void OnGameStarted()
        {
            currentRun = null;
            ShowRunMenu();
        }

        public void CreateNewRun(int tiers, float difficulty, bool includeShops, bool includeBosses)
        {
            currentRun = new Run
            {
                tiers = tiers,
                difficulty = difficulty,
                includeShopLevels = includeShops,
                includeBossLevels = includeBosses,
                runResources = CalculateStartingResources(),
                startingHealth = 3,
                health = 3
            };

            GenerateLevelTree(tiers, difficulty, includeShops, includeBosses);
            ResourceManager.Instance.InitializeResourceDictionary(currentRun.runResources);

            OnNewRunStarted?.Invoke(currentRun);
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
            currentRun = JsonDataService.Instance.LoadDataRelative<Run>(GameConstants.RunPath, null);

            if (currentRun == null)
                CreateNewRun(defaultRun.tiers, defaultRun.difficulty, defaultRun.includeShopLevels, defaultRun.includeBossLevels);
            else
                LoadRun();

            // LogLevelTree();

            OnHideRunMenu?.Invoke();
            OnRunStarted?.Invoke(currentRun);
            
            ContinueRun();
        }

        private void LoadRun()
        {
            currentRun.DeserializeNodeTree();
            ResourceManager.Instance.InitializeResourceDictionary(currentRun.runResources);
        }

        public IEnumerator StartCurrentLevelCo()
        {
            if (currentRun.IsComplete)
            {
                Debug.Log($"{currentRun.currentTier} {currentRun.tiers} ");
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

            StartCoroutine(SceneLoadManager.Instance.UnloadScenes(new () {"AsteroidsScene", "ShopScene", "BossScene"}, true, () => { ContinueRun(); } ));           
        }

        public void ContinueRun()
        {
            if (currentRun.IsComplete)
            {    
                EndRun(true);
                return;
            }

            if (currentRun.currentTier > -1) 
                GameManager.Instance.SaveGame();
            OnRunContinued?.Invoke(CurrentRun);
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

        private void SaveRunData()
        {
            CurrentRun.PrepForSerialization();
            currentRun.runResources.Clear();
            foreach(KeyValuePair<ResourceType, int> resource in ResourceManager.Instance.gatheredResources)
                CurrentRun.runResources.Add(resource.Key, resource.Value);

            if(!JsonDataService.Instance.SaveData(GameConstants.RunPath, CurrentRun))
                throw new InvalidOperationException("Could not save run data");
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            GameManager.Instance.OnGameStart -= OnGameStarted;
            GameManager.Instance.OnGameSaved -= SaveRunData;
        }
    }
}