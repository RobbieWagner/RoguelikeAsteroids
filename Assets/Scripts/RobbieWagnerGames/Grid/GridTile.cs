using UnityEngine;

namespace RobbieWagnerGames.GridSystem
{
    /// <summary>
    /// Represents the data for a single grid tile
    /// </summary>
    [System.Serializable]
    public class GridTile
    {
        [SerializeField] private TileType type = TileType.Default;
        [SerializeField] private Vector3Int gridPosition;
        [SerializeField] private float movementCost = 1f;
        
        public TileType Type => type;
        public Vector3Int GridPosition => gridPosition;
        public float MovementCost => movementCost;

        /// <summary>
        /// Create a new grid tile
        /// </summary>
        public GridTile(TileType tileType, Vector3Int position, float cost = 1f)
        {
            type = tileType;
            gridPosition = position;
            movementCost = Mathf.Max(0.1f, cost); // Ensure minimal movement cost
        }

        /// <summary>
        /// Change the tile type and automatically adjust movement cost
        /// </summary>
        public void ChangeType(TileType newType)
        {
            type = newType;
            movementCost = GetDefaultMovementCost(newType);
        }

        /// <summary>
        /// Directly set both type and movement cost
        /// </summary>
        public void SetTileProperties(TileType newType, float cost)
        {
            type = newType;
            movementCost = Mathf.Max(0.1f, cost);
        }

        /// <summary>
        /// Get default movement cost for tile type
        /// </summary>
        public static float GetDefaultMovementCost(TileType tileType)
        {
            return tileType switch
            {
                TileType.Water => 2f,
                TileType.Beach => 1.2f,
                TileType.Grass => 1f,
                TileType.Forest => 1.5f,
                _ => 1f
            };
        }
    }

    /// <summary>
    /// Types of tiles available in the grid system
    /// </summary>
    public enum TileType
    {
        None = -1,
        Default = 0,
        Water = 1,
        Beach = 2,
        Grass = 3,
        Forest = 4
    }
}