
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using hvo.Scripts.Managers;
using HvO.UI;
using UnityEngine.UI;

public enum ClickType
{
    Move, Attack, Build, Chop
}

public enum GameState
{
    Playing, Paused
}

public class GameManager : BaseGameManager
{
    [Header("UI")]
    [SerializeField] private PointToClick m_PointToMovePrefab;
    [SerializeField] private PointToClick m_PointToBuildPrefab;
    [SerializeField] private PointToClick m_PointToAttackPrefab;
    [SerializeField] private PointToClick m_PointToChopPrefab;
    [SerializeField] private ActionBar m_ActionBar;
    [SerializeField] private ConfirmationBar m_BuildConfirmationBar;
    [SerializeField] private TextPopupController m_TextPopupController;
    [SerializeField] private ResourceDataUI m_ResourceDataUI;
    [SerializeField] private Button m_buttonBattle;
    [SerializeField] private FoodPanel m_FoodPanel;

    [Header("Camera Settings")]
    [SerializeField] private float m_PanSpeed = 100;
    [SerializeField] private float m_MobilePanSpeed = 10;

    [Header("VFX")]
    [SerializeField] private ParticleSystem m_ConstructionEffectPrefab;

    [Header("Resources")]
    [SerializeField] private Transform m_TreeContainer;
    [SerializeField] private GoldMine m_ActiveGoldMine;
    
    [Header("Audio")]
    [SerializeField] private AudioSettings m_PlacementAudioSettings;
    [SerializeField] private AudioSettings m_BgMusicAudioSettings;


    public Unit ActiveUnit;


    private Tree[] m_Trees = new Tree[0];
    private CameraController m_CameraController;
    private PlacementProcess m_PlacementProcess;
    private BuildAssignButton m_AssignButton;
    private IWorkerAssignable m_Assignable; // what the floating "assign worker" button targets
    private int m_LastAvailableWorkers = -1; // to refresh the "free workers" UI only when it changes

    [SerializeField] private int m_BasePopulation = 4; // starting housing capacity (the castle)

    public GoldMine ActiveGoldMine => m_ActiveGoldMine;
    public bool HasActiveUnit => ActiveUnit != null;

    void Start()
    {
        base.Awake();
        Time.timeScale = 1;
        m_Resources.OnChanged += RefreshResourceUI; // refresh on any resource change, not just gold/wood
        m_Population.OnChanged += RefreshPopulationUI; // refresh when housing capacity changes
        m_Population.IncreaseMax(m_BasePopulation);
        RefreshPopulationUI();
        m_CameraController = new CameraController(m_PanSpeed, m_MobilePanSpeed);
        ClearActionBarUI();
        AddResources(500, 500);
        m_buttonBattle.onClick.AddListener(GoToBattle);
        AudioManager.Get().PlayMusic(m_BgMusicAudioSettings);
        SetupBuildMenu();
    }

    void SetupBuildMenu()
    {
        var catalog = Resources.Load<BuildCatalogSO>("BuildCatalog");
        if (catalog == null)
        {
            Debug.LogWarning("[BuildMenu] BuildCatalog not found in Resources.");
            return;
        }

        Canvas canvas = m_ActionBar != null ? m_ActionBar.GetComponentInParent<Canvas>() : FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("[BuildMenu] No Canvas found for the build menu.");
            return;
        }

        var menu = new GameObject("BuildMenu").AddComponent<BuildMenu>();
        menu.Initialize(catalog.Buildings, canvas, StartBuildProcess);

