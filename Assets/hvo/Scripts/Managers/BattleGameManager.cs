using System;
using System.Collections.Generic;
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
            InitializePanelUnitUI();
            m_BattleGrid = new BattleGrid(m_Width, m_Height, m_floorTile, m_Tilemap);
            GenerateBattleGrid();
            m_ButtonPrefab.onClick.AddListener(StartBattle);
        }

        private void StartBattle()
        {
            if(m_PlayerUnits.Count > 0 && !m_IsBattleStarted && m_Enemies.Count > 0)
                m_IsBattleStarted = true;
            else
                m_IsBattleStarted = false;
        }

        private void GenerateBattleGrid()
        {
            m_BattleGrid.GenerateGrid();
            m_BattleGrid.CenterMap();
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