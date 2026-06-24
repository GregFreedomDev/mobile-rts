/// <summary>All resource kinds tracked by the economy. Gold/Wood are the originals;
/// the rest power farming, processing and crafting (Features 3+).</summary>
public enum ResourceType
{
    Gold,
    Wood,
    Food,
    Iron,
    Carbon,
    Wheat,
    Corn,
    Flour,
    Bread
}

/// <summary>A typed resource amount, serializable so ScriptableObjects can declare multi-resource costs.</summary>
[System.Serializable]
public struct ResourceCost
{
    public ResourceType Type;
    public int Amount;

    public ResourceCost(ResourceType type, int amount)
    {
        Type = type;
        Amount = amount;
    }
}
