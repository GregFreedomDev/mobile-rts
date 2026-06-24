using UnityEngine;

/// <summary>
/// Generic processing building: while a villager tends it, each cycle it consumes an input resource
/// and yields an output (only when there is enough input). Configure the input/output per prefab.
/// Used by the Molino (Trigo → Harina) and the Cocina (Harina → Pan), etc.
/// </summary>
public class ProcessingBuilding : WorkerTendedBuilding
{
    [Header("Processing")]
    [SerializeField] private ResourceType m_InputResource = ResourceType.Wheat;
    [SerializeField] private int m_InputAmount = 2;
    [SerializeField] private ResourceType m_OutputResource = ResourceType.Flour;
    [SerializeField] private int m_OutputAmount = 1;

    // Producers (farms, other processors) deliver this input here.
    public override ResourceType? ConsumesResource => m_InputResource;

    protected override bool CanProduce() => MGameGameManager.HasResource(m_InputResource, m_InputAmount);

    protected override void RunCycle(out ResourceType output, out int amount)
    {
        output = m_OutputResource;
        amount = MGameGameManager.SpendResource(m_InputResource, m_InputAmount) ? m_OutputAmount : 0;
    }
}
