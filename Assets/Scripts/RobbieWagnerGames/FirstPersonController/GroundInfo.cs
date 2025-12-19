using UnityEngine;

namespace RobbieWagnerGames.FirstPerson
{
    public enum GroundType
    {
        None,
        Stone,
        Wood,
        Dirt,
        Grass,
        Water,
        Sand,
        Gravel
    }

    /// <summary>
    /// Defines the type of ground surface for footstep sounds and movement effects
    /// </summary>
    public class GroundInfo : MonoBehaviour
    {
        [Tooltip("Type of ground surface")]
        [SerializeField] private GroundType groundType = GroundType.None;
        
        public GroundType Type => groundType;
    }
}