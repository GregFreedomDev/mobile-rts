using System.Text;
using UnityEngine;
using TMPro;
using hvo.Scripts.Managers;

public class ResourceDataUI: MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_GoldText;
    [SerializeField] private TextMeshProUGUI m_WoodText;
    [SerializeField] private TextMeshProUGUI m_PopulationText;      // optional; auto-created if empty
    [SerializeField] private TextMeshProUGUI m_ExtraResourcesText;  // optional; auto-created if empty

    // Extra resources (beyond gold/wood) shown only once they have been produced.
    private static readonly ResourceType[] s_ExtraTypes =
    {
        ResourceType.Food, ResourceType.Wheat, ResourceType.Corn,
        ResourceType.Flour, ResourceType.Bread, ResourceType.Iron, ResourceType.Carbon
    };

    public void UpdateResourceDisplay(int gold, int wood)
    {
        m_GoldText.text = gold.ToString();
        m_WoodText.text = wood.ToString();
    }

    public void UpdatePopulation(int current, int max, int availableWorkers)
    {
        if (m_PopulationText == null)
            m_PopulationText = CreateCornerLabel(-30f, new Vector2(300f, 50f));

        m_PopulationText.text = $"Pob: {current}/{max}   Libres: {availableWorkers}";
    }

    public void UpdateExtraResources(ResourceManager resources)
    {
        if (m_ExtraResourcesText == null)
            m_ExtraResourcesText = CreateCornerLabel(-70f, new Vector2(220f, 240f));

        var sb = new StringBuilder();
        foreach (ResourceType type in s_ExtraTypes)
        {
            int amount = resources.GetAmount(type);
            if (amount > 0) sb.AppendLine($"{DisplayName(type)}: {amount}");
        }

        m_ExtraResourcesText.text = sb.ToString();
    }

    private static string DisplayName(ResourceType type) => type switch
    {
        ResourceType.Food => "Comida",
        ResourceType.Iron => "Hierro",
        ResourceType.Carbon => "Carbón",
        ResourceType.Wheat => "Trigo",
        ResourceType.Corn => "Maíz",
        ResourceType.Flour => "Harina",
        ResourceType.Bread => "Pan",
        _ => type.ToString()
    };

    // Clones the gold label (to inherit font/material/size) and pins it to the top-left corner,
    // so counters appear with no extra Editor setup. Assign the fields in the Inspector to place
    // them precisely instead.
    private TextMeshProUGUI CreateCornerLabel(float yOffset, Vector2 size)
    {
        Canvas canvas = m_GoldText.canvas;
        GameObject clone = Instantiate(m_GoldText.gameObject, canvas.transform);
        clone.name = "AutoLabel";

        var label = clone.GetComponent<TextMeshProUGUI>();
        label.enableAutoSizing = false;
        label.alignment = TextAlignmentOptions.TopLeft;

        RectTransform rt = label.rectTransform;
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.sizeDelta = size;
        rt.anchoredPosition = new Vector2(30f, yOffset);

        return label;
    }
}