        var font = FindAnyObjectByType<TMPro.TextMeshProUGUI>()?.font;
        m_AssignButton = new GameObject("BuildAssignButton").AddComponent<BuildAssignButton>();
        m_AssignButton.Initialize(canvas, font, AssignNearestWorker);
    }

    void AssignNearestWorker(IWorkerAssignable target)
    {
        WorkerUnit best = null;
        float bestSqr = float.MaxValue;
        Vector3 targetPos = target.AnchorTransform.position;

        foreach (var unit in m_PlayerUnits)
        {
            if (unit is WorkerUnit worker && worker.IsAvailable) // only idle workers, never steal a busy one
            {
                float sqr = (worker.transform.position - targetPos).sqrMagnitude;
                if (sqr < bestSqr) { bestSqr = sqr; best = worker; }
            }
        }

        if (best != null)
            target.AssignWorker(best);
        else
            ShowTextPopup("No hay trabajadores disponibles", targetPos + Vector3.up, Color.red);
    }

    void Update()
    {
        if (m_AssignButton != null) m_AssignButton.Track(m_Assignable);

        int available = AvailableWorkers;
        if (available != m_LastAvailableWorkers)
        {
            m_LastAvailableWorkers = available;
            RefreshPopulationUI();
        }

        if (m_GameState == GameState.Paused) return;

        m_CameraController.Update();

        if (m_PlacementProcess != null)
        {
            m_PlacementProcess.Update();
        }
        else if (HvoUtils.TryGetShortClickPosition(out Vector2 inputPosition))
        {
            DetectClick(inputPosition);
        }
    }

    public override void RegisterUnit(Unit unit)
    {
        if (unit.IsPlayer)
        {
            if (unit.IsBuilding)
            {
                m_PlayerBuildings.Add(unit as StructureUnit);
            }
            else
            {
                m_PlayerUnits.Add(unit);
                RefreshPopulationUI();
            }
        }
        else
        {
            m_Enemies.Add(unit);
        }
    }

    public override void UnregisterUnit(Unit unit)
    {
        if (unit.IsPlayer)
        {
            if (ActiveUnit == unit)
            {
                if (m_PlacementProcess != null)
                {
                    CancelBuildPlacement();
                }

                ClearActionBarUI();
                ActiveUnit.Deselect();
                ActiveUnit = null;
            }

            unit.StopMovement();

            if (unit.IsBuilding)
            {
                m_PlayerBuildings.Remove(unit as StructureUnit);
            }
            else
            {
                m_PlayerUnits.Remove(unit);
                RefreshPopulationUI();
            }
        }
        else
        {
            m_Enemies.Remove(unit);
        }
    }

    public override void ShowTextPopup(string text, Vector3 position, Color color)
    {
        m_TextPopupController.Spawn(text, position, color);
    }

    public Tree FindClosestUnclaimedTree(Vector3 originPosition)
    {
        Tree closestTree = null;
        float closestDistanceSqr = float.MaxValue;

        if (m_Trees.Length == 0)
        {
            m_Trees = new Tree[m_TreeContainer.childCount];

            for (int i = 0; i < m_TreeContainer.childCount; i++)
            {
                m_Trees[i] = m_TreeContainer.GetChild(i).GetComponent<Tree>();
            }
        }

        foreach (var tree in m_Trees)
        {
            if (tree.Claimed) continue;

            float sqrDistance = (tree.transform.position - originPosition).sqrMagnitude;

            if (sqrDistance < closestDistanceSqr)
            {
                closestDistanceSqr = sqrDistance;
                closestTree = tree;
            }
        }

        return closestTree;
    }
    
    public StructureUnit FindClosestWoodStorage(Vector3 originPoint)
    {
        float closestDistacenSqr = float.MaxValue;
        StructureUnit closestUnit = null;

        foreach (StructureUnit unit in m_PlayerBuildings)
        {
            if (unit.CurrentState == UnitState.Dead || !unit.CanStoreWood) continue;

            float sqrDistance = (unit.transform.position - originPoint).sqrMagnitude;
            if (sqrDistance < closestDistacenSqr)
            {
                closestUnit = unit;
                closestDistacenSqr = sqrDistance;
            }
        }

        return closestUnit;
    }

    public StructureUnit FindClosestGoldStorage(Vector3 originPoint)
    {
        float closestDistacenSqr = float.MaxValue;
        StructureUnit closestUnit = null;

        foreach (StructureUnit unit in m_PlayerBuildings)
        {
            if (unit.CurrentState == UnitState.Dead || !unit.CanStoreGold) continue;

            float sqrDistance = (unit.transform.position - originPoint).sqrMagnitude;
            if (sqrDistance < closestDistacenSqr)
            {
                closestUnit = unit;
                closestDistacenSqr = sqrDistance;
            }
        }

        return closestUnit;
    }

    public void StartBuildProcess(BuildActionSO buildAction)
    {
        if (m_PlacementProcess != null) return;
        var tilemapManager = TilemapManager.Get();
        m_PlacementProcess = new PlacementProcess(
            buildAction,
            tilemapManager
        );
        m_PlacementProcess.ShowPlacementOutline();
        m_BuildConfirmationBar.Show(buildAction.GoldCost, buildAction.WoodCost);
        m_BuildConfirmationBar.SetupHooks(ConfirmBuildPlacement, CancelBuildPlacement);
        m_CameraController.LockCamera = true;
    }

    public void StartUnitTrainProcess(TrainUnitActionSO trainUnitAction)
    {
        m_BuildConfirmationBar.Show(
            trainUnitAction.GoldCost,
            trainUnitAction.WoodCost
        );

        m_BuildConfirmationBar.SetupHooks(
            () => FinalizeUnitTraining(trainUnitAction),
            CancelUnitTrainingConfirmation
        );
    }

    void FinalizeUnitTraining(TrainUnitActionSO trainUnitAction)
    {
        AudioManager.Get().PlayBtnClick();

        if (!TryDeductResources(trainUnitAction.GoldCost, trainUnitAction.WoodCost))
        {
            Debug.Log("Not Enough Resources!");
            return;
        }

        Collider2D castleCollider = ActiveUnit.Collider;
        float spawnRadius = castleCollider.bounds.extents.x;
        int maxSpawnAttemps = 20;

        for (int i = 0; i < maxSpawnAttemps; i++)
        {
            float angle = (360f / maxSpawnAttemps) * i;
            Vector2 spawnOffset = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * spawnRadius;
            Vector2 spawnPosition = (Vector2)ActiveUnit.transform.position + spawnOffset;

            var spawnPositionOnCollider = castleCollider.ClosestPoint(spawnPosition);
            var opositeDirection = (spawnPositionOnCollider - (Vector2)castleCollider.transform.position).normalized;
            spawnPositionOnCollider += opositeDirection * 0.3f;

            var unitLayerMask = LayerMask.GetMask("Unit");
            var hits = Physics2D.OverlapCircleAll(spawnPositionOnCollider, 0.5f, unitLayerMask);
            bool isPositionOccupied = false;

            foreach (var hit in hits)
            {
                if (hit != castleCollider)
                {
                    isPositionOccupied = true;
                    break;
                }
            }

            if (!isPositionOccupied)
            {
                Instantiate(trainUnitAction.UnitPrefab, spawnPositionOnCollider, Quaternion.identity);
                m_BuildConfirmationBar.UpdateRequirementsUI(trainUnitAction.GoldCost, trainUnitAction.WoodCost);
                return;
            }
        }

        AddResources(trainUnitAction.GoldCost, trainUnitAction.WoodCost);
    }

    void CancelUnitTrainingConfirmation()
    {
        AudioManager.Get().PlayBtnClick();
        m_BuildConfirmationBar.Hide();
    }

    void DetectClick(Vector2 inputPosition)
    {
        if (HvoUtils.IsPointerOverUIElement())
        {
            return;
        }

        Vector2 worldPoint = Camera.main.ScreenToWorldPoint(inputPosition);
        RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);

        if (HasActiveUnit && ActiveUnit is WorkerUnit worker)
        {
            // Mining still works by clicking the mine with a worker selected (no mine button yet).
            if (TryGetClickedResource(hit, out GoldMine mine))
            {
                worker.SendToMine(mine, DestinationSource.PlayerClick);
                DisplayClickEffect(mine.transform.position, ClickType.Build);
                return;
            }
        }

        // Clicking a tree selects it (shows the "Cortar leña" button); it is NOT chopped directly,
        // even with a worker selected.
        if (TryGetClickedResource(hit, out Tree clickedTree))
        {
            SelectAssignable(clickedTree);
            return;
        }

        if (HasClickedOnUnit(hit, out var unit))
        {
            if (unit.IsPlayer)
            {
                HandleClickOnPlayerUnit(unit);
            }
            else
            {
                HandleClickOnEnemy(unit);
            }
        }
        else
        {
            HandleClickOnGround(worldPoint);
        }
    }

    void SelectAssignable(IWorkerAssignable assignable)
    {
        if (HasActiveUnit) CancelActiveUnit(); // drop any unit selection (and its assignable)
        m_Assignable = assignable;
    }

    public void CancelActiveUnit()
    {
        ActiveUnit.Deselect();
        ActiveUnit = null;
        m_Assignable = null;

        ClearActionBarUI();
    }

    public void FocusActionUI(int idx)
    {
        m_ActionBar.FocusAction(idx);
    }

    bool TryGetClickedResource<T>(RaycastHit2D hit, out T resource) where T : MonoBehaviour
    {
        resource = null;
        if (hit.collider == null) return false;
        return hit.collider.TryGetComponent(out resource);
    }

    bool HasClickedOnUnit(RaycastHit2D hit, out Unit unit)
    {
        if (hit.collider != null && hit.collider.TryGetComponent<Unit>(out var clickedUnit))
        {
            unit = clickedUnit;
            return true;
        }

        unit = null;
        return false;
    }

    void HandleClickOnGround(Vector2 worldPoint)
    {
        // Clicking empty ground just clears the selection. Workers are no longer moved manually;
        // they only move via job assignments (build/tend/chop/mine).
        if (HasActiveUnit) CancelActiveUnit();
        m_Assignable = null;
    }

    void HandleClickOnPlayerUnit(Unit unit)
    {
        if (HasActiveUnit)
        {
            if (HasClickedOnActiveUnit(unit))
            {
                CancelActiveUnit();
                return;
            }
            else if (ActiveUnit is WorkerUnit worker)
            {
                // Depositing gathered resources stays. Assigning a worker to build/tend a building
                // is done via the building's "assign worker" button — not by clicking the building
                // with a worker selected (that just selects the building now).
                if (worker.IsHoldingWood && WorkerClickedOnWoodStorage(unit))
                {
                    HandleResourceReturn(worker, unit as StructureUnit);
                    return;
                }
                else if (worker.IsHoldingGold && WorkerClickedOnGoldStorage(unit))
                {
                    HandleResourceReturn(worker, unit as StructureUnit);
                    return;
                }
            }
        }

        SelectNewUnit(unit);
    }

    void HandleResourceReturn(WorkerUnit worker, StructureUnit structure)
    {
        var closestPoint = structure.Collider.ClosestPoint(worker.transform.position);
        worker.MoveTo(closestPoint, DestinationSource.PlayerClick);
        worker.SetTask(UnitTask.ReturnResource);

        if (worker.IsHoldingGold && structure.CanStoreGold)
        {
            worker.SetGoldStorage(structure);
        }
        else if (worker.IsHoldingWood && structure.CanStoreWood)
        {
            worker.SetWoodStorage(structure);
        }

        DisplayClickEffect(structure.transform.position, ClickType.Build);
    }

    bool WorkerClickedOnWoodStorage(Unit clickedUnit)
    {
        return
            (clickedUnit is StructureUnit structure)
            && structure.CanStoreWood;
    }

    bool WorkerClickedOnGoldStorage(Unit clickedUnit)
    {
        return
            (clickedUnit is StructureUnit structure)
            && structure.CanStoreGold;
    }

    void HandleClickOnEnemy(Unit enemyUnit)
    {
        if (HasActiveUnit)
        {
            ActiveUnit.SetTarget(enemyUnit, DestinationSource.PlayerClick);
            ActiveUnit.SetTask(UnitTask.Attack);
            DisplayClickEffect(enemyUnit.GetTopPosition(), ClickType.Attack);
        }
    }

    void SelectNewUnit(Unit unit)
    {
        if (unit.CurrentState == UnitState.Dead) return;

        if (HasActiveUnit)
        {
            ActiveUnit.Deselect();
        }

        ShowUnitActions(unit);
        ActiveUnit = unit;
        ActiveUnit.Select();
        m_Assignable = unit as IWorkerAssignable; // foundation/farm → show the assign button
    }

    bool HasClickedOnActiveUnit(Unit clickedUnit)
    {
        return clickedUnit == ActiveUnit;
    }


    void DisplayClickEffect(Vector2 worldPoint, ClickType clickType)
    {
        if (clickType == ClickType.Move)
        {
            Instantiate(m_PointToMovePrefab, (Vector3)worldPoint, Quaternion.identity);
        }
        else if (clickType == ClickType.Build)
        {
            Instantiate(m_PointToBuildPrefab, (Vector3)worldPoint, Quaternion.identity);
        }
        else if (clickType == ClickType.Attack)
        {
            Instantiate(m_PointToAttackPrefab, (Vector3)worldPoint, Quaternion.identity);
        }
        else if (clickType == ClickType.Chop)
        {
            Instantiate(m_PointToChopPrefab, (Vector3)worldPoint, Quaternion.identity);
        }
    }

    void ShowUnitActions(Unit unit)
    {
        ClearActionBarUI();

        if (unit.Actions.Length == 0)
        {
            return;
        }

        m_ActionBar.Show();

        foreach (var action in unit.Actions)
        {
            m_ActionBar.RegisterAction(
                action.Icon,
                () => {
                    AudioManager.Get().PlayBtnClick();
                    action.Execute(this);
                }
            );
        }
    }

    void ClearActionBarUI()
    {
        if (m_BuildConfirmationBar.enabled)
        {
            m_BuildConfirmationBar.Hide();
        }

        m_ActionBar.ClearActions();
        m_ActionBar.Hide();
    }

