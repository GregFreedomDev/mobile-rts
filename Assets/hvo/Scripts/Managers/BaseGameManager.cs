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
        protected int m_Gold = 0;
        protected int m_Wood = 0;
        private GameState m_GameState = GameState.Playing;

        public int Gold => m_Gold;
        public int Wood => m_Wood;

        private void Start()
        {
            m_GameOverLayout.OnQuitClicked += GoToMenu;
        }

        public Unit FindClosestUnit(Vector3 originPosition, float maxDistance, bool isPlayer)
        {
            IEnumerable<Unit> units = isPlayer ? GetAllPlayerUnits() : m_Enemies;
            float sqrMaxDistance = maxDistance * maxDistance;
            Unit closestUnit = null;
            float closestDistanceSqr = float.MaxValue;

            foreach (Unit unit in units)
            {
                if (unit.CurrentState == UnitState.Dead) continue;

                float sqrDistance = (unit.transform.position - originPosition).sqrMagnitude;
                if (sqrDistance < closestDistanceSqr)
                {
                    closestUnit = unit;
                    closestDistanceSqr = sqrDistance;
                }
            }

            return closestUnit;
        }
        
        IEnumerable<Unit> GetAllPlayerUnits()
        {
            return m_PlayerUnits;
        }

        public IEnumerable<Unit> GetAllUnits()
        {
            return m_PlayerUnits.Concat(m_Enemies);
        }
        
        public List<Unit> GetFriendlyUnits(bool isPlayer)
        {
            return isPlayer ? m_PlayerUnits : m_Enemies;
        }
        
        public void AddResources(int gold, int wood)
        {
            m_Gold += gold;
            m_Wood += wood;
        }
        
        
        public void HandleGameOver(bool isVictory)
        {
            if (isVictory)
            {
                AudioManager.Get().PlayMusic(m_WinAudioSettings);
            }
            else
            {
                AudioManager.Get().PlayMusic(m_LoseAudioSettings);
            }

            Time.timeScale = 0;
            m_GameOverLayout.ShowGameOver(isVictory);
            m_GameState = GameState.Paused;
        }
        
        void GoToMenu()
        {
            SceneManager.LoadScene("MenuScene");
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
            m_GameOverLayout.OnQuitClicked -= GoToMenu;
        }
    }
}