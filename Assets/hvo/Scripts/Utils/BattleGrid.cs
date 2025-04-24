namespace hvo.Scripts.Utils
{
    using UnityEngine;
    using UnityEngine.Tilemaps;

    public class BattleGrid
    {
        private Tilemap m_Tilemap;
        private TileBase m_FloorTile;
        private Unit[,] m_Grid;
        private int m_Width = 14, m_Height = 5; 
        public Tilemap Tilemap => m_Tilemap;
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
            {
                for (int y = 0; y < m_Height; y++)
                {
                    Vector3Int tilePos = new Vector3Int(x, y, 0);
                    m_Tilemap.SetTile(tilePos, m_FloorTile);
                }
            }
        }
        public bool CanPlaceUnit(Vector3Int cellPos, Unit unit)
        {
            if (!IsInBounds(cellPos)) return false;
            if (m_Grid[cellPos.x, cellPos.y] != null) return false;

            var isWarrior = unit.IsPlayer;
            switch (isWarrior)
            {
                case true when cellPos.x > 6:
                case false when cellPos.x < 7:
                    return false;
                default:
                    return true;
            }
        }
        
        public Vector3 ClampToBounds(Vector3 worldPosition)
        {
            Vector3Int cellPos = m_Tilemap.WorldToCell(worldPosition);
            cellPos.x = Mathf.Clamp(cellPos.x, 0, m_Width - 1);
            cellPos.y = Mathf.Clamp(cellPos.y, 0, m_Height - 1);
            return m_Tilemap.GetCellCenterWorld(cellPos);
        }


        public void RegisterUnit(Vector3Int cellPos, Unit unit)
        {
            m_Grid[cellPos.x, cellPos.y] = unit;
        }

        private bool IsInBounds(Vector3Int pos)
        {
            return pos.x >= 0 && pos.x < m_Width && pos.y >= 0 && pos.y < m_Height;
        }
    }

}