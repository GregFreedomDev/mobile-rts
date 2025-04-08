using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro;

    public class BattleController : MonoBehaviour
    {
        [Header("Battle Settings")]
        [SerializeField] private int maxEnemiesPerWave = 5;
        [SerializeField] private float timeBetweenSpawns = 1f;
        private GameObject[] enemyPrefabs;

        [Header("References")]
        [SerializeField] private BattleArena battleArena;
        [SerializeField] private Button startButton;
        [SerializeField] private TextMeshProUGUI startButtonText;

        private GameManager gameManager;
        private float spawnTimer;
        private int currentWaveSize;
        private List<Unit> enemyUnits = new List<Unit>();
        private bool battleStarted;
        public bool BattleStarted => battleStarted;

        private void Start()
        {
            gameManager = GameManager.Get();
            if (battleArena == null)
            {
                battleArena = FindObjectOfType<BattleArena>();
            }

            // Configurar el botón de inicio
            if (startButton == null)
            {
                startButton = GameObject.Find("BattleStartButton")?.GetComponent<Button>();
            }
            if (startButtonText == null && startButton != null)
            {
                startButtonText = startButton.GetComponentInChildren<TextMeshProUGUI>();
            }
            if (startButton != null)
            {
                startButton.onClick.AddListener(StartBattle);
                if (startButtonText != null)
                {
                    startButtonText.text = "Iniciar Batalla";
                }
            }

            // Cargar todos los prefabs de unidades y filtrar los que tienen EnemyUnit
            GameObject[] allUnitPrefabs = Resources.LoadAll<GameObject>("Prefabs/Units");
            List<GameObject> enemyPrefabsList = new List<GameObject>();

            foreach (GameObject prefab in allUnitPrefabs)
            {
                if (prefab.GetComponent<EnemyUnit>() != null)
                {
                    enemyPrefabsList.Add(prefab);
                }
            }

            enemyPrefabs = enemyPrefabsList.ToArray();

            if (enemyPrefabs == null || enemyPrefabs.Length == 0)
            {
                Debug.LogError("No se encontraron prefabs con el componente EnemyUnit en Resources/Prefabs/Units!");
            }
            else
            {
                Debug.Log($"Se encontraron {enemyPrefabs.Length} prefabs de enemigos");
            }
        }

        public void StartBattle()
        {
            if (battleStarted) return;
            
            // Reproducir sonido de clic
            AudioManager.Get().PlayBtnClick();
            
            battleStarted = true;
            currentWaveSize = 0;
            spawnTimer = timeBetweenSpawns;
            
            // Actualizar el botón
            if (startButton != null)
            {
                startButton.interactable = false;
                if (startButtonText != null)
                {
                    startButtonText.text = "Batalla en Curso";
                }
            }
            
        }

        private void OnDisable()
        {
            if (startButton != null)
            {
                startButton.onClick.RemoveListener(StartBattle);
            }
        }
    }
