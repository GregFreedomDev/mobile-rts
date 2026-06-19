namespace hvo.Scripts.Utils
{
    using UnityEngine;
    using UnityEngine.Tilemaps;

    public class BattleGrid
    {
        private readonly Tilemap m_Tilemap;
        private readonly TileBase m_FloorTile;
        private readonly Unit[,] m_Grid;
        private readonly int m_Width;
        private readonly int m_Height;

        private Vector3Int m_HighlightedCell = new Vector3Int(-1, -1, 0);

        public Tilemap Tilemap => m_Tilemap;
        public int Width => m_Width;
        public int Height => m_Height;

        public BattleGrid(int width, int height, TileBase floorTile, Tilemap tilemap)
        {
            m_FloorTile = floorTile;
            m_Tilemap = tilemap;
            m_Width = width;
            m_Height = height;
            m_Grid = new Unit[m_Width, m_Height];
        }

        public void GenerateGrid()
        {
            m_Tilemap.ClearAllTiles();
            for (int x = 0; x < m_Width; x++)
                for (int y = 0; y < m_Height; y++)
                    m_Tilemap.SetTile(new Vector3Int(x, y, 0), m_FloorTile);
        }

        // Overload for drag from panel (pass isPlayer flag)
        public bool CanPlaceUnit(Vector3Int cellPos, bool isPlayer)
        {
            if (!IsInBounds(cellPos)) return false;
            if (m_Grid[cellPos.x, cellPos.y] != null) return false;

            int midpoint = m_Width / 2;
            if (isPlayer && cellPos.x >= midpoint) return false;
            if (!isPlayer && cellPos.x < midpoint) return false;

            return true;
        }

        // Overload for repositioning an already-placed unit
        public bool CanPlaceUnit(Vector3Int cellPos, Unit unit)
        {
            return CanPlaceUnit(cellPos, unit.IsPlayer);
        }

        public void RegisterUnit(Vector3Int cellPos, Unit unit)
        {
            if (IsInBounds(cellPos))
                m_Grid[cellPos.x, cellPos.y] = unit;
        }

        public Vector3 ClampToBounds(Vector3 worldPosition)
        {
            Vector3Int cellPos = m_Tilemap.WorldToCell(worldPosition);
            cellPos.x = Mathf.Clamp(cellPos.x, 0, m_Width - 1);
            cellPos.y = Mathf.Clamp(cellPos.y, 0, m_Height - 1);
            return m_Tilemap.GetCellCenterWorld(cellPos);
        }

        public void HighlightCell(Vector3Int cellPos, bool isPlayer)
        {
            ClearHighlight();
            if (!IsInBounds(cellPos)) return;

            bool valid = CanPlaceUnit(cellPos, isPlayer);
            m_Tilemap.SetTileFlags(cellPos, TileFlags.None);
            m_Tilemap.SetColor(cellPos, valid ? new Color(0.3f, 1f, 0.3f, 0.6f) : new Color(1f, 0.3f, 0.3f, 0.6f));
            m_HighlightedCell = cellPos;
        }

        public void ClearHighlight()
        {
            if (m_HighlightedCell.x < 0) return;
            if (IsInBounds(m_HighlightedCell))
            {
                m_Tilemap.SetTileFlags(m_HighlightedCell, TileFlags.None);
                m_Tilemap.SetColor(m_HighlightedCell, Color.white);
            }
            m_HighlightedCell = new Vector3Int(-1, -1, 0);
        }

        private bool IsInBounds(Vector3Int pos)
        {
            return pos.x >= 0 && pos.x < m_Width && pos.y >= 0 && pos.y < m_Height;
        }
    }
}
