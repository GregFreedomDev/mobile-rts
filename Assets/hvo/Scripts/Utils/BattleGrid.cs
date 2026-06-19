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

        private GameObject m_HighlightObject;
        private SpriteRenderer m_HighlightRenderer;
        private GameObject m_VisualsRoot;
        private Sprite m_QuadSprite;

        // Render orders: floor tilemap is 14, units are 25 — keep grid visuals in between.
        private const int k_ZoneOrder = 15;
        private const int k_LineOrder = 16;
        private const int k_HighlightOrder = 20;

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

        // Overlay colors (this tilemap's material ignores per-tile tint, so we draw real sprites)
        private static readonly Color k_PlayerZoneColor = new Color(0.25f, 0.5f, 1f, 0.20f);
        private static readonly Color k_EnemyZoneColor  = new Color(1f, 0.3f, 0.3f, 0.20f);
        private static readonly Color k_LineColor       = new Color(0f, 0f, 0f, 0.40f);
        private static readonly Color k_MidLineColor    = new Color(1f, 0.9f, 0.2f, 0.85f);

        public void GenerateGrid()
        {
            m_Tilemap.ClearAllTiles();
            for (int x = 0; x < m_Width; x++)
                for (int y = 0; y < m_Height; y++)
                    m_Tilemap.SetTile(new Vector3Int(x, y, 0), m_FloorTile);

            GenerateGridVisuals();
        }

        // Builds zone tints + grid lines as sprite quads, since tile tinting doesn't work here.
        private void GenerateGridVisuals()
        {
            if (m_VisualsRoot != null) Object.Destroy(m_VisualsRoot);
            m_VisualsRoot = new GameObject("BattleGridVisuals");

            int midpoint = m_Width / 2;
            Vector3 cs = m_Tilemap.cellSize;
            Vector3 origin = m_Tilemap.CellToWorld(new Vector3Int(0, 0, 0));
            float fullW = m_Width * cs.x;
            float fullH = m_Height * cs.y;
            float midX = origin.x + midpoint * cs.x;
            float centerY = origin.y + fullH / 2f;

            // Zone tints
            CreateQuad("PlayerZone",
                new Vector3(origin.x + (midpoint * cs.x) / 2f, centerY, 0f),
                new Vector2(midpoint * cs.x, fullH), k_PlayerZoneColor, k_ZoneOrder);
            CreateQuad("EnemyZone",
                new Vector3(midX + ((m_Width - midpoint) * cs.x) / 2f, centerY, 0f),
                new Vector2((m_Width - midpoint) * cs.x, fullH), k_EnemyZoneColor, k_ZoneOrder);

            float thin = Mathf.Max(cs.x, cs.y) * 0.04f;

            // Vertical lines
            for (int x = 0; x <= m_Width; x++)
            {
                float wx = origin.x + x * cs.x;
                bool isMid = x == midpoint;
                CreateQuad($"VLine_{x}",
                    new Vector3(wx, centerY, 0f),
                    new Vector2(isMid ? thin * 2.5f : thin, fullH),
                    isMid ? k_MidLineColor : k_LineColor,
                    isMid ? k_LineOrder + 1 : k_LineOrder);
            }

            // Horizontal lines
            for (int y = 0; y <= m_Height; y++)
            {
                float wy = origin.y + y * cs.y;
                CreateQuad($"HLine_{y}",
                    new Vector3(origin.x + fullW / 2f, wy, 0f),
                    new Vector2(fullW, thin), k_LineColor, k_LineOrder);
            }
        }

        private void CreateQuad(string name, Vector3 center, Vector2 size, Color color, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(m_VisualsRoot.transform, false);
            go.transform.position = new Vector3(center.x, center.y, 0f);
            go.transform.localScale = new Vector3(size.x, size.y, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = GetQuadSprite();
            sr.color = color;
            sr.sortingOrder = order;
        }

        private Sprite GetQuadSprite()
        {
            if (m_QuadSprite == null)
            {
                Texture2D tex = Texture2D.whiteTexture;
                m_QuadSprite = Sprite.Create(
                    tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), tex.width);
            }
            return m_QuadSprite;
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

        // Highlight colors — cyan reads as "placeable" against the green floor, red as blocked
        private static readonly Color k_ValidColor   = new Color(0.3f, 0.9f, 1f, 0.6f);
        private static readonly Color k_InvalidColor = new Color(1f, 0.25f, 0.25f, 0.6f);

        public void HighlightCell(Vector3Int cellPos, bool isPlayer)
        {
            if (!IsInBounds(cellPos))
            {
                ClearHighlight();
                return;
            }

            EnsureHighlight();
            bool valid = CanPlaceUnit(cellPos, isPlayer);
            m_HighlightObject.transform.position = m_Tilemap.GetCellCenterWorld(cellPos);
            m_HighlightRenderer.color = valid ? k_ValidColor : k_InvalidColor;
            m_HighlightObject.SetActive(true);
        }

        public void ClearHighlight()
        {
            if (m_HighlightObject != null)
                m_HighlightObject.SetActive(false);
        }

        // Lazily build a single reusable quad sprite that marks the target cell.
        private void EnsureHighlight()
        {
            if (m_HighlightObject != null) return;

            m_HighlightObject = new GameObject("CellHighlight");
            m_HighlightRenderer = m_HighlightObject.AddComponent<SpriteRenderer>();
            m_HighlightRenderer.sprite = GetQuadSprite();
            m_HighlightRenderer.sortingOrder = k_HighlightOrder; // above grid lines, below units

            Vector3 cellSize = m_Tilemap.cellSize;
            m_HighlightObject.transform.localScale = new Vector3(cellSize.x, cellSize.y, 1f);
            m_HighlightObject.SetActive(false);
        }

        private bool IsInBounds(Vector3Int pos)
        {
            return pos.x >= 0 && pos.x < m_Width && pos.y >= 0 && pos.y < m_Height;
        }
    }
}
