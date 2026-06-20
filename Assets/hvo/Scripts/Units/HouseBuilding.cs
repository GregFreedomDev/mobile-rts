using UnityEngine;

/// <summary>
/// A building that adds housing capacity and (optionally) generates villagers over time.
/// Reusable: capacity 1 + GeneratesVillagers for a House, or capacity 4 + no generation for
/// the starting Castle. Population capacity is registered lazily on the first post-construction
/// frame, which covers both freshly built houses and buildings that start already built.
/// </summary>
public class HouseBuilding : StructureUnit
{
    [Header("Population")]
    [SerializeField] private int m_PopulationCapacity = 1;
    [SerializeField] private bool m_GeneratesVillagers = true;
    [SerializeField] private Unit m_VillagerPrefab;
    [SerializeField] private float m_GenerationInterval = 10f;

    private bool m_CapacityRegistered;
    private float m_NextSpawnTime;

    protected override void AfterConstructionUpdate()
    {
        if (!m_CapacityRegistered)
        {
            if (MGameGameManager == null) return; // wait until the manager reference is resolved
            MGameGameManager.Population.IncreaseMax(m_PopulationCapacity);
            m_CapacityRegistered = true;
            m_NextSpawnTime = Time.time + m_GenerationInterval;
        }

        if (!m_GeneratesVillagers || m_VillagerPrefab == null) return;
        if (Time.time < m_NextSpawnTime) return;

        m_NextSpawnTime = Time.time + m_GenerationInterval;
        TrySpawnVillager();
    }

    private void TrySpawnVillager()
    {
        if (MGameGameManager == null || !MGameGameManager.HasPopulationSpace) return;

        float radius = Collider != null ? Collider.bounds.extents.x + 0.6f : 1f;
        Vector2 dir = Random.insideUnitCircle.normalized;
        if (dir == Vector2.zero) dir = Vector2.down;
        Vector2 spawnPos = (Vector2)transform.position + dir * radius;

        Instantiate(m_VillagerPrefab, spawnPos, Quaternion.identity);
    }
}
