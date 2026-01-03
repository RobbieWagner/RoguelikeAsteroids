using UnityEngine;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
	public class ParallaxEffect : MonoBehaviour
	{
		private float startPos;
		private float length;
		[SerializeField] private SpriteRenderer mainSpriteRenderer;
		[SerializeField] private float parallaxEffect;

		[SerializeField] private Transform parallaxFollow;

		private void Awake()
		{
			startPos = transform.position.y;
			length = mainSpriteRenderer.bounds.size.y;
		}

        private void FixedUpdate()
        {
            float distance = parallaxFollow.position.y * parallaxEffect;
			float movement = parallaxFollow.position.y * (1 - parallaxEffect);

			transform.position = new Vector2(transform.position.x, startPos + distance);

			if (movement > startPos + length)
				startPos += length;
			else if (movement < startPos - length)
				startPos -= length;
        }
    }
}