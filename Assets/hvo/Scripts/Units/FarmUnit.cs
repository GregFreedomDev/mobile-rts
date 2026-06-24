using UnityEngine;

/// <summary>
/// Resource producer (the "Huerto"/farm). Grows a SINGLE crop, yielding it every cycle while an
/// assigned villager tends it. Wheat by default; set Output Resource to Corn for a corn farm.
/// </summary>
public class FarmUnity : WorkerTendedBuilding
{
    [Header("Crop")]
    [SerializeField] private ResourceType m_OutputResource = ResourceType.Wheat;
    [SerializeField] private int m_AmountPerCycle = 2;

    public ResourceType OutputResource => m_OutputResource;

    protected override bool CanProduce() => true; // a farm always grows its crop

    // The base carries the harvest to a consumer (e.g. the mill for wheat), or adds it directly.
    protected override void RunCycle(out ResourceType output, out int amount)
    {
        output = m_OutputResource;
        amount = m_AmountPerCycle;
    }
}
