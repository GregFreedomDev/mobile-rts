using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

    public class UnitDragAndDrop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private GameObject unitPrefab;
        private GameObject draggedUnit;
        private Canvas canvas;
        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private GameManager gameManager;

        private void Awake()
        {
            canvas = GetComponentInParent<Canvas>();
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            gameManager = GameManager.Get();
        }

        public void SetUnitPrefab(GameObject prefab)
        {
            unitPrefab = prefab;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            canvasGroup.alpha = 0.6f;
            canvasGroup.blocksRaycasts = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;

            // Obtener la posición del mouse en el mundo
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldPosition.z = 0;

            // Verificar si hay suficientes recursos para crear la unidad
            Unit unit = unitPrefab.GetComponent<Unit>();
            if (unit != null)
            {
                // Verificar si la unidad tiene un costo de recursos
                if (unit is StructureUnit structureUnit)
                {
                    // Buscar el BuildActionSO asociado a esta estructura
                    BuildActionSO buildAction = Resources.Load<BuildActionSO>($"Scriptable/Units/Build{unitPrefab.name}Action");
                    if (buildAction != null)
                    {
                        if (!gameManager.TryDeductResources(buildAction.GoldCost, buildAction.WoodCost))
                        {
                            Debug.Log("No hay suficientes recursos!");
                            rectTransform.anchoredPosition = Vector2.zero;
                            return;
                        }
                    }
                }

                // Instanciar la unidad en la posición del mouse
                GameObject newUnit = Instantiate(unitPrefab, worldPosition, Quaternion.identity);
                gameManager.RegisterUnit(newUnit.GetComponent<Unit>());
            }

            // Resetear la posición del botón
            rectTransform.anchoredPosition = Vector2.zero;
        }
    }
