using System;
using System.Collections.Generic;
using System.Linq;
using hvo.Scripts.Utils;
using HvO.UI;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace hvo.Scripts.Managers
{
    public class BattleGameManager : BaseGameManager
    {
        [SerializeField] private Canvas m_MainCanvas;
        [SerializeField] private GameObject m_UnitListPanelPrefab;
        [SerializeField] private UnitElementBar m_UnitElementPrefab;
        [SerializeField] private TileBase m_floorTile;
        [SerializeField] private Tilemap m_Tilemap;
        [SerializeField] private Button m_ButtonPrefab;
        [SerializeField] private TextPopupController m_TextPopupController;

        private BattleGrid m_BattleGrid;
        private int m_Width = 14, m_Height = 5;
        private bool m_IsBattleStarted = false;

        public BattleGrid BattleGrid => m_BattleGrid;
        public bool IsBattleStarted => m_IsBattleStarted;

        private readonly string[] unitPrefabNames = new string[]
        {
            "Warrior",
            "Archer",
            "Demolisher",
            "Goblin"
        };

        private void Start()
        {
            base.Awake();
            base.AddResources(500,500);
            InitializePanelUnitUI();
            m_BattleGrid = new BattleGrid(m_Width, m_Height, m_floorTile, m_Tilemap);
            GenerateBattleGrid();
            m_ButtonPrefab.onClick.AddListener(StartBattle);
            //SpawnUnitsInArena();
        }

        protected void ResetStartBattle()
        {
            Time.timeScale = 1;
            m_GameState = GameState.Playing;
            m_IsBattleStarted = false;
            m_PlayerUnits.Clear();
            m_Enemies.Clear();
        }

        private void StartBattle()
        {
            if (GetAllPlayerUnits().ToList().Count > 0 && !m_IsBattleStarted && GetAllEnemiesUnits().ToList().Count > 0)
            {
                m_IsBattleStarted = true;

                Debug.Log("⚔️ ¡Batalla iniciada!");

                foreach (Unit unit in GetAllPlayerUnits().ToList())
                {
                    unit.BeginBattle();
                }

                foreach (Unit unit in GetAllEnemiesUnits().ToList())
                {
                    unit.BeginBattle();
                }
            }
            else
            {
                Debug.LogWarning("No se pudo iniciar la batalla. Verifica que haya unidades de ambos lados.");
            }
        }
        
        
        public void SpawnUnitsInArena(int warriorsPerRow = 2, int goblinsPerRow = 2)
        {
            if (BattleGrid == null)
            {
                Debug.LogWarning("BattleGrid no está inicializado.");
                return;
            }

            var tilemap = BattleGrid.Tilemap;
            var width = 14; // o BattleGrid.Width si está expuesto
            var height = 5;  // o BattleGrid.Height si está expuesto

            GameObject warriorPrefab = Resources.Load<GameObject>("Prefabs/Units/Warrior");
            GameObject goblinPrefab = Resources.Load<GameObject>("Prefabs/Units/Goblin");

            if (warriorPrefab == null || goblinPrefab == null)
            {
                Debug.LogError("No se encontraron los prefabs Warrior o Goblin en Resources.");
                return;
            }

            for (int y = 0; y < height; y++)
            {
                // Spawn Warriors en el lado izquierdo (columnas 0–6)
                for (int x = 0; x < warriorsPerRow; x++)
                {
                    Vector3Int cell = new Vector3Int(x, y, 0);
                    Vector3 worldPos = tilemap.GetCellCenterWorld(cell);
                    GameObject warrior = Instantiate(warriorPrefab, worldPos, Quaternion.identity);
                }

                // Spawn Goblins en el lado derecho (columnas 7–13)
                for (int x = 0; x < goblinsPerRow; x++)
                {
                    Vector3Int cell = new Vector3Int(width - 1 - x, y, 0);
                    Vector3 worldPos = tilemap.GetCellCenterWorld(cell);
                    GameObject goblin = Instantiate(goblinPrefab, worldPos, Quaternion.identity);
                }
            }
        }


        
        
        private void GenerateBattleGrid()
        {
            m_BattleGrid.GenerateGrid();
        }

        protected override void GoToBattle()
        {
            ResetStartBattle();
            base.GoToBattle();
        }

        private void InitializePanelUnitUI()
        {
            GameObject panelInstance = Instantiate(m_UnitListPanelPrefab, m_MainCanvas.transform);
            UnitListBar unitListUI = panelInstance.AddComponent<UnitListBar>();
            unitListUI.Initialize(m_UnitElementPrefab);
            
            foreach (string prefabName in unitPrefabNames)
            {
                GameObject unitPrefab = Resources.Load<GameObject>($"Prefabs/Units/{prefabName}");
                if (unitPrefab != null)
                {
                    unitListUI.AddUnit(unitPrefab);
                }
                else
                {
                    Debug.LogWarning($"No se pudo cargar el prefab de la unidad: {prefabName}");
                }
            }
        }
        
        public override void ShowTextPopup(string text, Vector3 position, Color color)
        {
            m_TextPopupController.Spawn(text, position, color);
        }

        public override void RegisterUnit(Unit unit)
        {
            if (unit.IsPlayer)
            {
                m_PlayerUnits.Add(unit);
            }
            else
            {
                m_Enemies.Add(unit);
            }
        }

        public override void UnregisterUnit(Unit unit)
        {
            if (unit.IsPlayer)
            {
                m_PlayerUnits.Remove(unit);
            }
            else
            {
                m_Enemies.Remove(unit);
            }
        }
    }
}