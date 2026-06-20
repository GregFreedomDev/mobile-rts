using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The list of buildings shown in the global Build menu. Lives under Resources so it can be loaded
/// without any Inspector wiring.
/// </summary>
[CreateAssetMenu(fileName = "BuildCatalog", menuName = "HvO/BuildCatalog")]
public class BuildCatalogSO : ScriptableObject
{
    [SerializeField] private List<BuildActionSO> m_Buildings = new();

    public IReadOnlyList<BuildActionSO> Buildings => m_Buildings;
}
