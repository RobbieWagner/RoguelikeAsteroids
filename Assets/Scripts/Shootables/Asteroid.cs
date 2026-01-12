using System.Collections;
using RobbieWagnerGames.Audio;
using UnityEngine;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
    public class Asteroid : Shootable
    {
        [SerializeField] private bool generateResources = true;
        [SerializeField] private Collider2D coll;
        [SerializeField] private float colliderEnableDelay = 0f;

        protected override void Awake()
        {
            if (colliderEnableDelay > 0)
            {
                coll.enabled = false;
                StartCoroutine(DelayEnable());
            }
            base.Awake();
        }

        protected override void OnShootableDestroyedFromShot()
        {
            BasicAudioManager.Instance.Play(AudioSourceName.AsteroidDestroyed);
            base.OnShootableDestroyedFromShot();
        }

        protected override void GenerateRandomResources()
        {
            resourceData = new ResourceGatherData();

            if (!generateResources) return;
            
            int totalResources = Random.Range(3, 8);
            
            for (int i = 0; i < totalResources; i++)
            {
                float roll = Random.value;
                if (roll < 0.6f)
                    resourceData.AddResource(ResourceType.TITANIUM, 1);
                else if (roll < 0.9f)
                    resourceData.AddResource(ResourceType.PLATINUM, 1);
                else
                    resourceData.AddResource(ResourceType.IRIDIUM, 1);
            }
        }

        private IEnumerator DelayEnable()
        {
            yield return new WaitForSeconds(colliderEnableDelay);
            coll.enabled = true;
        }
    }
}