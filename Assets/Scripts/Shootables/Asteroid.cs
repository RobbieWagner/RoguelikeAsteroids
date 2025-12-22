using RobbieWagnerGames.Audio;
using RobbieWagnerGames.RoguelikeAsteroids;
using UnityEngine;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
    public class Asteroid : Shootable
    {
        protected override void OnShootableDestroyedFromShot()
        {
            BasicAudioManager.Instance.Play(AudioSourceName.AsteroidDestroyed);
            base.OnShootableDestroyedFromShot();
        }

        protected override void GenerateRandomResources()
        {
            resourceData = new ResourceGatherData();
            
            int totalResources = UnityEngine.Random.Range(3, 8);
            
            for (int i = 0; i < totalResources; i++)
            {
                float roll = UnityEngine.Random.value;
                if (roll < 0.6f)
                    resourceData.AddResource(ResourceType.TITANIUM, 1);
                else if (roll < 0.9f)
                    resourceData.AddResource(ResourceType.PLATINUM, 1);
                else
                    resourceData.AddResource(ResourceType.IRIDIUM, 1);
            }
        }
    }
}