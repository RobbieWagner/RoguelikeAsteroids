using UnityEngine;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
	public class BurstAsteroid : Asteroid
	{
		[SerializeField] private Asteroid asteroidChunkPrefab;
		[SerializeField] private int chunksToSpawn;
		[SerializeField] private Vector2 chunkSpeedRange;
		[SerializeField] private float bounds = 3;

        protected override void OnDestroy()
        {
			if(doDestructionEffects)
			{
				for (int i = 0; i < chunksToSpawn; i++)
					ShootableManager.Instance.SpawnAsteroid(asteroidChunkPrefab, Random.Range(chunkSpeedRange.x, chunkSpeedRange.y), Random.insideUnitCircle, transform.position, bounds);
			}
			
            base.OnDestroy();
        }
	}
}