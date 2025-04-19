using hvo.Scripts.Managers;
using hvo.Scripts.Units;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace HvO.UI
{
    public class UnitElementBar : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler,
        IPointerExitHandler
    {
        private GameObject unitPrefab;
        private CanvasGroup canvasGroup;
        private TextMeshProUGUI buttonText;
        private bool isEnemyUnit;

        // Vista previa durante el arrastre
        private GameObject previewObject;
        private SpriteRenderer previewRenderer;
        private bool isDragging;
        private bool isPointerOver;
        private bool isValidPosition;
        private BattleGameManager battleGameManager;

        void Start()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            buttonText = GetComponentInChildren<TextMeshProUGUI>();
            battleGameManager = BaseGameManager.Get() as BattleGameManager;
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

            Debug.unityLogger.Log("OnBeginDrag");
            isDragging = true;
            canvasGroup.alpha = 0.6f;

            /*  if (gameManager != null && gameManager.CameraController != null)
              {
                  gameManager.CameraController.SetDragging(true);
              }
  */
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
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isDragging) return;

            isDragging = false;
            canvasGroup.alpha = 1f;
            
            if (previewObject != null)
            {
                Destroy(previewObject);
            }

            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldPosition.z = 0;
            
            Vector3Int cellPos = battleGameManager.BattleGrid.Tilemap.WorldToCell(worldPosition);

            if (battleGameManager.BattleGrid.CanPlaceUnit(cellPos, unitPrefab.GetComponent<Unit>()))
            {
                GameObject newUnit = Instantiate(unitPrefab, worldPosition, Quaternion.identity);
                newUnit.transform.position = battleGameManager.BattleGrid.Tilemap.GetCellCenterWorld(cellPos);
                var unitComp = newUnit.GetComponent<Unit>();
                unitComp.GridPosition = cellPos;
                newUnit.AddComponent<UnitDragger>();
                battleGameManager.BattleGrid.RegisterUnit(cellPos, newUnit.GetComponent<Unit>());
            }
            

            /*     if (gameManager != null && gameManager.CameraController != null)
                 {
                     gameManager.CameraController.SetDragging(false);
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
        }*/
        }
    }
}