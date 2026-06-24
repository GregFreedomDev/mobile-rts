

using UnityEngine;

public class WorkerUnit : HumanoidUnit
{
    [SerializeField] private float m_WoodGatherTickTime = 1f;
    [SerializeField] private int m_WoodPerTick = 4;
    [SerializeField] private float m_HitTreeFrequency = 0.5f;

    [SerializeField] private SpriteRenderer m_HoldingWoodSprite;
    [SerializeField] private SpriteRenderer m_HoldingGoldSprite;
    [SerializeField] private AudioSettings m_ChopAudioSettings;
    [SerializeField] private AudioSettings m_BuildAudioSettings;

    private float m_ChoppingTimer;
    private float m_HitTreeTimer;
    private int m_WoodCollected;
    private int m_GoldCollected;
    private int m_WoodCapacity = 20;
    private int m_GoldCapacity = 10;
    private Tree m_AssignedTree;
    private GoldMine m_AssignedMine;
    private StructureUnit m_AssignedWoodStorage;
    private StructureUnit m_AssignedGoldStorage;

    public bool IsHoldingWood => m_WoodCollected > 0;
    public bool IsHoldingGold => m_GoldCollected > 0;
    public bool IsHoldingResource => IsHoldingWood || IsHoldingGold;

    // Set while assigned to tend a building (robust against the task being reset elsewhere).
    public bool IsTending { get; set; }

    // Free to take a new job: alive, not tending a building, and with no other task.
    public bool IsAvailable => CurrentState != UnitState.Dead && CurrentTask == UnitTask.None && !IsTending;

    
    protected override void UpdateBehaviour()
    {
        var gameManager = (MGameGameManager as GameManager);
        switch (CurrentTask)
        {
            case UnitTask.Build when HasTarget:
                CheckForConstruction();
                break;
            case UnitTask.Chop
                when m_AssignedTree != null
                     && m_WoodCollected < m_WoodCapacity:
                HandleChoppingTask();
                break;
            case UnitTask.Mine
                when m_AssignedMine != null
                     && !IsHoldingGold:
                HandleMinningTask();
                break;
            case UnitTask.ReturnResource when IsHoldingGold && TryToReturnResources(m_AssignedGoldStorage):
                MGameGameManager.ShowTextPopup(m_GoldCollected.ToString(), GetTopPosition(), Color.yellow);
                MGameGameManager.AddResources(m_GoldCollected, 0);
                m_GoldCollected = 0;
                MoveTo(gameManager.ActiveGoldMine.GetBottomPosition());
                SetTask(UnitTask.Mine);
                break;
            case UnitTask.ReturnResource:
            {
                if (IsHoldingWood && TryToReturnResources(m_AssignedWoodStorage, 1f))
                {
                    MGameGameManager.ShowTextPopup(m_WoodCollected.ToString(), GetTopPosition(), Color.green);
                    MGameGameManager.AddResources(0, m_WoodCollected);
                    m_WoodCollected = 0;
                    TryMoveToClosestTree();
                }

                break;
            }
        }

        if (CurrentState == UnitState.Chopping && m_WoodCollected < m_WoodCapacity)
        {
            StartChopping();
        }

        HandleResourceDisplay();
    }

    public void OnBuildingFinished(Vector3 stepAsidePosition)
    {
        ResetState();
        MoveTo(stepAsidePosition); // walk off the finished building so the worker isn't standing on it
    }

    public void SetWoodStorage(StructureUnit storage)
    {
        m_AssignedWoodStorage = storage;
    }

    public void SetGoldStorage(StructureUnit storage)
    {
        m_AssignedGoldStorage = storage;
    }

    public void SendToBuild(StructureUnit structure, DestinationSource destinationSource = DestinationSource.CodeTriggered)
    {
        // Walk to the tile next to the structure (in the worker's direction), not its center —
        // otherwise the pathfinder drops the worker onto the building. One tile out keeps it beside.
        Vector2 fromCenter = (Vector2)transform.position - (Vector2)structure.transform.position;
        Vector2 step;
        if (fromCenter == Vector2.zero)
            step = Vector2.down;
        else if (Mathf.Abs(fromCenter.x) >= Mathf.Abs(fromCenter.y))
            step = new Vector2(Mathf.Sign(fromCenter.x), 0f);
        else
            step = new Vector2(0f, Mathf.Sign(fromCenter.y));

        Vector3 approach = (Vector2)structure.transform.position + step;

        MoveTo(approach, destinationSource);
        SetTarget(structure);
        SetTask(UnitTask.Build);
    }

    public void SendToChop(Tree tree, DestinationSource destinationSource = DestinationSource.CodeTriggered)
    {
        if (tree.TryToClaim())
        {
            MoveTo(tree.GetBottomPosition(), destinationSource);
            SetTask(UnitTask.Chop);
            m_AssignedTree = tree;
        }
    }

    public void SendToMine(GoldMine mine, DestinationSource destinationSource = DestinationSource.CodeTriggered)
    {
        MoveTo(mine.GetBottomPosition(), destinationSource);
        SetTask(UnitTask.Mine);
        m_AssignedMine = mine;
    }

    public void OnEnterMine()
    {
        Hide();
    }

    public void OnLeaveMine()
    {
        var gameManager = (MGameGameManager as GameManager);
        Show();
        m_GoldCollected = m_GoldCapacity;
        SetState(UnitState.Idle);

        m_AssignedGoldStorage = gameManager.FindClosestGoldStorage(transform.position);

        if (m_AssignedGoldStorage != null)
        {
            MoveTo(m_AssignedGoldStorage.transform.position);
            SetTask(UnitTask.ReturnResource);
        }
    }

