using RobbieWagnerGames.Audio;
using UnityEngine;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
    public class EnemyShip : Shootable
    {
        protected override void OnShootableDestroyedFromShot()
        {
            BasicAudioManager.Instance.Play(AudioSourceName.AsteroidDestroyed);
            base.OnShootableDestroyedFromShot();
        }
    }
}
