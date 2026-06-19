using System;
using System.Collections.Generic;
using UnityEngine;

namespace hvo.Scripts.Spawning
{
    [CreateAssetMenu(fileName = "EnemyFormation", menuName = "HvO/Enemy Formation")]
    public class EnemyFormationSO : ScriptableObject
    {
        [Serializable]
        public class EnemyEntry
        {
            public GameObject prefab;
            public int column; // 7–13 (enemy side)
            public int row;    // 0–4
        }

        public List<EnemyEntry> enemies = new();
    }
}
