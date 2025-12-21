using System;
using System.Collections;
using System.Collections.Generic;
using RobbieWagnerGames.Utilities;
using UnityEngine;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
    public class AsteroidManager : MonoBehaviourSingleton<AsteroidManager>
    {
        [Header("Spawning")]
        private List<Shootable> spawnedAsteroids = new List<Shootable>();
        public int maxAsteroids = 10;
        [SerializeField] private Shootable asteroidPrefab;
        [SerializeField] private float spawnRadius;
        [SerializeField] private float boundsRadius;

        [Header("Movement")]
        [SerializeField] private float targetRadius;
        [SerializeField] private Vector2 speedRange;
        [SerializeField] private float spawnCooldown = 2f;

        private Coroutine asteroidSpawnCoroutine = null;
        private bool isActive = false;
        public event Action<Shootable, DestructionReason> AsteroidDestroyedEvent;

        protected override void Awake()
        {
            base.Awake();
            
            GameManager.Instance.OnGameStart += StartAsteroidSpawner;
            GameManager.Instance.OnGameOver += StopAndClearAsteroids;
            GameManager.Instance.OnReturnToMenu += StopAndClearAsteroids;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            GameManager.Instance.OnGameStart -= StartAsteroidSpawner;
            GameManager.Instance.OnGameOver -= StopAndClearAsteroids;
            GameManager.Instance.OnReturnToMenu -= StopAndClearAsteroids;
        
            foreach (var asteroid in spawnedAsteroids)
            {
                if (asteroid != null)
                {
                    var asteroidComponent = asteroid.GetComponent<Shootable>();
                    if (asteroidComponent != null)
                        asteroidComponent.OnShootableDestroyed -= HandleAsteroidDestroyed;
                }
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

        private IEnumerator RunAsteroidSpawner()
        {
            while(isActive)
            {
                yield return new WaitForSeconds(spawnCooldown);
                
                if(spawnedAsteroids.Count < maxAsteroids)
                    SpawnAsteroid();
            }
        }

        private void SpawnAsteroid()
        {
            Shootable asteroid = Instantiate(asteroidPrefab);
            
            Vector2 pos = UnityEngine.Random.insideUnitCircle.normalized * spawnRadius;
            asteroid.transform.position = pos;
            Vector2 targetPos = UnityEngine.Random.insideUnitCircle.normalized * targetRadius;
            Vector2 direction = (targetPos - pos).normalized;

            asteroid.Initialize(
                UnityEngine.Random.Range(speedRange.x, speedRange.y), 
                direction, 
                boundsRadius
            );
            
            asteroid.OnShootableDestroyed += HandleAsteroidDestroyed;
            
            spawnedAsteroids.Add(asteroid);
        }

        private void HandleAsteroidDestroyed(Shootable shootable, DestructionReason reason)
        {
            shootable.OnShootableDestroyed -= HandleAsteroidDestroyed;
            
            if (spawnedAsteroids.Remove(shootable))
                AsteroidDestroyedEvent?.Invoke(shootable, reason);
        }

        private void StopAndClearAsteroids()
        {
            isActive = false;
            
            if(asteroidSpawnCoroutine != null)
            {
                StopCoroutine(asteroidSpawnCoroutine);
                asteroidSpawnCoroutine = null;
            }
            
            ClearAllAsteroids();
        }

        private void ClearAllAsteroids()
        {
            foreach(Shootable shootable in spawnedAsteroids)
            {
                if(shootable != null)
                    Destroy(shootable.gameObject);
            }
            spawnedAsteroids.Clear();
        }
    }
}