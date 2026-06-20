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

    public void AssignWorker(WorkerUnit worker)
    {
        m_AssignedWorker = worker;
        SendWorkerToPlot(); // walk it over to start working
    }

    protected override void AfterConstructionUpdate()
    {
        if (MGameGameManager == null) return;

        EnsureBar();

        if (!IsWorkerNear())
        {
            m_Bar.SetVisible(false); // worker is away (walking over or sent elsewhere)
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
