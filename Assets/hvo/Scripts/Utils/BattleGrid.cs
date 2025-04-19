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

        public void CenterMap()
        {
            Vector3 offset = new Vector3(-m_Width / 2f + 0.5f, -m_Height / 2f + 0.5f, 0f);
            m_Tilemap.transform.position = offset;
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