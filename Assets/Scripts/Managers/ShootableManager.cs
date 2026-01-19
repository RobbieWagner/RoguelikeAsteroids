using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RobbieWagnerGames.Utilities;
using UnityEngine;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
    public class ShootableManager : MonoBehaviourSingleton<ShootableManager>
    {
        [SerializeField] private AsteroidsLevelController levelController;

        [Header("Asteroid Spawning")]
        [SerializeField] private List<Asteroid> asteroidPrefabs;
        [SerializeField] private int maxAsteroids = 10;
        [SerializeField] private float spawnRadius;
        [SerializeField] private float boundsRadius;
        [SerializeField] private float targetRadius;
        [SerializeField] private Vector2 speedRange;
        [SerializeField] private float spawnCooldown = 2f;

        [Header("Resource Pips")]
        [SerializeField] private ResourcePip resourcePipPrefab;

        private List<Shootable> spawnedShootables = new List<Shootable>();
        
        private Coroutine asteroidSpawnCoroutine = null;
        private bool isActive = false;
        [SerializeField] private bool startOnLevelStart = true;
        
        public event Action<Shootable, DestructionReason> ShootableDestroyedEvent;


        protected override void Awake()
        {
            base.Awake();
            
            if(startOnLevelStart)
                levelController.OnLevelStarted += StartAsteroidSpawner;
            levelController.OnLevelFailed += StopAndClearAllShootables;
            levelController.OnLevelCompleted += StopAndClearAllShootables;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            levelController.OnLevelStarted -= StartAsteroidSpawner;
            levelController.OnLevelFailed -= StopAndClearAllShootables;
            levelController.OnLevelCompleted -= StopAndClearAllShootables;
            
            foreach (Shootable shootable in spawnedShootables)
            {
                if (shootable != null)
                    shootable.OnShootableDestroyed -= HandleShootableDestroyed;
            }
        }

        public void ConfigureForLevel(Level level)
        {
            maxAsteroids = (int) (10 * level.difficultyMultiplier * 2);
            spawnCooldown = 1f/level.difficultyMultiplier;
            speedRange = new Vector2(level.difficultyMultiplier, level.difficultyMultiplier + 2);
        }

        public void StartAsteroidSpawner()
        {
            if(!isActive && asteroidSpawnCoroutine == null)
            {
                ConfigureForLevel(levelController.levelDetails);
                isActive = true;
                asteroidSpawnCoroutine = StartCoroutine(RunAsteroidSpawner());
            }
        }

        private void StopAsteroidSpawner()
        {
            isActive = false;
            
            if(asteroidSpawnCoroutine != null)
            {
                StopCoroutine(asteroidSpawnCoroutine);
                asteroidSpawnCoroutine = null;
            }
        }

        private IEnumerator RunAsteroidSpawner()
        {
            while(isActive)
            {
                yield return new WaitForSeconds(spawnCooldown);
                
                int asteroidCount = spawnedShootables.OfType<Asteroid>().Count();
                
                if(asteroidCount < maxAsteroids)
                    SpawnAsteroid();
            }
        }

        private void SpawnAsteroid()
        {
            Vector2 pos = UnityEngine.Random.insideUnitCircle.normalized * spawnRadius;
            Vector2 targetPos = UnityEngine.Random.insideUnitCircle.normalized * targetRadius;
            Vector2 direction = (targetPos - pos).normalized;
            float speed = UnityEngine.Random.Range(speedRange.x, speedRange.y);

            Asteroid prefabToSpawn = asteroidPrefabs[UnityEngine.Random.Range(0, asteroidPrefabs.Count)];
            
            SpawnAsteroid(prefabToSpawn, speed, direction,pos);
        }

        public void SpawnAsteroid(Asteroid asteroidPrefab, float speed, Vector2 direction, Vector2 pos, float radius = -1)
        {
            float bounds = radius > 0 ? radius : boundsRadius;
            
            Asteroid asteroid = Instantiate(asteroidPrefab, transform);
            asteroid.transform.position = pos;
            
            asteroid.Initialize(speed, direction, bounds);
            
            asteroid.OnShootableDestroyed += HandleShootableDestroyed;
            spawnedShootables.Add(asteroid);
        }

        private void HandleShootableDestroyed(Shootable shootable, DestructionReason reason)
        {
            shootable.OnShootableDestroyed -= HandleShootableDestroyed;
            
            if (spawnedShootables.Remove(shootable))
                ShootableDestroyedEvent?.Invoke(shootable, reason);
        }

        public void DestroyAllShootablesOfType<T>() where T : Shootable
        {
            List<T> shootablesOfType = spawnedShootables.OfType<T>().ToList();
            
            foreach(T shootable in shootablesOfType)
            {
                if(shootable != null)
                {
                    shootable.OnShootableDestroyed -= HandleShootableDestroyed;
                    shootable.doDestructionEffects = false;
                    Destroy(shootable.gameObject);
                    spawnedShootables.Remove(shootable);
                }
            }
        }

        private void StopAndClearAllShootables()
        {
            StopAsteroidSpawner();
            ClearAllShootables();
        }

        private void ClearAllShootables()
        {
            foreach(Shootable shootable in spawnedShootables)
            {
                if(shootable != null)
                {
                    shootable.OnShootableDestroyed -= HandleShootableDestroyed;
                    shootable.doDestructionEffects = false;
                    Destroy(shootable.gameObject);
                }
            }
            spawnedShootables.Clear();
        }

        public int GetAsteroidCount() => spawnedShootables.OfType<Asteroid>().Count();
        public int GetShipCount() => spawnedShootables.OfType<EnemyShip>().Count();
        public bool HasActiveShootables() => spawnedShootables.Any();
    }
}