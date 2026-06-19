using System.Linq;
using hvo.Scripts.Managers;
using hvo.Scripts.Units;
using hvo.Scripts.Utils;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace HvO.UI
{
    public class UnitElementBar : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler,
        IPointerEnterHandler, IPointerExitHandler
    {
        private GameObject m_UnitPrefab;
        private CanvasGroup m_CanvasGroup;
        private TextMeshProUGUI m_ButtonText;
        private bool m_IsEnemyUnit;
        private bool m_IsDragging;
        private GameObject m_PreviewObject;

        // Lazy resolution — resolve the concrete BattleGameManager directly.
        // (BaseGameManager.Get() casts unreliably because its fallback can't AddComponent
        //  the abstract BaseGameManager type, leaving a junk object that breaks lookups.)
        private BattleGameManager m_BattleManager;
        private BattleGameManager BattleManager
        {
            get
            {
                if (m_BattleManager == null)
                    m_BattleManager = Object.FindFirstObjectByType<BattleGameManager>();
                return m_BattleManager;
            }
        }

        void Start()
        {
            m_CanvasGroup = GetComponent<CanvasGroup>();
            m_ButtonText = GetComponentInChildren<TextMeshProUGUI>();
        }

        public void Initialize(GameObject unitPrefab)
        {
            if (unitPrefab == null) return;

            m_UnitPrefab = unitPrefab;
            m_IsEnemyUnit = unitPrefab.GetComponent<EnemyUnit>() != null;

            if (m_ButtonText != null)
                m_ButtonText.text = unitPrefab.name;

            Transform imageContainer = transform.Find("ImageContainer");
            if (imageContainer != null)
            {
                Image unitImage = imageContainer.GetComponentInChildren<Image>();
                SpriteRenderer sr = unitPrefab.GetComponent<SpriteRenderer>();
                if (unitImage != null && sr != null && sr.sprite != null)
                {
                    unitImage.sprite = sr.sprite;
                    if (m_IsEnemyUnit)
                        unitImage.GetComponent<RectTransform>().localScale = new Vector3(-1, 1, 1);
                }
            }
        }

        public void OnPointerEnter(PointerEventData eventData) { }
        public void OnPointerExit(PointerEventData eventData) { }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (m_UnitPrefab == null) return;
            if (BattleManager != null && BattleManager.IsBattleStarted) return;
            if (!CanDeployMore()) return;

            m_IsDragging = true;
            DragState.IsDraggingUnit = true; // suppress camera panning while placing a unit
            if (m_CanvasGroup != null) m_CanvasGroup.alpha = 0.6f;

            m_PreviewObject = new GameObject("UnitPreview");
            SpriteRenderer previewRenderer = m_PreviewObject.AddComponent<SpriteRenderer>();
            SpriteRenderer originalRenderer = m_UnitPrefab.GetComponent<SpriteRenderer>();
            if (originalRenderer != null && originalRenderer.sprite != null)
            {
                previewRenderer.sprite = originalRenderer.sprite;
                previewRenderer.color = new Color(1f, 1f, 1f, 0.5f);
                previewRenderer.sortingOrder = 100;
                m_PreviewObject.transform.localScale = new Vector3(m_IsEnemyUnit ? -1 : 1, 1, 1);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!m_IsDragging || m_PreviewObject == null) return;

            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldPos.z = 0;
            m_PreviewObject.transform.position = worldPos;

            if (BattleManager?.BattleGrid != null)
            {
                Vector3Int cellPos = BattleManager.BattleGrid.Tilemap.WorldToCell(worldPos);
                BattleManager.BattleGrid.HighlightCell(cellPos, !m_IsEnemyUnit);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!m_IsDragging) return;

            m_IsDragging = false;
            DragState.IsDraggingUnit = false;
            if (m_CanvasGroup != null) m_CanvasGroup.alpha = 1f;

            if (m_PreviewObject != null)
            {
                Destroy(m_PreviewObject);
                m_PreviewObject = null;
            }

            BattleManager?.BattleGrid?.ClearHighlight();

            if (BattleManager?.BattleGrid == null) return;
            if (!CanDeployMore()) return;

            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldPos.z = 0;
            Vector3Int cellPos = BattleManager.BattleGrid.Tilemap.WorldToCell(worldPos);

            if (BattleManager.BattleGrid.CanPlaceUnit(cellPos, isPlayer: !m_IsEnemyUnit))
            {
                Vector3 snapped = BattleManager.BattleGrid.Tilemap.GetCellCenterWorld(cellPos);
                GameObject newUnit = Instantiate(m_UnitPrefab, snapped, Quaternion.identity);
                Unit unitComp = newUnit.GetComponent<Unit>();
                unitComp.GridPosition = cellPos;
                newUnit.AddComponent<UnitDragger>();
                BattleManager.BattleGrid.RegisterUnit(cellPos, unitComp);
            }
        }

        private bool CanDeployMore()
        {
            if (BattleManager == null || m_IsEnemyUnit) return true;
            return BattleManager.GetAllPlayerUnits().Count() < BattleManager.MaxPlayerUnits;
        }
    }
}
