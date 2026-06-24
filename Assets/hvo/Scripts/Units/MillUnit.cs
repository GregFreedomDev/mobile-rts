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

    protected override bool CanProduce() => MGameGameManager.HasResource(m_InputResource, m_InputAmount);

    protected override void OnCycleComplete()
    {
        if (!MGameGameManager.SpendResource(m_InputResource, m_InputAmount)) return;

        MGameGameManager.AddResource(m_OutputResource, m_OutputAmount);
        MGameGameManager.ShowTextPopup($"+{m_OutputAmount} {m_OutputResource}", GetTopPosition(),
            new Color(0.9f, 0.85f, 0.7f));
    }
}
