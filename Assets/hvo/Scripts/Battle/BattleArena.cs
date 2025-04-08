using UnityEngine;
using System.Collections.Generic;


    public class BattleArena : MonoBehaviour
    {
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private int gridWidth = 20;  // Ancho del grid (X)
        [SerializeField] private int gridHeight = 10; // Alto del grid (Y)
        [SerializeField] private Color gridColor = new Color(1f, 1f, 1f, 0.3f);
        [SerializeField] private Color occupiedColor = new Color(1f, 0f, 0f, 0.4f);
        [SerializeField] private Color validColor = new Color(0f, 1f, 0f, 0.4f);
        [SerializeField] private Color enemyZoneColor = new Color(1f, 0.5f, 0.5f, 0.3f);
        [SerializeField] private Color playerZoneColor = new Color(0.5f, 0.5f, 1f, 0.3f);
        [SerializeField] private Sprite gridCellSprite;

        // Propiedades públicas para acceder al tamaño del grid
        public int GridWidth => gridWidth;
        public int GridHeight => gridHeight;

        private bool[,] occupiedCells;
        private Dictionary<Unit, Vector2Int> unitPositions;
        private GameObject[,] gridCells;

        private void Start()
        {
            if (gridCellSprite == null)
            {
                Debug.LogError("Grid Cell Sprite no está asignado!");
                return;
            }

            occupiedCells = new bool[gridWidth, gridHeight];
            unitPositions = new Dictionary<Unit, Vector2Int>();
            gridCells = new GameObject[gridWidth, gridHeight];
            CreateGridVisual();
        }

        private void CreateGridVisual()
        {
            float halfWidth = gridWidth * cellSize * 0.5f;
            float halfHeight = gridHeight * cellSize * 0.5f;

            GameObject container = new GameObject("GridContainer");
            container.transform.SetParent(transform);
            container.transform.localPosition = Vector3.zero;

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    GameObject cell = new GameObject($"GridCell_{x}_{y}");
                    cell.transform.SetParent(container.transform);
                    
                    Vector3 position = new Vector3(
                        x * cellSize - halfWidth + cellSize * 0.5f,
                        y * cellSize - halfHeight + cellSize * 0.5f,
                        0f
                    );
                    cell.transform.localPosition = position;

                    SpriteRenderer renderer = cell.AddComponent<SpriteRenderer>();
                    renderer.sprite = gridCellSprite;
                    renderer.sortingOrder = 11;
                    
                    // Establecer color según la zona (enemigos a la derecha, jugadores a la izquierda)
                    renderer.color = x >= gridWidth / 2 ? enemyZoneColor : playerZoneColor;
                    
                    // Establecer escala a 1x1
                    renderer.transform.localScale = Vector3.one;

                    gridCells[x, y] = cell;
                }
            }
        }

        public Vector3 GetWorldPosition(Vector2Int gridPosition)
        {
            float halfWidth = gridWidth * cellSize * 0.5f;
            float halfHeight = gridHeight * cellSize * 0.5f;
            return new Vector3(
                gridPosition.x * cellSize - halfWidth + cellSize * 0.5f,
                gridPosition.y * cellSize - halfHeight + cellSize * 0.5f,
                0f
            );
        }

        public Vector2Int GetGridPosition(Vector3 worldPosition)
        {
            float halfWidth = gridWidth * cellSize * 0.5f;
            float halfHeight = gridHeight * cellSize * 0.5f;
            int x = Mathf.FloorToInt((worldPosition.x + halfWidth) / cellSize);
            int y = Mathf.FloorToInt((worldPosition.y + halfHeight) / cellSize);
            return new Vector2Int(x, y);
        }

        public bool IsValidPosition(Vector2Int gridPosition, bool isEnemy)
        {
            // Verificar límites del grid
            if (gridPosition.x < 0 || gridPosition.x >= gridWidth ||
                gridPosition.y < 0 || gridPosition.y >= gridHeight ||
                occupiedCells[gridPosition.x, gridPosition.y])
            {
                return false;
            }

            // Verificar zona correcta (enemigos a la derecha, jugadores a la izquierda)
            bool isEnemyZone = gridPosition.x >= gridWidth / 2;
            return isEnemy == isEnemyZone;
        }

        public bool PlaceUnit(Unit unit, Vector2Int gridPosition)
        {
            bool isEnemy = unit.GetComponent<EnemyUnit>() != null;
            if (!IsValidPosition(gridPosition, isEnemy)) return false;

            occupiedCells[gridPosition.x, gridPosition.y] = true;
            unitPositions[unit] = gridPosition;
            
            // Posicionar la unidad y orientarla según su equipo
            Vector3 worldPos = GetWorldPosition(gridPosition);
            unit.transform.position = worldPos;
            
            // Girar las unidades para que se miren entre sí
            unit.transform.localScale = new Vector3(isEnemy ? -1 : 1, 1, 1);

            if (gridCells[gridPosition.x, gridPosition.y] != null)
            {
                gridCells[gridPosition.x, gridPosition.y].GetComponent<SpriteRenderer>().color = occupiedColor;
            }

            return true;
        }

        public void RemoveUnit(Unit unit)
        {
            if (unitPositions.TryGetValue(unit, out Vector2Int position))
            {
                occupiedCells[position.x, position.y] = false;
                unitPositions.Remove(unit);

                if (gridCells[position.x, position.y] != null)
                {
                    bool isEnemyZone = position.x >= gridWidth / 2;
                    gridCells[position.x, position.y].GetComponent<SpriteRenderer>().color = 
                        isEnemyZone ? enemyZoneColor : playerZoneColor;
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;

            float halfWidth = gridWidth * cellSize * 0.5f;
            float halfHeight = gridHeight * cellSize * 0.5f;
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    Vector3 center = new Vector3(
                        x * cellSize - halfWidth + cellSize * 0.5f,
                        y * cellSize - halfHeight + cellSize * 0.5f,
                        0f
                    );

                    Gizmos.color = occupiedCells[x, y] ? occupiedColor : gridColor;
                    Gizmos.DrawCube(center, new Vector3(cellSize * 0.9f, cellSize * 0.9f, 0.1f));
                }
            }
        }
    }
