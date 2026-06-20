using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace hvo.Scripts.Managers
{
    public abstract class BaseGameManager: SingletonManager<BaseGameManager>, IRegisterUnit, IRegisterResource
    {
        
        [SerializeField] private AudioSettings m_WinAudioSettings;
        [SerializeField] private AudioSettings m_LoseAudioSettings;
        [SerializeField] private GameOverLayout m_GameOverLayout;

        protected List<Unit> m_PlayerUnits = new();
        protected List<Unit> m_Enemies = new();
        protected List<StructureUnit> m_PlayerBuildings = new();
        protected readonly ResourceManager m_Resources = new();
        protected readonly PopulationManager m_Population = new();
        protected GameState m_GameState = GameState.Playing;
        protected Player m_Player;
        public Player Player => m_Player;

        // Named ResourceBank (not "Resources") so it never shadows UnityEngine.Resources in subclasses.
        public ResourceManager ResourceBank => m_Resources;
        public int Gold => m_Resources.GetAmount(ResourceType.Gold);
        public int Wood => m_Resources.GetAmount(ResourceType.Wood);

        public PopulationManager Population => m_Population;
        // Current population = living villagers; derived from the registered units so it can't desync.
        public int CurrentPopulation => m_PlayerUnits.Count(u => u != null && u is WorkerUnit);
        public bool HasPopulationSpace => CurrentPopulation < m_Population.MaxPopulation;

        public void Awake()
        {
            m_Player = new Player("Player1");
            m_GameOverLayout.OnBackClicked += GoToVillage;
            m_GameOverLayout.OnRetryClicked += GoToBattle;
            m_GameOverLayout.OnContinueClicked += ContinueNextLevel;
        }

        //Si llevo un rato intentando llegar y no puedo, entonces seleccionar
        //un enemigo más cercano incluso si ya tiene atancates
        public Unit FindClosestUntargetedUnit(Vector3 originPosition, bool isPlayer)
        {
            IEnumerable<Unit> units = isPlayer ? GetAllPlayerUnits() : GetAllEnemiesUnits();
            Unit selectedUnit = null;
            float closestDistanceSqr = float.MaxValue;

            foreach (Unit unit in units)
            {
                if (unit.CurrentState == UnitState.Dead) continue;

                float sqrDistance = (unit.transform.position - originPosition).sqrMagnitude;

                // Siempre elegimos el más cercano, con preferencia por los que tienen menos atacantes
                if (selectedUnit == null ||
                    unit.CurrentAttackers < selectedUnit.CurrentAttackers ||
                    (unit.CurrentAttackers == selectedUnit.CurrentAttackers && sqrDistance < closestDistanceSqr))
                {
                    selectedUnit = unit;
                    closestDistanceSqr = sqrDistance;
                }
            }

            return selectedUnit;
        }
        
        public IEnumerable<Unit> GetAllPlayerUnits()
        {
            return m_PlayerUnits.Where(it => it != null);
        }
        
        public IEnumerable<Unit> GetAllEnemiesUnits()
        {
            return m_Enemies.Where(it => it != null);;
        }

        public IEnumerable<Unit> GetAllUnits()
        {
            return m_PlayerUnits.Concat(m_Enemies).Where(it => it != null);
        }
        
        public List<Unit> GetFriendlyUnits(bool isPlayer)
        {
            return isPlayer ? m_PlayerUnits : m_Enemies;
        }
        
        public virtual void AddResources(int gold, int wood)
        {
            m_Resources.Add(ResourceType.Gold, gold);
            m_Resources.Add(ResourceType.Wood, wood);
        }

        // Generic resource API (Food, Iron, Wheat, ... used by Features 3+).
        public int GetResource(ResourceType type) => m_Resources.GetAmount(type);
        public virtual void AddResource(ResourceType type, int amount) => m_Resources.Add(type, amount);
        public bool HasResource(ResourceType type, int amount) => m_Resources.Has(type, amount);
        public bool SpendResource(ResourceType type, int amount) => m_Resources.TrySpend(type, amount);
        
        
        public void HandleGameOver(bool isVictory, int stars = 3)
        {
            if (isVictory)
            {
                AudioManager.Get().PlayMusic(m_WinAudioSettings);
                m_GameOverLayout.ShowVictory(Gold, stars, new List<Reward> { new Reward("Gold", 100), new Reward("Exp", 100) }, 5);
            }
            else
            {
                m_GameOverLayout.ShowDefeat(Gold);
                AudioManager.Get().PlayMusic(m_LoseAudioSettings);
            }

            Time.timeScale = 0;
            m_GameState = GameState.Paused;
        }
        
        void GoToVillage()
        {
            Time.timeScale = 1f; // HandleGameOver froze time; unfreeze before leaving the scene
            SceneManager.LoadScene("PlayScene");
        }

        void ContinueNextLevel()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("BattleScene");
        }

        protected virtual void GoToBattle()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("BattleScene");
        }

        public virtual void FocusActionUI(int index)
        { }
        
        public virtual void RegisterUnit(Unit unit)
        { }

        public virtual void UnregisterUnit(Unit unit)
        { }

        public virtual void ShowTextPopup(string text, Vector3 position, Color color)
        { }

        void OnDestroy()
        {
            if (m_GameOverLayout == null) return;
            m_GameOverLayout.OnBackClicked -= GoToVillage;
            m_GameOverLayout.OnRetryClicked -= GoToBattle;
            m_GameOverLayout.OnContinueClicked -= ContinueNextLevel;
        }
    }
}