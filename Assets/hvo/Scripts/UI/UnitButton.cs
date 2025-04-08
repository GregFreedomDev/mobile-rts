using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace HvO.UI
{
    public class UnitButton : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private GameObject unitPrefab;
        private GameManager gameManager;
        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private Image buttonImage;
        private TextMeshProUGUI buttonText;
        private BattleArena battleArena;
        private bool isEnemyUnit;

        // Vista previa durante el arrastre
        private GameObject previewObject;
        private SpriteRenderer previewRenderer;
        private bool isDragging;
        private bool isPointerOver;
        private bool isValidPosition;

        void Start()
        {
            gameManager = GameManager.Get();
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            buttonImage = GetComponent<Image>();
            buttonText = GetComponentInChildren<TextMeshProUGUI>();
            battleArena = FindObjectOfType<BattleArena>();
        }

        public void Initialize(GameObject unitPrefab)
        {
            if (unitPrefab == null)
            {
                Debug.LogError("Unit prefab is null!");
                return;
            }

            this.unitPrefab = unitPrefab;
            isEnemyUnit = unitPrefab.GetComponent<EnemyUnit>() != null;

            // Configurar el texto del botón
            buttonText = GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = unitPrefab.name;
            }

            // Configurar la imagen del botón
            Transform imageContainer = transform.Find("ImageContainer");
            if (imageContainer != null)
            {
                Image unitImage = imageContainer.GetComponentInChildren<Image>();
                if (unitImage != null)
                {
                    SpriteRenderer spriteRenderer = unitPrefab.GetComponent<SpriteRenderer>();
                    if (spriteRenderer != null && spriteRenderer.sprite != null)
                    {
                        unitImage.sprite = spriteRenderer.sprite;
                        
                        // Voltear la imagen si es una unidad enemiga
                        if (isEnemyUnit)
                        {
                            RectTransform imageRect = unitImage.GetComponent<RectTransform>();
                            imageRect.localScale = new Vector3(-1, 1, 1);
                        }
                    }
                }
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isPointerOver = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isPointerOver = false;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (unitPrefab == null) return;

            isDragging = true;
            canvasGroup.alpha = 0.6f;

            if (gameManager != null && gameManager.CameraController != null)
            {
                gameManager.CameraController.SetDragging(true);
            }

            // Crear preview
            previewObject = new GameObject("UnitPreview");
            previewRenderer = previewObject.AddComponent<SpriteRenderer>();
            SpriteRenderer originalRenderer = unitPrefab.GetComponent<SpriteRenderer>();
            if (originalRenderer != null && originalRenderer.sprite != null)
            {
                previewRenderer.sprite = originalRenderer.sprite;
                previewRenderer.color = new Color(1f, 1f, 1f, 0.5f);
                previewRenderer.sortingOrder = 100;

                // Orientar el preview según si es enemigo o no
                previewObject.transform.localScale = new Vector3(isEnemyUnit ? -1 : 1, 1, 1);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging || previewObject == null) return;

            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldPosition.z = 0;
            previewObject.transform.position = worldPosition;

            if (battleArena != null)
            {
                Vector2Int gridPosition = battleArena.GetGridPosition(worldPosition);
                isValidPosition = battleArena.IsValidPosition(gridPosition, isEnemyUnit);
                previewRenderer.color = new Color(1f, 1f, 1f, isValidPosition ? 0.7f : 0.3f);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isDragging) return;

            isDragging = false;
            canvasGroup.alpha = 1f;

            if (gameManager != null && gameManager.CameraController != null)
            {
                gameManager.CameraController.SetDragging(false);
            }

            if (previewObject != null)
            {
                Destroy(previewObject);
            }

            if (isValidPosition && battleArena != null)
            {
                Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                worldPosition.z = 0;
                Vector2Int gridPosition = battleArena.GetGridPosition(worldPosition);

                Unit unit = unitPrefab.GetComponent<Unit>();
                if (unit != null)
                {
                    if (unit is StructureUnit structureUnit)
                    {
                        BuildActionSO buildAction = Resources.Load<BuildActionSO>($"Scriptable/Units/Build{unitPrefab.name}Action");
                        if (buildAction != null)
                        {
                            if (!gameManager.TryDeductResources(buildAction.GoldCost, buildAction.WoodCost))
                            {
                                Debug.Log("No hay suficientes recursos!");
                                return;
                            }
                        }
                    }

                    GameObject newUnit = Instantiate(unitPrefab);
                    if (battleArena.PlaceUnit(newUnit.GetComponent<Unit>(), gridPosition))
                    {
                        // Orientar la unidad según si es enemigo o no
                        newUnit.transform.localScale = new Vector3(isEnemyUnit ? -1 : 1, 1, 1);
                        gameManager.RegisterUnit(newUnit.GetComponent<Unit>());
                    }
                    else
                    {
                        Destroy(newUnit);
                    }
                }
            }
        }

        void OnDestroy()
        {
            if (isDragging && gameManager != null && gameManager.CameraController != null)
            {
                gameManager.CameraController.SetDragging(false);
            }
        }
    }
} 