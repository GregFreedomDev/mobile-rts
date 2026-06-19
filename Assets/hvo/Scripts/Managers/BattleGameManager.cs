using System.Collections.Generic;
using System.Linq;
using hvo.Scripts.Spawning;
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
        [SerializeField] private TileBase m_FloorTile;
        [SerializeField] private Tilemap m_Tilemap;
        [SerializeField] private Button m_StartBattleButton;
        [SerializeField] private TextPopupController m_TextPopupController;
        [SerializeField] private BattleTimer m_BattleTimer;
        [SerializeField] private EnemyFormationSO m_EnemyFormation;
        [SerializeField] private int m_MaxPlayerUnits = 10;

        private BattleGrid m_BattleGrid;
        private int m_Width = 14, m_Height = 5;
        private bool m_IsBattleStarted;
        private int m_InitialPlayerUnitCount;
        private int m_InitialEnemyUnitCount;

        public BattleGrid BattleGrid => m_BattleGrid;
        public bool IsBattleStarted => m_IsBattleStarted;
        public int MaxPlayerUnits => m_MaxPlayerUnits;

        private readonly string[] m_UnitPrefabNames = { "Warrior", "Archer", "Demolisher" };

        private new void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            m_BattleGrid = new BattleGrid(m_Width, m_Height, m_FloorTile, m_Tilemap);
            m_BattleGrid.GenerateGrid();

            InitializePanelUnitUI();
            SpawnEnemyFormation();

            m_StartBattleButton.onClick.AddListener(StartBattle);

            if (m_BattleTimer != null)
                m_BattleTimer.OnTimerExpired += OnTimeUp;
        }

        private void Update()
        {
            if (!m_IsBattleStarted) return;

            if (!GetAllEnemiesUnits().Any())
            {
                EndBattle(true);
            }
            else if (!GetAllPlayerUnits().Any())
            {
                EndBattle(false);
            }
        }

        private void StartBattle()
        {
            var playerUnits = GetAllPlayerUnits().ToList();
            var enemyUnits = GetAllEnemiesUnits().ToList();

            if (playerUnits.Count == 0 || enemyUnits.Count == 0 || m_IsBattleStarted) return;

            m_IsBattleStarted = true;
            m_InitialPlayerUnitCount = playerUnits.Count;
            m_InitialEnemyUnitCount = enemyUnits.Count;

            foreach (Unit unit in playerUnits) unit.BeginBattle();
            foreach (Unit unit in enemyUnits) unit.BeginBattle();

            if (m_BattleTimer != null) m_BattleTimer.StartTimer();

            m_StartBattleButton.interactable = false;
        }

        private void OnTimeUp()
        {
            if (!m_IsBattleStarted) return;

            int playerHp = GetAllPlayerUnits().Sum(u => u.CurrentHealth);
            int enemyHp = GetAllEnemiesUnits().Sum(u => u.CurrentHealth);
            EndBattle(playerHp >= enemyHp);
        }

        private void EndBattle(bool isVictory)
        {
            if (!m_IsBattleStarted) return;
            m_IsBattleStarted = false;

            if (m_BattleTimer != null) m_BattleTimer.StopTimer();

            int lost = m_InitialPlayerUnitCount - GetAllPlayerUnits().Count();
            float elapsed = m_BattleTimer != null ? m_BattleTimer.Elapsed : 0f;
            int stars = StarRatingCalculator.Calculate(isVictory, elapsed, lost, m_InitialPlayerUnitCount);

            HandleGameOver(isVictory, stars);
        }

        private void SpawnEnemyFormation()
        {
            if (m_EnemyFormation == null) return;

            foreach (var entry in m_EnemyFormation.enemies)
            {
                if (entry.prefab == null) continue;

                Vector3Int cellPos = new Vector3Int(entry.column, entry.row, 0);
                if (!m_BattleGrid.CanPlaceUnit(cellPos, isPlayer: false)) continue;

                Vector3 worldPos = m_Tilemap.GetCellCenterWorld(cellPos);
                GameObject go = Instantiate(entry.prefab, worldPos, Quaternion.identity);
                m_BattleGrid.RegisterUnit(cellPos, go.GetComponent<Unit>());
            }
        }

        private void InitializePanelUnitUI()
        {
            GameObject panelInstance = Instantiate(m_UnitListPanelPrefab, m_MainCanvas.transform);
            UnitListBar unitListUI = panelInstance.AddComponent<UnitListBar>();
            unitListUI.Initialize(m_UnitElementPrefab);

            foreach (string prefabName in m_UnitPrefabNames)
            {
                GameObject unitPrefab = Resources.Load<GameObject>($"Prefabs/Units/{prefabName}");
                if (unitPrefab != null)
                    unitListUI.AddUnit(unitPrefab);
            }
        }

        protected void ResetBattle()
        {
            Time.timeScale = 1;
            m_GameState = GameState.Playing;
            m_IsBattleStarted = false;
            m_PlayerUnits.Clear();
            m_Enemies.Clear();
        }

        protected override void GoToBattle()
        {
            ResetBattle();
            base.GoToBattle();
        }

        public override void ShowTextPopup(string text, Vector3 position, Color color)
        {
            m_TextPopupController.Spawn(text, position, color);
        }

        public override void RegisterUnit(Unit unit)
        {
            if (unit.IsPlayer) m_PlayerUnits.Add(unit);
            else m_Enemies.Add(unit);
        }

        public override void UnregisterUnit(Unit unit)
        {
            if (unit.IsPlayer) m_PlayerUnits.Remove(unit);
            else m_Enemies.Remove(unit);
        }
    }
}
