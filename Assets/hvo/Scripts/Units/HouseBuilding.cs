using hvo.Scripts.Utils;
using UnityEngine;

/// <summary>
/// A building that adds housing capacity and (optionally) generates villagers over time.
/// Reusable: capacity 1 + GeneratesVillagers for a House, or capacity 4 + no generation for
/// the starting Castle. Each house only produces villagers to fill its OWN added capacity and
/// then stops, so building a new house makes just that house produce. A world-space progress
/// bar shows production progress while it is producing.
/// </summary>
public class HouseBuilding : StructureUnit
{
    [Header("Population")]
    [SerializeField] private int m_PopulationCapacity = 1;
    [SerializeField] private bool m_GeneratesVillagers = true;
    [SerializeField] private Unit m_VillagerPrefab;
    [SerializeField] private float m_GenerationInterval = 10f;

    private bool m_CapacityRegistered;
    private int m_VillagersProduced;
    private float m_ProductionTimer;
    private ProductionBar m_Bar;

    protected override void AfterConstructionUpdate()
    {
        if (!m_CapacityRegistered)
        {
            if (MGameGameManager == null) return; // wait until the manager reference is resolved
            MGameGameManager.Population.IncreaseMax(m_PopulationCapacity);
            m_CapacityRegistered = true;
        }

        // Only fill this house's own slots; once it has produced its capacity it stops for good.
        bool wantsToProduce = m_GeneratesVillagers
            && m_VillagerPrefab != null
            && m_VillagersProduced < m_PopulationCapacity;

        if (!wantsToProduce)
        {
            m_Bar?.SetVisible(false);
            return;
        }

        EnsureBar();

        bool hasSpace = MGameGameManager != null && MGameGameManager.HasPopulationSpace;
        if (!hasSpace)
        {
            m_Bar.SetVisible(false); // global cap full — wait
            return;
        }

        m_ProductionTimer += Time.deltaTime;
        m_Bar.SetVisible(true);
        m_Bar.SetProgress(m_ProductionTimer / m_GenerationInterval);

        if (m_ProductionTimer >= m_GenerationInterval)
        {
            m_ProductionTimer = 0f;
            m_VillagersProduced++;
            SpawnVillager();
        }
    }

    private void EnsureBar()
    {
        if (m_Bar != null) return;

        // Pin the bar just above the visible sprite (the collider is farm-sized and too tall).
        float topY = 1.0f;
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) topY = sr.bounds.max.y - transform.position.y + 0.15f;

        m_Bar = new ProductionBar(transform, topY);
    }

    private void SpawnVillager()
    {
        float radius = Collider != null ? Collider.bounds.extents.x + 0.6f : 1f;
        Vector2 dir = Random.insideUnitCircle.normalized;
        if (dir == Vector2.zero) dir = Vector2.down;
        Vector2 spawnPos = (Vector2)transform.position + dir * radius;

        Instantiate(m_VillagerPrefab, spawnPos, Quaternion.identity);
    }
}
