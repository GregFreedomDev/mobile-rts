using hvo.Scripts.Utils;
using UnityEngine;

/// <summary>
/// Base for buildings that do a job every cycle while an assigned villager tends them
/// (Huerto, Molino, Fundición, ...). Handles assigning/walking/wandering the worker, the
/// progress bar, and the cycle timer. Subclasses define what each cycle produces.
/// </summary>
public abstract class WorkerTendedBuilding : StructureUnit
{
    [Header("Cycle")]
    [SerializeField] protected float m_CycleSeconds = 8f;

    private const float TendLeash = 3.5f;
    private const float WorkRingBase = 0.9f;
    private const float NearRange = 2f;

    private WorkerUnit m_AssignedWorker;
    private bool m_TenderArrived;
    private float m_Timer;
    private float m_NextWanderTime;
    private ProductionBar m_Bar;

    // Minimal footprint so the tending worker can stand right next to the plot/building.
    protected override int WalkabilityWidth => 1;
    protected override int WalkabilityHeight => 1;

    private bool IsTended => m_AssignedWorker != null && m_AssignedWorker.CurrentState != UnitState.Dead;

    // While a foundation, fall back to the base (assign a builder). Once built, needs a tender.
    public override bool NeedsWorker => IsUnderConstuction ? base.NeedsWorker : !IsTended;
    public override bool HasWorker => !IsUnderConstuction && IsTended;

    public override void AssignWorker(WorkerUnit worker)
    {
        if (IsUnderConstuction) base.AssignWorker(worker); // build the foundation
        else AssignTender(worker);
    }

    public override void ReleaseWorker()
    {
        if (m_AssignedWorker == null) return;

        WorkerUnit worker = m_AssignedWorker;
        Unassign(); // clears the reference and frees the worker's task

        // Walk it off to a spot clearly away from the building.
        Vector2 dir = Random.insideUnitCircle.normalized;
        if (dir == Vector2.zero) dir = Vector2.down;
        worker.MoveTo((Vector2)transform.position + dir * 4f);
    }

    // --- subclass hooks ---
    /// <summary>Whether the building can run a cycle right now (e.g. has enough input resource).</summary>
    protected abstract bool CanProduce();
    /// <summary>Runs once per completed cycle: consume input / add output.</summary>
    protected abstract void OnCycleComplete();

    protected override void AfterConstructionUpdate()
    {
        if (MGameGameManager == null) return;

        EnsureBar();
        UpdateTenderAssignment();

        bool working = m_AssignedWorker != null && IsWorkerNear();
        if (!working || !CanProduce())
        {
            m_Bar.SetVisible(false);
            if (!working) m_Timer = 0f; // reset only when unattended; hold progress if just waiting on input
            return;
        }

        // Keep the worker wandering around so it looks like it's working.
        if (Time.time >= m_NextWanderTime && m_AssignedWorker.CurrentState != UnitState.Moving)
            SendWorkerToPlot();

        m_Timer += Time.deltaTime;
        m_Bar.SetVisible(true);
        m_Bar.SetProgress(m_Timer / m_CycleSeconds);

        if (m_Timer >= m_CycleSeconds)
        {
            m_Timer = 0f;
            OnCycleComplete();
        }
    }

    private void AssignTender(WorkerUnit worker)
    {
        m_AssignedWorker = worker;
        m_TenderArrived = false;
        worker.SetTask(UnitTask.Farm); // mark busy so it isn't picked for other jobs
        SendWorkerToPlot();
    }

    // Drops the tender entirely once it has arrived and then left, so moving the worker away is a
    // permanent un-assignment rather than a pause.
    private void UpdateTenderAssignment()
    {
        if (m_AssignedWorker == null) return;

        if (m_AssignedWorker.CurrentState == UnitState.Dead)
        {
            Unassign();
            return;
        }

        float distance = Vector2.Distance(m_AssignedWorker.transform.position, transform.position);
        if (distance <= NearRange) m_TenderArrived = true;
        else if (m_TenderArrived && distance > TendLeash) Unassign();
    }

    private void Unassign()
    {
        if (m_AssignedWorker != null && m_AssignedWorker.CurrentTask == UnitTask.Farm)
            m_AssignedWorker.SetTask(UnitTask.None); // free it for other jobs
        m_AssignedWorker = null;
        m_TenderArrived = false;
    }

    private void SendWorkerToPlot()
    {
        if (m_AssignedWorker == null) return;

        Vector2 dir = Random.insideUnitCircle.normalized;
        if (dir == Vector2.zero) dir = Vector2.down;
        float ring = WorkRingBase * Random.Range(1f, 1.3f);

        m_AssignedWorker.MoveTo((Vector2)transform.position + dir * ring);
        m_NextWanderTime = Time.time + Random.Range(2f, 3.5f);
    }

    private bool IsWorkerNear()
    {
        if (m_AssignedWorker == null || m_AssignedWorker.CurrentState == UnitState.Dead) return false;
        return Vector2.Distance(m_AssignedWorker.transform.position, transform.position) <= NearRange;
    }

    private void EnsureBar()
    {
        if (m_Bar != null) return;

        float topY = 1.0f;
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) topY = sr.bounds.max.y - transform.position.y + 0.15f;

        m_Bar = new ProductionBar(transform, topY);
    }
}
