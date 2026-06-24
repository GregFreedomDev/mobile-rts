using UnityEngine;

/// <summary>
/// Processing building (the "Molino"/mill). While a villager tends it, each cycle it consumes an
/// input resource and produces an output. MVP: Trigo → Harina. Only runs when there is enough input.
/// </summary>
public class MillUnit : WorkerTendedBuilding
{
    [Header("Processing")]
    [SerializeField] private ResourceType m_InputResource = ResourceType.Wheat;
    [SerializeField] private int m_InputAmount = 2;
    [SerializeField] private ResourceType m_OutputResource = ResourceType.Flour;
    [SerializeField] private int m_OutputAmount = 1;

    public override ResourceType? ConsumesResource => m_InputResource; // farms deliver wheat here

    protected override bool CanProduce() => MGameGameManager.HasResource(m_InputResource, m_InputAmount);

    protected override void RunCycle(out ResourceType output, out int amount)
    {
        // Consume the input and yield the output (which has no further consumer, so it's added directly).
        output = m_OutputResource;
        amount = MGameGameManager.SpendResource(m_InputResource, m_InputAmount) ? m_OutputAmount : 0;
    }
}
