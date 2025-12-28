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
        [Header("Asteroid Spawning")]
        [SerializeField] private Asteroid asteroidPrefab;
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
        
        public event Action<Shootable, DestructionReason> ShootableDestroyedEvent;

        protected override void Awake()
        {
            base.Awake();
            
            LevelManager.Instance.OnLevelStarted += ConfigureForLevel;
            LevelManager.Instance.OnLevelFailed += StopAndClearAllShootables;
            LevelManager.Instance.OnLevelCompleted += StopAndClearAllShootables;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            LevelManager.Instance.OnLevelStarted -= ConfigureForLevel;
            LevelManager.Instance.OnLevelFailed -= StopAndClearAllShootables;
            LevelManager.Instance.OnLevelCompleted -= StopAndClearAllShootables;
            
            foreach (var shootable in spawnedShootables)
            {
                if (shootable != null)
                    shootable.OnShootableDestroyed -= HandleShootableDestroyed;
            }
        }

        public void ConfigureForLevel(Level level)
        {
            maxAsteroids = level.asteroidCount;
            spawnCooldown = level.asteroidSpawnRate;
            speedRange = level.asteroidSpeedRange;
            
            //StopAndClearAllShootables();
            StartAsteroidSpawner();
        }

        public void StartAsteroidSpawner()
        {
            if(!isActive && asteroidSpawnCoroutine == null)
            {
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
            Asteroid asteroid = Instantiate(asteroidPrefab, transform);
            
            Vector2 pos = UnityEngine.Random.insideUnitCircle.normalized * spawnRadius;
            asteroid.transform.position = pos;
            Vector2 targetPos = UnityEngine.Random.insideUnitCircle.normalized * targetRadius;
            Vector2 direction = (targetPos - pos).normalized;

            asteroid.Initialize(
                UnityEngine.Random.Range(speedRange.x, speedRange.y), 
                direction, 
                boundsRadius
            );
            
            asteroid.OnShootableDestroyed += HandleShootableDestroyed;
            spawnedShootables.Add(asteroid);
        }

        public void SpawnResourcePips(Transform parent, ResourceGatherData resourceData, int pipCount = 5)
        {
            if (resourcePipPrefab == null || resourceData == null || resourceData.resources == null) return;
            
            foreach (KeyValuePair<ResourceType, int> resource in resourceData.resources)
            {
                if (resource.Key == ResourceType.NONE || resource.Value <= 0) continue;
                
                int remainingAmount = resource.Value;

                while (remainingAmount >= 10)
                {
                    SpawnPip(parent, resource.Key, 10);
                    remainingAmount -= 10;
                }
                
                while (remainingAmount >= 5)
                {
                    SpawnPip(parent, resource.Key, 5);
                    remainingAmount -= 5;
                }
                
                while (remainingAmount > 0)
                {
                    SpawnPip(parent, resource.Key, 1);
                    remainingAmount -= 1;
                }
            }
        }

        private void SpawnPip(Transform parent, ResourceType resourceType, int amount)
        {
            ResourcePip pip = Instantiate(resourcePipPrefab, parent.position, Quaternion.identity, transform);
            
            if (pip != null)
            {
                pip.Initialize(resourceType, amount);
                pip.AddRandomForce(UnityEngine.Random.Range(1f, 3f));
            }
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
                    Destroy(shootable.gameObject);
                    spawnedShootables.Remove(shootable);
                }
            }
        }

        private void StopAndClearAllShootables(Level level = null)
        {
            StopAsteroidSpawner();
            ClearAllShootables();
        }

        private void ClearAllShootables()
        {
            foreach(var shootable in spawnedShootables)
            {
                if(shootable != null)
                {
                    shootable.OnShootableDestroyed -= HandleShootableDestroyed;
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