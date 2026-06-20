using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Global Build UI built at runtime: a "Construir" button that opens a catalog of buildings
/// (icon + name + info button). The info button shows the building's cost/time/description.
/// Selecting a building hands it back via the onSelect callback to start placement.
/// </summary>
public class BuildMenu : MonoBehaviour
{
    private static readonly Color PanelColor = new Color(0.12f, 0.12f, 0.14f, 0.95f);
    private static readonly Color ItemColor = new Color(0.22f, 0.22f, 0.26f, 0.95f);
    private static readonly Color AccentColor = new Color(0.85f, 0.7f, 0.25f, 1f);
    private static readonly Color TextColor = new Color(0.95f, 0.95f, 0.95f, 1f);

    private Canvas m_Canvas;
    private TMP_FontAsset m_Font;
    private Action<BuildActionSO> m_OnSelect;
    private IReadOnlyList<BuildActionSO> m_Buildings;

    private GameObject m_Catalog;
    private GameObject m_InfoPopup;
    private bool m_Open;

    public void Initialize(IReadOnlyList<BuildActionSO> buildings, Canvas canvas, Action<BuildActionSO> onSelect)
    {
        m_Buildings = buildings;
        m_Canvas = canvas;
        m_OnSelect = onSelect;
        m_Font = FindFont();

        CreateBuildButton();
        CreateCatalog();
        ToggleCatalog(false);
    }

    private TMP_FontAsset FindFont()
    {
        var anyText = FindAnyObjectByType<TextMeshProUGUI>();
        if (anyText != null && anyText.font != null) return anyText.font;
        return TMP_Settings.defaultFontAsset;
    }

    private void CreateBuildButton()
    {
        var btn = CreateButton("BuildButton", m_Canvas.transform, new Vector2(130f, 52f),
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(20f, 20f), AccentColor);
        AddLabel(btn.transform, "Construir", 22f);
        btn.onClick.AddListener(() => ToggleCatalog(!m_Open));
    }

    private const float CardW = 180f;
    private const float CardH = 210f;
    private const float Gap = 16f;

    private void CreateCatalog()
    {
        // Horizontal row of cards, centered on screen.
        float panelW = m_Buildings.Count * CardW + (m_Buildings.Count + 1) * Gap;
        float panelH = CardH + 2f * Gap;

        m_Catalog = CreatePanel("BuildCatalog", m_Canvas.transform, new Vector2(panelW, panelH),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, PanelColor);

        for (int i = 0; i < m_Buildings.Count; i++)
            CreateCatalogItem(m_Buildings[i], i);
    }

    private void CreateCatalogItem(BuildActionSO building, int index)
    {
        // Cards laid out left-to-right, vertically centered in the panel.
        float x = Gap + index * (CardW + Gap);
        var item = CreateButton($"Item_{building.ActionName}", m_Catalog.transform, new Vector2(CardW, CardH),
            new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(x, 0f), ItemColor);

        // icon (top)
        var icon = CreateImage("Icon", item.transform, new Vector2(120f, 120f),
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -14f));
        icon.sprite = building.Icon;
        icon.preserveAspect = true;
        icon.raycastTarget = false;

        // name (bottom)
        var name = AddLabel(item.transform, building.ActionName, 20f);
        Anchor(name.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(6f, 12f), new Vector2(-6f, 58f));
        name.alignment = TextAlignmentOptions.Center;

        // select building
        item.onClick.AddListener(() =>
        {
            ToggleCatalog(false);
            m_OnSelect?.Invoke(building);
        });

        // info button (top-right corner)
        var info = CreateButton("Info", item.transform, new Vector2(26f, 26f),
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-4f, -4f), AccentColor);
        AddLabel(info.transform, "i", 16f);
        info.onClick.AddListener(() => ShowInfo(building));
    }

    private void ShowInfo(BuildActionSO building)
    {
        if (m_InfoPopup != null) Destroy(m_InfoPopup);

        var popup = CreatePanel("BuildInfo", m_Canvas.transform, new Vector2(340f, 250f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, PanelColor);
        m_InfoPopup = popup;

        var title = AddLabel(popup.transform, building.ActionName, 24f);
        Anchor(title.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(12f, -44f), new Vector2(-12f, -8f));
        title.alignment = TextAlignmentOptions.Left;
        title.color = AccentColor;

        string body =
            $"Oro: {building.GoldCost}\n" +
            $"Madera: {building.WoodCost}\n" +
            $"Tiempo: {building.ConstructionTime:0}s\n\n" +
            (string.IsNullOrEmpty(building.Description) ? "" : building.Description);

        var text = AddLabel(popup.transform, body, 18f);
        Anchor(text.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(12f, 12f), new Vector2(-12f, -50f));
        text.alignment = TextAlignmentOptions.TopLeft;

        var close = CreateButton("Close", popup.transform, new Vector2(28f, 28f),
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-4f, -4f), new Color(0.7f, 0.25f, 0.25f, 1f));
        AddLabel(close.transform, "X", 16f);
        close.onClick.AddListener(() => Destroy(popup));
    }

    private void ToggleCatalog(bool open)
    {
        m_Open = open;
        if (m_Catalog != null) m_Catalog.SetActive(open);
        if (!open && m_InfoPopup != null) Destroy(m_InfoPopup);
    }

    // ---- UI building helpers ----

    private GameObject CreatePanel(string name, Transform parent, Vector2 size,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = pivot;
        rt.sizeDelta = size; rt.anchoredPosition = anchoredPos;
        go.GetComponent<Image>().color = color;
        return go;
    }

    private Button CreateButton(string name, Transform parent, Vector2 size,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Color color)
    {
        var go = CreatePanel(name, parent, size, anchorMin, anchorMax, pivot, anchoredPos, color);
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = go.GetComponent<Image>();
        return btn;
    }

    private Image CreateImage(string name, Transform parent, Vector2 size,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = pivot;
        rt.sizeDelta = size; rt.anchoredPosition = anchoredPos;
        return go.GetComponent<Image>();
    }

    private TextMeshProUGUI AddLabel(Transform parent, string content, float fontSize)
    {
        var go = new GameObject("Label", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

        var label = go.AddComponent<TextMeshProUGUI>();
        if (m_Font != null) label.font = m_Font;
        label.text = content;
        label.fontSize = fontSize;
        label.color = TextColor;
        label.alignment = TextAlignmentOptions.Center;
        label.raycastTarget = false;
        return label;
    }

    private static void Anchor(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin; rt.offsetMax = offsetMax;
    }
}
