using System;
using System.Collections.Generic;
using System.Linq;
using hvo.Scripts.Managers;
using Unity.VisualScripting;
using UnityEngine;

public class RenderSorter : MonoBehaviour
{
    [SerializeField] private int baseOrder = 15;

    private List<Unit> allUnits = new();
    private BattleGameManager battleGameManager;
    private void Start()
    {
        battleGameManager = BaseGameManager.Get() as BattleGameManager;
    }

    void LateUpdate()
    {
        if (battleGameManager == null) return;
        allUnits.Clear();

        // Suponiendo que tienes acceso a estas listas desde tu GameManager
        allUnits = battleGameManager.GetAllUnits().ToList();

        // Ordenar por Y de mayor a menor (más bajo en pantalla primero)
        allUnits.Sort((a, b) => b.transform.position.y.CompareTo(a.transform.position.y));

        // Asignar orden incremental
        for (int i = 0; i < allUnits.Count; i++)
        {
            if (allUnits[i] != null && allUnits[i].Renderer != null)
            {
                allUnits[i].Renderer.sortingOrder = baseOrder + i;
            }
        }
    }
}