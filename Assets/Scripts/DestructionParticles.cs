using UnityEngine;
using System.Collections;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
	public class DestructionParticles : MonoBehaviour
	{
		[SerializeField] private ParticleSystem particles;

		public IEnumerator PlayParticles(float timeToLive = 1f, Color? color = null)
		{
			if (color != null)
			{
				ParticleSystem.ColorOverLifetimeModule col = particles.colorOverLifetime;	
				col.enabled = true;

				Gradient grad = new Gradient();

				grad.SetKeys(
					new GradientColorKey[] { new GradientColorKey(color.Value, 0.0f), new GradientColorKey(color.Value, 1.0f) },
            		new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
				);
			} 

			particles.Play();
			yield return new WaitForSeconds(timeToLive);
			particles.Stop();
			yield return new WaitForSeconds(particles.main.startLifetime.constant);
			Destroy(gameObject);
		}
	}
}