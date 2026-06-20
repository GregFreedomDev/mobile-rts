

using UnityEngine;

public class StructureUnit : Unit, IWorkerAssignable
{
    [SerializeField] private bool m_CanStoreWood = false;
    [SerializeField] private bool m_CanStoreGold = false;
    private BuildingProcess m_BuildingProcess;

    public override bool IsBuilding => true;
    public bool IsUnderConstuction => m_BuildingProcess != null;
    // A placed foundation that has no builder assigned or on the way.
    public bool IsAwaitingWorker => m_BuildingProcess != null && !m_BuildingProcess.HasAssignedBuilder;

    public void AssignBuilder(WorkerUnit worker) => m_BuildingProcess?.AssignBuilder(worker);
    public bool CanStoreGold => m_CanStoreGold;
    public bool CanStoreWood => m_CanStoreWood;

    // IWorkerAssignable — a foundation offers a builder; subclasses (farm) extend for the built state.
    public Transform AnchorTransform => transform;
    public virtual string AssignLabel => "Asignar trabajador";
    public virtual bool NeedsWorker => IsAwaitingWorker;
    public virtual void AssignWorker(WorkerUnit worker)
    {
        if (IsUnderConstuction) AssignBuilder(worker);
    }

    void Update()
    {
        if (IsUnderConstuction)
        {
            m_BuildingProcess.Update();
        }
        else
        {
            AfterConstructionUpdate();
        }
    }

    public virtual void OnConstructionFinished()
    {
        m_BuildingProcess = null;
        UpdateWalkability();
    }

    public void RegisterProcess(BuildingProcess process)
    {
        m_BuildingProcess = process;
    }

    public void AssignWorkerToBuildProcess(WorkerUnit worker)
    {
        m_BuildingProcess?.AddWorker(worker);
    }

    public void UnassignWorkerFromBuildProcess()
    {
        m_BuildingProcess?.RemoveWorker();
    }

    protected virtual void AfterConstructionUpdate() {}

    // Size (in tiles) of the non-walkable block this building stamps onto the pathfinding grid.
    // Smaller buildings (e.g. the farm) override this so units can stand closer to them.
    protected virtual int WalkabilityWidth => 4;
    protected virtual int WalkabilityHeight => 4;

    void UpdateWalkability()
    {
        int buildingWidthInTiles = WalkabilityWidth;
        int buildingHeightInTiles = WalkabilityHeight;

        float halfWidth = buildingWidthInTiles / 2f;
        float halfHeight = buildingHeightInTiles / 2f;

        Vector3Int startPosition = new Vector3Int(
            Mathf.FloorToInt(transform.position.x - halfWidth),
            Mathf.FloorToInt(transform.position.y - halfHeight),
            0
        );

        TilemapManager.Get()
            .UpdateNodesInArea(startPosition, buildingWidthInTiles, buildingHeightInTiles);
    }
}
