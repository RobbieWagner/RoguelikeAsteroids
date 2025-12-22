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

        private List<Shootable> spawnedShootables = new List<Shootable>();
        
        private Coroutine asteroidSpawnCoroutine = null;
        private bool isActive = false;
        
        public event Action<Shootable, DestructionReason> ShootableDestroyedEvent;

        protected override void Awake()
        {
            base.Awake();
            
            GameManager.Instance.OnGameStart += StartAsteroidSpawner;
            GameManager.Instance.OnGameOver += StopAndClearAllShootables;
            GameManager.Instance.OnReturnToMenu += StopAndClearAllShootables;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            GameManager.Instance.OnGameStart -= StartAsteroidSpawner;
            GameManager.Instance.OnGameOver -= StopAndClearAllShootables;
            GameManager.Instance.OnReturnToMenu -= StopAndClearAllShootables;
            
            foreach (var shootable in spawnedShootables)
            {
                if (shootable != null)
                    shootable.OnShootableDestroyed -= HandleShootableDestroyed;
            }
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
            Asteroid asteroid = Instantiate(asteroidPrefab);
            
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

        private void HandleShootableDestroyed(Shootable shootable, DestructionReason reason)
        {
            shootable.OnShootableDestroyed -= HandleShootableDestroyed;
            
            if (spawnedShootables.Remove(shootable))
                ShootableDestroyedEvent?.Invoke(shootable, reason);
        }

        public void DestroyAllShootablesOfType<T>() where T : Shootable
        {
            var shootablesOfType = spawnedShootables.OfType<T>().ToList();
            
            foreach(var shootable in shootablesOfType)
            {
                if(shootable != null)
                {
                    shootable.OnShootableDestroyed -= HandleShootableDestroyed;
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