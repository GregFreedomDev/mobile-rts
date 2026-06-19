using System.Linq;
using hvo.Scripts.Managers;
using hvo.Scripts.Units;
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
        private BattleGameManager m_BattleManager;

        void Start()
        {
            m_CanvasGroup = GetComponent<CanvasGroup>();
            m_ButtonText = GetComponentInChildren<TextMeshProUGUI>();
            m_BattleManager = BaseGameManager.Get() as BattleGameManager;
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
            if (m_UnitPrefab == null || m_BattleManager == null) return;
            if (m_BattleManager.IsBattleStarted) return;
            if (!CanDeployMore()) return;

            m_IsDragging = true;
            m_CanvasGroup.alpha = 0.6f;

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

            Vector3Int cellPos = m_BattleManager.BattleGrid.Tilemap.WorldToCell(worldPos);
            m_BattleManager.BattleGrid.HighlightCell(cellPos, !m_IsEnemyUnit);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!m_IsDragging) return;

            m_IsDragging = false;
            m_CanvasGroup.alpha = 1f;

            if (m_PreviewObject != null)
            {
                Destroy(m_PreviewObject);
                m_PreviewObject = null;
            }

            m_BattleManager.BattleGrid.ClearHighlight();

            if (!CanDeployMore()) return;

            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldPos.z = 0;
            Vector3Int cellPos = m_BattleManager.BattleGrid.Tilemap.WorldToCell(worldPos);

            if (m_BattleManager.BattleGrid.CanPlaceUnit(cellPos, isPlayer: !m_IsEnemyUnit))
            {
                Vector3 snapped = m_BattleManager.BattleGrid.Tilemap.GetCellCenterWorld(cellPos);
                GameObject newUnit = Instantiate(m_UnitPrefab, snapped, Quaternion.identity);
                Unit unitComp = newUnit.GetComponent<Unit>();
                unitComp.GridPosition = cellPos;
                newUnit.AddComponent<UnitDragger>();
                m_BattleManager.BattleGrid.RegisterUnit(cellPos, unitComp);
            }
        }

        private bool CanDeployMore()
        {
            if (m_BattleManager == null || m_IsEnemyUnit) return true;
            return m_BattleManager.GetAllPlayerUnits().Count() < m_BattleManager.MaxPlayerUnits;
        }
    }
}
