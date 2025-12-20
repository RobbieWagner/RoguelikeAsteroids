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

        [Header("Movement")]
        [SerializeField] private float targetRadius;
        [SerializeField] private Vector2 speedRange;
        [SerializeField] private float spawnCooldown = 2f;

        private Coroutine asteroidSpawnCoroutine = null;

        // protected override void Awake()
        // {
        //     base.Awake();
        //     StartCoroutine(RunAsteroidSpawner());
        // }

        public void StartAsteroidSpawner()
        {
            if(asteroidSpawnCoroutine == null)
                asteroidSpawnCoroutine = StartCoroutine(RunAsteroidSpawner());
        }

        public IEnumerator RunAsteroidSpawner()
        {
            while(true)
            {
                yield return new WaitForSeconds(spawnCooldown);
                SpawnAsteroid();
            }
        }

        public void SpawnAsteroid()
        {
            Shootable asteroid = Instantiate(asteroidPrefab);

            Vector2 pos = UnityEngine.Random.insideUnitCircle.normalized * spawnRadius;
            asteroid.transform.position = pos;
            Vector2 targetPos = UnityEngine.Random.insideUnitCircle.normalized * targetRadius;
            Vector2 direction = (targetPos - pos).normalized;

            // Debug.Log($"pos: {pos}\ntarget: {targetPos}\ndirection{direction}");

            asteroid.SetMovement(UnityEngine.Random.Range(speedRange.x, speedRange.y), direction);
            spawnedAsteroids.Add(asteroid);
        }

        public void OnDestroyAsteroid(Shootable shootable)
        {
            spawnedAsteroids.Remove(shootable);
        }

        public void StopAsteroidSpawner()
        {
            StopCoroutine(asteroidSpawnCoroutine);
            asteroidSpawnCoroutine = null;
        }

        public void DestroyAllAsteroids()
        {
            foreach(Shootable shootable in spawnedAsteroids)
                Destroy(shootable.gameObject);
            spawnedAsteroids.Clear();
        }
    }
}