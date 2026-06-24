using hvo.Scripts.Utils;
using UnityEngine;

/// <summary>
/// Base for buildings that do a job every cycle while an assigned villager tends them
/// (Huerto, Molino, ...). Handles assigning/walking/wandering the worker, the progress bar and the
/// cycle timer. Each cycle's output is delivered: if some other built building CONSUMES that
/// resource (e.g. the mill consumes wheat), the worker carries it there and it only counts on
/// arrival; otherwise it is added straight to the economy.
/// </summary>
public abstract class WorkerTendedBuilding : StructureUnit
{
    [Header("Cycle")]
    [SerializeField] protected float m_CycleSeconds = 8f;

    private const float WorkRingBase = 0.9f;
    private const float NearRange = 2f;

    private enum Phase { Working, Delivering }

    private WorkerUnit m_AssignedWorker;
    private float m_Timer;
    private float m_NextWanderTime;
    private ProductionBar m_Bar;

    private Phase m_Phase = Phase.Working;
    private StructureUnit m_DeliveryTarget;
    private ResourceType m_CarriedResource;
    private int m_CarriedAmount;

    // Minimal footprint so the tending worker can stand right next to the plot/building.
    protected override int WalkabilityWidth => 1;
    protected override int WalkabilityHeight => 1;

    /// <summary>Resource this building consumes as input (so producers can deliver to it). Null = none.</summary>
    public virtual ResourceType? ConsumesResource => null;

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
        Unassign();

        Vector2 dir = Random.insideUnitCircle.normalized;
        if (dir == Vector2.zero) dir = Vector2.down;
        worker.MoveTo((Vector2)transform.position + dir * 4f); // walk it off
    }

    // --- subclass hooks ---
    /// <summary>Whether the building can run a cycle right now (e.g. has enough input resource).</summary>
    protected abstract bool CanProduce();
    /// <summary>Produces one cycle: returns the resource and amount to deliver/add (amount 0 = nothing).</summary>
    protected abstract void RunCycle(out ResourceType output, out int amount);

    protected override void AfterConstructionUpdate()
    {
        if (MGameGameManager == null) return;

        EnsureBar();

        if (m_AssignedWorker != null && m_AssignedWorker.CurrentState == UnitState.Dead)
            Unassign();

        if (m_AssignedWorker == null)
        {
            m_Bar.SetVisible(false);
            m_Timer = 0f;
            m_Phase = Phase.Working;
            return;
        }

        if (m_Phase == Phase.Delivering)
        {
            m_Bar.SetVisible(false);
            HandleDelivery();
            return;
        }

        if (!IsWorkerNear() || !CanProduce())
        {
            m_Bar.SetVisible(false);
            if (!IsWorkerNear()) m_Timer = 0f; // worker still walking back/over
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
            CompleteCycle();
        }
    }

    private void CompleteCycle()
    {
        RunCycle(out ResourceType output, out int amount);
        if (amount <= 0) return;

        var consumer = FindConsumer(output);
        if (consumer != null)
        {
            // Carry the harvest to the consumer; it only counts once delivered.
            m_Phase = Phase.Delivering;
            m_DeliveryTarget = consumer;
            m_CarriedResource = output;
            m_CarriedAmount = amount;
            m_AssignedWorker.MoveTo(ApproachPoint(consumer));
        }
        else
        {
            Deposit(output, amount, transform.position); // no consumer — straight into the economy
        }
    }

    private void HandleDelivery()
    {
        if (m_DeliveryTarget == null) // consumer vanished — drop it into the economy
        {
            Deposit(m_CarriedResource, m_CarriedAmount, transform.position);
            EndDelivery();
            return;
        }

        float reach = 0.8f + (m_DeliveryTarget.Collider != null ? m_DeliveryTarget.Collider.bounds.extents.magnitude : 0.6f);
        float dist = Vector2.Distance(m_AssignedWorker.transform.position, m_DeliveryTarget.transform.position);

        if (dist <= reach)
        {
            Deposit(m_CarriedResource, m_CarriedAmount, m_DeliveryTarget.transform.position);
            EndDelivery();
            m_AssignedWorker.MoveTo((Vector2)transform.position + Vector2.down * WorkRingBase); // head back
        }
        else if (m_AssignedWorker.CurrentState != UnitState.Moving)
        {
            m_AssignedWorker.MoveTo(ApproachPoint(m_DeliveryTarget)); // nudge if it stopped short
        }
    }

    private void EndDelivery()
    {
        m_Phase = Phase.Working;
        m_DeliveryTarget = null;
        m_CarriedAmount = 0;
    }

    private void Deposit(ResourceType res, int amount, Vector3 at)
    {
        MGameGameManager.AddResource(res, amount);
        MGameGameManager.ShowTextPopup($"+{amount} {res}", at + Vector3.up, new Color(0.95f, 0.85f, 0.25f));
    }

    private WorkerTendedBuilding FindConsumer(ResourceType output)
    {
        WorkerTendedBuilding best = null;
        float bestSqr = float.MaxValue;

        foreach (var building in FindObjectsByType<WorkerTendedBuilding>(FindObjectsSortMode.None))
        {
            if (building == this || building.IsUnderConstuction) continue;
            if (building.ConsumesResource != output) continue;

            float sqr = (building.transform.position - transform.position).sqrMagnitude;
            if (sqr < bestSqr) { bestSqr = sqr; best = building; }
        }

        return best;
    }

    private Vector2 ApproachPoint(StructureUnit target)
    {
        Vector2 from = (Vector2)m_AssignedWorker.transform.position - (Vector2)target.transform.position;
        Vector2 dir = from.sqrMagnitude > 0.01f ? from.normalized : Vector2.down;
        float r = (target.Collider != null ? target.Collider.bounds.extents.x : 0.6f) + 0.6f;
        return (Vector2)target.transform.position + dir * r;
    }

    // Lets another building drop this worker when it gets reassigned, so one villager only ever
    // tends a single building (otherwise nearby buildings would all "share" the same tender).
    public void ReleaseIfTending(WorkerUnit worker)
    {
        if (m_AssignedWorker == worker) Unassign();
    }

    private void AssignTender(WorkerUnit worker)
    {
        // Drop the worker from any building it was tending before.
        foreach (var other in FindObjectsByType<WorkerTendedBuilding>(FindObjectsSortMode.None))
            if (other != this) other.ReleaseIfTending(worker);

        m_AssignedWorker = worker;
        m_Phase = Phase.Working;
        worker.IsTending = true;        // busy regardless of the task field
        worker.SetTask(UnitTask.Farm);  // mark busy so it isn't picked for other jobs
        SendWorkerToPlot();
    }

    private void Unassign()
    {
        if (m_AssignedWorker != null)
        {
            m_AssignedWorker.IsTending = false;
            if (m_AssignedWorker.CurrentTask == UnitTask.Farm)
                m_AssignedWorker.SetTask(UnitTask.None); // free it for other jobs
        }
        m_AssignedWorker = null;
        m_Phase = Phase.Working;
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
