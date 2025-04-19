using hvo.Scripts.Managers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace hvo.Scripts.Units
{
    public class UnitDragger : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private Camera mainCam;
        private Vector3Int originalGridPos;
        private Vector3 originalWorldPos;
        private BattleGameManager battleGameManager;
        private Unit unit;
        private GameObject preview;

        void Start()
        {
            mainCam = Camera.main;
            battleGameManager = BaseGameManager.Get() as BattleGameManager;
            unit = GetComponent<Unit>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if(battleGameManager == null || battleGameManager.IsBattleStarted) return;
            
            originalWorldPos = transform.position;
            originalGridPos = unit.GridPosition;

            // Crear vista previa transparente
            preview = new GameObject("UnitDragPreview");
            var spriteRenderer = preview.AddComponent<SpriteRenderer>();
            var unitSprite = GetComponent<SpriteRenderer>();

            spriteRenderer.sprite = unitSprite.sprite;
            spriteRenderer.color = new Color(1f, 1f, 1f, 0.5f);
            spriteRenderer.sortingOrder = 100;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (preview == null) return;
            Vector3 worldPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
            worldPos.z = 0;
            preview.transform.position = worldPos;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (preview != null) Destroy(preview);

            Vector3 worldPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
            worldPos.z = 0;
            Vector3Int newGridPos = battleGameManager.BattleGrid.Tilemap.WorldToCell(worldPos);

            if (battleGameManager.BattleGrid.CanPlaceUnit(newGridPos, unit))
            {
                // Actualiza la grilla
                battleGameManager.BattleGrid.RegisterUnit(originalGridPos, null);
                battleGameManager.BattleGrid.RegisterUnit(newGridPos, unit);

                // Mueve la unidad
                transform.position = battleGameManager.BattleGrid.Tilemap.GetCellCenterWorld(newGridPos);
                unit.GridPosition = newGridPos;
            }
            else
            {
                // Revertir posición si no es válida
                transform.position = originalWorldPos;
            }
        }
    }
}