    public void PlayBuildSound()
    {
        m_AudioManager.PlaySound(m_BuildAudioSettings, transform.position);
    }

    protected override void OnSetDestination(DestinationSource source)
    {
        base.OnSetDestination(source);
        if (CurrentState == UnitState.Minig) return;

        SetState(UnitState.Moving);
        ResetState();
    }

    protected override void Die()
    {
        base.Die();
        if (m_AssignedTree != null) m_AssignedTree.Release();

    }

    bool TryToReturnResources(StructureUnit storage, float distanceTreshold = 0.5f)
    {
        if (storage != null)
        {
            var closestPoint = storage.Collider.ClosestPoint(transform.position);
            var distance = Vector3.Distance(closestPoint, transform.position);
            return distance < distanceTreshold;
        }

        return false;
    }

    void HandleResourceDisplay()
    {
        if (IsHoldingResource)
        {
            if (IsHoldingGold)
            {
                m_HoldingGoldSprite.gameObject.SetActive(true);
                m_HoldingWoodSprite.gameObject.SetActive(false);
            }
            else
            {
                m_HoldingGoldSprite.gameObject.SetActive(false);
                m_HoldingWoodSprite.gameObject.SetActive(true);
            }

            m_Animator.SetFloat("IsHoldingResource", 1f);
        }
        else
        {
            m_HoldingGoldSprite.gameObject.SetActive(false);
            m_HoldingWoodSprite.gameObject.SetActive(false);
            m_Animator.SetFloat("IsHoldingResource", 0f);
        }
    }

    void HandleMinningTask()
    {
        var mineBottomPosition = m_AssignedMine.GetBottomPosition();
        var workerClosestPoint = Collider.ClosestPoint(mineBottomPosition);
        var distance = Vector3.Distance(mineBottomPosition, workerClosestPoint);

        if (distance <= 0.20f)
        {
            if (m_AssignedMine.TryToEnterMine(this))
            {
                m_WoodCollected = 0;
                StopMovement();
                SetState(UnitState.Minig);
            }
        }
    }

    void HandleChoppingTask()
    {
        var treeBottomPosition = m_AssignedTree.GetBottomPosition();
        var workerClosestPoint = Collider.ClosestPoint(treeBottomPosition);

        var distance = Vector3.Distance(treeBottomPosition, workerClosestPoint);

        if (distance <= 0.1f)
        {
            m_GoldCollected = 0;
            StopMovement();
            SetState(UnitState.Chopping);
        }
    }

    void StartChopping()
    {
        m_Animator.SetBool("IsChopping", true);
        m_ChoppingTimer += Time.deltaTime;
        m_HitTreeTimer += Time.deltaTime;

        if (m_HitTreeTimer >= m_HitTreeFrequency)
        {
            m_HitTreeTimer = 0;
            m_AssignedTree.Hit();
            m_AudioManager.PlaySound(m_ChopAudioSettings, transform.position);
        }

        if (m_ChoppingTimer >= m_WoodGatherTickTime)
        {
            m_WoodCollected += m_WoodPerTick;
            m_ChoppingTimer = 0;

            if (m_WoodCollected == m_WoodCapacity)
            {
                HandleChoppingFinished();
            }
        }
    }

    void HandleChoppingFinished()
    {
        var gameManager = (MGameGameManager as GameManager);

        m_Animator.SetBool("IsChopping", false);

        m_AssignedWoodStorage = gameManager.FindClosestWoodStorage(transform.position);

        if (m_AssignedWoodStorage != null)
        {
            var closestPointOnStorage = m_AssignedWoodStorage.Collider.ClosestPoint(transform.position);
            MoveTo(closestPointOnStorage);
        }

        SetState(UnitState.Idle);
        SetTask(UnitTask.ReturnResource);
    }

    void CheckForConstruction()
    {
        // Measure to the building's collider edge (not its center) so large buildings are reachable.
        var structure = Target as StructureUnit;
        float distance = structure != null && structure.Collider != null
            ? Vector2.Distance(transform.position, structure.Collider.ClosestPoint(transform.position))
            : Vector3.Distance(transform.position, Target.transform.position);

        if (distance <= m_ObjectDetectionRadius && CurrentState == UnitState.Idle)
        {
            StartBuilding(structure);
        }
    }

    void StartBuilding(StructureUnit structure)
    {
        SetState(UnitState.Building);
        m_Animator.SetBool("IsBuilding", true);
        structure.AssignWorkerToBuildProcess(this);
    }

    void TryMoveToClosestTree()
    {
        var gameManager = (MGameGameManager as GameManager);

        var closestTree = gameManager.FindClosestUnclaimedTree(transform.position);

        if (closestTree != null)
        {
            SendToChop(closestTree);
        }
    }

    void ResetState()
    {
        SetTask(UnitTask.None);

        if (HasTarget) CleanupTarget();

        m_Animator.SetBool("IsBuilding", false);
        m_Animator.SetBool("IsChopping", false);

        m_ChoppingTimer = 0;

        if (m_AssignedTree != null)
        {
            m_AssignedTree.Release();
            m_AssignedTree = null;
        }
    }

    void CleanupTarget()
    {
        if (Target is StructureUnit structure)
        {
            structure.UnassignWorkerFromBuildProcess();
        }

        SetTarget(null);
    }
}
