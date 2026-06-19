using hvo.Scripts.Managers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace hvo.Scripts.Units
{
    public class UnitDragger : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private RectTransform m_UnitPanelRect;

        private Camera m_MainCam;
        private Vector3Int m_OriginalGridPos;
        private Vector3 m_OriginalWorldPos;
        private BattleGameManager m_BattleManager;
        private Unit m_Unit;
        private GameObject m_Preview;

        void Start()
        {
            m_MainCam = Camera.main;
            m_BattleManager = BaseGameManager.Get() as BattleGameManager;
            m_Unit = GetComponent<Unit>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (m_BattleManager == null || m_BattleManager.IsBattleStarted) return;

            m_OriginalWorldPos = transform.position;
            m_OriginalGridPos = m_Unit.GridPosition;

            m_Preview = new GameObject("UnitDragPreview");
            SpriteRenderer sr = m_Preview.AddComponent<SpriteRenderer>();
            SpriteRenderer unitSr = GetComponent<SpriteRenderer>();
            sr.sprite = unitSr.sprite;
            sr.color = new Color(1f, 1f, 1f, 0.5f);
            sr.sortingOrder = 100;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (m_Preview == null) return;

            Vector3 worldPos = m_MainCam.ScreenToWorldPoint(Input.mousePosition);
            worldPos.z = 0;
            m_Preview.transform.position = worldPos;

            Vector3Int cellPos = m_BattleManager.BattleGrid.Tilemap.WorldToCell(worldPos);
            m_BattleManager.BattleGrid.HighlightCell(cellPos, m_Unit.IsPlayer);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (m_Preview != null)
            {
                Destroy(m_Preview);
                m_Preview = null;
            }

            m_BattleManager.BattleGrid.ClearHighlight();

            Vector3 worldPos = m_MainCam.ScreenToWorldPoint(Input.mousePosition);
            worldPos.z = 0;

            if (IsOverPanel(eventData))
            {
                m_BattleManager.BattleGrid.RegisterUnit(m_OriginalGridPos, null);
                Destroy(gameObject);
                return;
            }

            Vector3Int newGridPos = m_BattleManager.BattleGrid.Tilemap.WorldToCell(worldPos);

            if (m_BattleManager.BattleGrid.CanPlaceUnit(newGridPos, m_Unit))
            {
                m_BattleManager.BattleGrid.RegisterUnit(m_OriginalGridPos, null);
                m_BattleManager.BattleGrid.RegisterUnit(newGridPos, m_Unit);
                transform.position = m_BattleManager.BattleGrid.Tilemap.GetCellCenterWorld(newGridPos);
                m_Unit.GridPosition = newGridPos;
            }
            else
            {
                transform.position = m_OriginalWorldPos;
            }
        }

        private bool IsOverPanel(PointerEventData eventData)
        {
            if (m_UnitPanelRect == null)
            {
                // Fallback: check if pointer is over any UI element
                return eventData.pointerCurrentRaycast.gameObject != null &&
                       eventData.pointerCurrentRaycast.gameObject.layer == LayerMask.NameToLayer("UI");
            }

            Canvas canvas = m_UnitPanelRect.GetComponentInParent<Canvas>();
            return RectTransformUtility.RectangleContainsScreenPoint(
                m_UnitPanelRect, Input.mousePosition, canvas?.worldCamera);
        }
    }
}
