

using UnityEngine;
using TMPro;

public class ResourceDataUI: MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_GoldText;
    [SerializeField] private TextMeshProUGUI m_WoodText;
    [SerializeField] private TextMeshProUGUI m_PopulationText; // optional; auto-created if left empty

    public void UpdateResourceDisplay(int gold, int wood)
    {
        m_GoldText.text = gold.ToString();
        m_WoodText.text = wood.ToString();
    }

    public void UpdatePopulation(int current, int max)
    {
        if (m_PopulationText == null)
            m_PopulationText = CreatePopulationLabel();

        m_PopulationText.text = $"Pob: {current}/{max}";
    }

    // Clones the gold label (to inherit font/material/size) and pins it to the top-left corner,
    // so a population counter appears with no extra Editor setup. Assign m_PopulationText in the
    // Inspector to place it precisely instead.
    private TextMeshProUGUI CreatePopulationLabel()
    {
        Canvas canvas = m_GoldText.canvas;
        GameObject clone = Instantiate(m_GoldText.gameObject, canvas.transform);
        clone.name = "PopulationText";

        var label = clone.GetComponent<TextMeshProUGUI>();
        label.enableAutoSizing = false;
        label.alignment = TextAlignmentOptions.TopLeft;

        RectTransform rt = label.rectTransform;
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.sizeDelta = new Vector2(220f, 50f);
        rt.anchoredPosition = new Vector2(30f, -30f);

        return label;
    }
}
