using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
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
        protected GameState m_GameState = GameState.Playing;

        public int Gold => m_Gold;
        public int Wood => m_Wood;

        public void Start()
        {
            m_GameOverLayout.OnBackClicked += GoToVillage;
            m_GameOverLayout.OnRetryClicked += GoToBattle;
            m_GameOverLayout.OnContinueClicked += ContinueNextLevel;
        }

        //update this method, actualmente estan eligiendo siempre el mas cercano. Pero deberia elegir el mas cercano que no está siendo atacado. Hay que pensarlo bien,
        //a veces conviene que vaya a atacar a los que están mas atras y que no
        //no tienen atacante asignado
        public Unit FindClosestUntargetedUnit(Vector3 originPosition, bool isPlayer)
        {
            IEnumerable<Unit> units = isPlayer ? GetAllPlayerUnits() : m_Enemies;
            Unit selectedUnit = null;
            float closestDistanceSqr = float.MaxValue;

            foreach (Unit unit in units)
            {
                Debug.Log($"🔍 Analizando unidad {unit.name}, estado: {unit.CurrentState}, posición: {unit.transform.position}");
                Debug.Log($"🧠 Buscando enemigos... Total encontrados: {(isPlayer ? GetAllPlayerUnits().ToList().Count : m_Enemies.Count)}");

                if (unit.CurrentState == UnitState.Dead) continue;

                float sqrDistance = (unit.transform.position - originPosition).sqrMagnitude;

                bool isBetterCandidate = false;

                if (unit.CurrentAttackers == 0)
                {
                    isBetterCandidate = true;
                }
                else if (selectedUnit != null && selectedUnit.CurrentAttackers > 0 && sqrDistance < closestDistanceSqr)
                {
                    isBetterCandidate = true;
                }

                if (isBetterCandidate)
                {
                    selectedUnit = unit;
                    closestDistanceSqr = sqrDistance;
                }
            }

            return selectedUnit;
        }


        public IEnumerable<Unit> GetAllPlayerUnits()
        {
            return m_PlayerUnits;
        }
        
        public IEnumerable<Unit> GetAllEnemiesUnits()
        {
            return m_Enemies;
        }

        public IEnumerable<Unit> GetAllUnits()
        {
            return m_PlayerUnits.Concat(m_Enemies);
        }
        
        public List<Unit> GetFriendlyUnits(bool isPlayer)
        {
            return isPlayer ? m_PlayerUnits : m_Enemies;
        }
        
        public virtual void AddResources(int gold, int wood)
        {
            m_Gold += gold;
            m_Wood += wood;
        }
        
        
        public void HandleGameOver(bool isVictory)
        {
            if (isVictory)
            {
                AudioManager.Get().PlayMusic(m_WinAudioSettings);
                m_GameOverLayout.ShowVictory(m_Gold, 3, new List<Reward> {new Reward("Gold", 100), new Reward("Exp", 100)}, 5);
            }
            else
            {
                m_GameOverLayout.ShowDefeat(m_Gold);
                AudioManager.Get().PlayMusic(m_LoseAudioSettings);
            }

            Time.timeScale = 0;
            m_GameState = GameState.Paused;
        }
        
        void GoToVillage()
        {
            Debug.Log("GoToVillage");
            SceneManager.LoadScene("PlayScene");
        }

        void ContinueNextLevel()
        {
            Debug.Log("ContinueNextLevel");

        }
        
        
        protected virtual void GoToBattle()
        {
            Debug.Log("Battle Scene");
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
            m_GameOverLayout.OnBackClicked -= GoToVillage;
            m_GameOverLayout.OnRetryClicked -= GoToBattle;
            m_GameOverLayout.OnContinueClicked -= ContinueNextLevel;
        }
    }
}