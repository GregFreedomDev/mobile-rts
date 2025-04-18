using System;
using System.Collections.Generic;
using HvO.UI;
using UnityEngine;

namespace hvo.Scripts.Managers
{
    public class BattleGameManager : BaseGameManager, IRegisterUnit, IRegisterResource
    {
        [SerializeField] private Canvas m_MainCanvas;
        [SerializeField] private GameObject m_UnitListPanelPrefab;
        [SerializeField] private UnitElementBar m_UnitElementPrefab;


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
        }


        private void InitializePanelUnitUI()
        {
            // Instanciar el panel
            GameObject panelInstance = Instantiate(m_UnitListPanelPrefab, m_MainCanvas.transform);

            // Añadir el componente UnitListUI
            UnitListBar unitListUI = panelInstance.AddComponent<UnitListBar>();

            // Inicializar la UI con el prefab del botón
            unitListUI.Initialize(m_UnitElementPrefab);

            // Cargar y agregar las unidades disponibles
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