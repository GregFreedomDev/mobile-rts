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

    protected override void OnCycleComplete()
    {
        MGameGameManager.AddResource(m_OutputResource, m_AmountPerCycle);
        MGameGameManager.ShowTextPopup($"+{m_AmountPerCycle} {m_OutputResource}", GetTopPosition(),
            new Color(0.95f, 0.85f, 0.25f));
    }
}
