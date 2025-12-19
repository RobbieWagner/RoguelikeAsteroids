using RobbieWagnerGames.Utilities;
using UnityEngine;

namespace RobbieWagnerGames.TileSelectionGame
{
    public class CameraManager : MonoBehaviourSingleton<CameraManager>
    {
        [SerializeField] private Camera _gameCamera;
        [SerializeField] private Vector3 _cameraPosOffset;

        public Camera GameCamera => _gameCamera;

        protected override void Awake()
        {
            base.Awake();
            
            if (_gameCamera == null)
            {
                _gameCamera = Camera.main;
            }
        }
    }
}