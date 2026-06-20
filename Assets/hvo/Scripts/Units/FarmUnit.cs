using hvo.Scripts.Utils;
using UnityEngine;

/// <summary>
/// Resource producer (the "Huerto"/farm). Each farm grows a SINGLE crop, yielding it every cycle
/// into the shared economy, but only while its assigned villager is tending it. Assign a worker by
/// selecting it and clicking the farm: the farm walks the worker over and keeps it wandering around
/// the plot as if working. Shows a world-space progress bar while producing.
/// </summary>
public class FarmUnity : StructureUnit
{
    [Header("Production")]
    [SerializeField] private ResourceType m_OutputResource = ResourceType.Wheat;
    [SerializeField] private int m_AmountPerCycle = 2;
    [SerializeField] private float m_CycleSeconds = 8f;

    private WorkerUnit m_AssignedWorker;
    private float m_Timer;
    private float m_NextWanderTime;
    private ProductionBar m_Bar;

    public ResourceType OutputResource => m_OutputResource;

    // Minimal footprint (just the plot tile) so the tending worker can stand right next to it.
    protected override int WalkabilityWidth => 1;
    protected override int WalkabilityHeight => 1;

    // Once built, the farm needs a villager to plant/tend it. While a foundation, fall back to the
    // base (assign a builder). A tender that has reached the plot but is then moved away gets fully
    // unassigned (see UpdateTenderAssignment), so it won't auto-resume if it later passes by.
    private const float TendLeash = 3.5f;
    private bool m_TenderArrived;
    private bool IsTended => m_AssignedWorker != null && m_AssignedWorker.CurrentState != UnitState.Dead;

    public override bool NeedsWorker => IsUnderConstuction ? base.NeedsWorker : !IsTended;

    public override void AssignWorker(WorkerUnit worker)
    {
        if (IsUnderConstuction) base.AssignWorker(worker); // build the foundation
        else AssignTender(worker);                         // plant/tend the crop
    }

    private void AssignTender(WorkerUnit worker)
    {
        m_AssignedWorker = worker;
        m_TenderArrived = false;
        worker.SetTask(UnitTask.Farm); // mark busy so it isn't picked for other jobs
        SendWorkerToPlot();            // walk it over to start working
    }

    // Drops the tender entirely once it has arrived and then left the plot, so moving the worker
    // away is a permanent un-assignment rather than a pause.
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

    protected override void AfterConstructionUpdate()
    {
        if (MGameGameManager == null) return;

        EnsureBar();
        UpdateTenderAssignment(); // release the tender if it has wandered off the plot

        if (m_AssignedWorker == null || !IsWorkerNear())
        {
            m_Bar.SetVisible(false); // no tender, or it's still walking over
            m_Timer = 0f;
            return;
        }

        // Keep the worker wandering around the plot so it looks like it's tending the crops.
        if (Time.time >= m_NextWanderTime && m_AssignedWorker.CurrentState != UnitState.Moving)
            SendWorkerToPlot();

        m_Timer += Time.deltaTime;
        m_Bar.SetVisible(true);
        m_Bar.SetProgress(m_Timer / m_CycleSeconds);

        if (m_Timer >= m_CycleSeconds)
        {
            m_Timer = 0f;
            Produce();
        }
    }

    // The worker wanders on a tight ring hugging the plot edge.
    private const float WorkRingBase = 0.9f;
    private const float NearRange = 2f;

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

    private void Produce()
    {
        MGameGameManager.AddResource(m_OutputResource, m_AmountPerCycle);
        MGameGameManager.ShowTextPopup($"+{m_AmountPerCycle} {m_OutputResource}", GetTopPosition(), new Color(0.95f, 0.85f, 0.25f));
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
