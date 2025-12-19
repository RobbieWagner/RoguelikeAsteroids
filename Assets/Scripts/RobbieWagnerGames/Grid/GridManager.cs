using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using RobbieWagnerGames.Utilities;

namespace RobbieWagnerGames.GridSystem
{
    /// <summary>
    /// Singleton manager for grid system with tilemaps and tile data
    /// </summary>
    [RequireComponent(typeof(Grid))]
    public class GridManager : MonoBehaviourSingleton<GridManager>
    {
        [Header("Grid Settings")]
        [SerializeField] private Tilemap mainTilemap;
        [SerializeField] private int gridSize = 10;
        [SerializeField] private TileBase defaultTile;
        [SerializeField] private bool initializeOnAwake = true;

        private Dictionary<Vector3Int, GridTile> tileData = new Dictionary<Vector3Int, GridTile>();
        private Grid unityGrid;

        public Tilemap CurrentTilemap => mainTilemap;
        public int CurrentSize => gridSize;
        public Grid UnityGrid => unityGrid;

        protected override void Awake()
        {
            base.Awake();
            unityGrid = GetComponent<Grid>();

            if (initializeOnAwake)
            {
                InitializeGrid();
            }
        }

        /// <summary>
        /// Initialize the grid with default tiles
        /// </summary>
        public void InitializeGrid()
        {
            if (mainTilemap == null)
            {
                Debug.LogError("Main Tilemap reference is not set!", this);
                return;
            }

            tileData.Clear();
            
            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    Vector3Int position = new Vector3Int(x, y, 0);
                    SetTile(position, defaultTile, TileType.Default);
                }
            }
        }

        /// <summary>
        /// Convert world position to grid cell position
        /// </summary>
        public Vector3Int WorldToCell(Vector3 worldPosition)
        {
            return unityGrid.WorldToCell(worldPosition);
        }

        /// <summary>
        /// Convert grid cell position to world position
        /// </summary>
        public Vector3 CellToWorld(Vector3Int cellPosition)
        {
            return unityGrid.CellToWorld(cellPosition) + unityGrid.cellSize * 0.5f; // Center of cell
        }

        /// <summary>
        /// Get tile data at specific grid coordinates
        /// </summary>
        public GridTile GetTileAt(Vector3Int position)
        {
            return tileData.TryGetValue(position, out GridTile tile) ? tile : null;
        }

        /// <summary>
        /// Set or update a tile at specific grid coordinates
        /// </summary>
        public void SetTile(Vector3Int position, TileBase visualTile, TileType tileType, float movementCost = -1f)
        {
            if (!IsPositionValid(position))
            {
                Debug.LogWarning($"Position {position} is outside grid bounds");
                return;
            }

            if (movementCost < 0)
            {
                movementCost = GridTile.GetDefaultMovementCost(tileType);
            }

            mainTilemap.SetTile(position, visualTile);
            tileData[position] = new GridTile(tileType, position, movementCost);
        }

        /// <summary>
        /// Change tile type at specific grid coordinates
        /// </summary>
        public void ChangeTileType(Vector3Int position, TileType newType, TileBase visualTile = null)
        {
            if (!tileData.TryGetValue(position, out GridTile tile))
            {
                Debug.LogWarning($"No tile found at position {position}");
                return;
            }

            tile.ChangeType(newType);
            mainTilemap.SetTile(position, visualTile ?? defaultTile);
        }

        /// <summary>
        /// Check if a position is within grid bounds
        /// </summary>
        public bool IsPositionValid(Vector3Int position)
        {
            return position.x >= 0 && position.x < gridSize && 
                   position.y >= 0 && position.y < gridSize;
        }

        /// <summary>
        /// Clear all tiles from the grid
        /// </summary>
        public void ClearGrid()
        {
            mainTilemap.ClearAllTiles();
            tileData.Clear();
        }

        protected override void OnDestroy()
        {
            // Clean up any resources if needed
            if (Instance == this)
            {
                tileData.Clear();
            }
            base.OnDestroy();
        }

        #region Debug
        /// <summary>
        /// Draw debug gizmos to visualize grid cells
        /// </summary>
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || unityGrid == null) return;

            Gizmos.color = Color.cyan;
            foreach (var kvp in tileData)
            {
                Vector3 worldPos = CellToWorld(kvp.Key);
                Gizmos.DrawWireCube(worldPos, unityGrid.cellSize * 0.9f);
            }
        }
        #endregion
    }
}