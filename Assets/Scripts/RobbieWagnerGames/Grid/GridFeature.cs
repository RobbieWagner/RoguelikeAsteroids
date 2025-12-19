using UnityEngine;
using UnityEngine.Tilemaps;

namespace RobbieWagnerGames.GridSystem
{
    /// <summary>
    /// Base class for features that can be added to a grid
    /// </summary>
    public abstract class GridFeature : MonoBehaviour
    {
        [Header("Grid Feature Settings")]
        [SerializeField] protected Vector3Int startPosition;
        [SerializeField] protected bool useStartingPosition = true;
        [SerializeField] protected TileBase featureTile;
        
        /// <summary>
        /// Add this feature to the specified grid
        /// </summary>
        public virtual void AddToGrid(GridManager grid)
        {
            if (grid == null)
            {
                Debug.LogWarning("Cannot add feature to null grid", this);
                return;
            }

            Tilemap tilemap = grid.CurrentTilemap;
            if (tilemap == null)
            {
                Debug.LogWarning("Grid has no active tilemap", this);
                return;
            }

            Vector3Int position = useStartingPosition ? startPosition : grid.WorldToCell(transform.position);
            ApplyFeature(tilemap, position, grid.CurrentSize);
        }

        /// <summary>
        /// Apply the specific feature implementation to the tilemap
        /// </summary>
        protected abstract void ApplyFeature(Tilemap tilemap, Vector3Int position, int gridSize);

        /// <summary>
        /// Remove this feature from the specified grid
        /// </summary>
        public virtual void RemoveFromGrid(GridManager grid)
        {
            if (grid == null) return;

            Tilemap tilemap = grid.CurrentTilemap;
            if (tilemap == null) return;

            Vector3Int position = useStartingPosition ? startPosition : grid.WorldToCell(transform.position);
            tilemap.SetTile(position, null);
        }
    }
}