void ConfirmBuildPlacement()
{
    if (!TryDeductResources(m_PlacementProcess.GoldCost, m_PlacementProcess.WoodCost))
    {
        Debug.Log("Not Enough Resources!");
        return;
    }

    if (m_PlacementProcess.TryFinalizePlacement(out Vector3 buildPosition))
    {
        // Place the foundation and keep it selected so the "assign worker" button shows below it.
        var process = new BuildingProcess(m_PlacementProcess.BuildAction, buildPosition, m_ConstructionEffectPrefab);
        SelectNewUnit(process.Structure);

        DisplayClickEffect(buildPosition, ClickType.Build);
        AudioManager.Get().PlaySound(m_PlacementAudioSettings, buildPosition);
        m_BuildConfirmationBar.Hide();

        m_PlacementProcess = null;
        m_CameraController.LockCamera = false;
    }
    else
    {
        AddResources(m_PlacementProcess.GoldCost, m_PlacementProcess.WoodCost); // refund invalid placement
    }
}


    void CancelBuildPlacement()
    {
        AudioManager.Get().PlayBtnClick();
        m_BuildConfirmationBar.Hide();
        m_PlacementProcess.Cleanup();
        m_PlacementProcess = null;
        m_CameraController.LockCamera = false;
    }

    public bool TryDeductResources(int goldCost, int woodCost)
    {
        if (Gold >= goldCost && Wood >= woodCost)
        {
            AddResources(-goldCost, -woodCost);
            return true;
        }

        return false;
    }

    void RefreshResourceUI()
    {
        if (m_ResourceDataUI == null) return;
        m_ResourceDataUI.UpdateResourceDisplay(Gold, Wood);
        m_ResourceDataUI.UpdateExtraResources(m_Resources);
    }

    void RefreshPopulationUI()
    {
        if (m_ResourceDataUI != null)
            m_ResourceDataUI.UpdatePopulation(CurrentPopulation, Population.MaxPopulation, AvailableWorkers);
    }
    
    void OnGUI()
    {
        // if (ActiveUnit != null)
        // {
        //     GUI.Label(new Rect(20, 120, 200, 20), "State: " + ActiveUnit.CurrentState.ToString(), new GUIStyle { fontSize = 30 });
        //     GUI.Label(new Rect(20, 160, 200, 20), "Task: " + ActiveUnit.CurrentTask.ToString(), new GUIStyle { fontSize = 30 });
        //     GUI.Label(new Rect(20, 200, 200, 20), "Stance: " + ActiveUnit.CurrentStance.ToString(), new GUIStyle { fontSize = 30 });
        // }
    }

    void CloseFoodPanel()
    {
        m_FoodPanel.Hide();
    }

    public void StartCookProcess(CookFoodActionSO cookFoodActionSo)
    {
        m_FoodPanel.Show();
        m_FoodPanel.SetupHooks(CloseFoodPanel);
    }
}