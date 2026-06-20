using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// A screen-space button that hovers below the currently selected assignable (a foundation, farm,
/// tree, ...) and, when clicked, dispatches a worker to it. Visible only while the target still
/// needs a worker. The caption adapts to the target ("Asignar trabajador", "Cortar leña", ...).
/// </summary>
public class BuildAssignButton : MonoBehaviour
{
    private Camera m_Cam;
    private RectTransform m_Rect;
    private GameObject m_ButtonGo;
    private TextMeshProUGUI m_Label;
    private IWorkerAssignable m_Target;
    private Action<IWorkerAssignable> m_OnAssign;

    public void Initialize(Canvas canvas, TMP_FontAsset font, Action<IWorkerAssignable> onAssign)
    {
        m_OnAssign = onAssign;
        m_Cam = Camera.main;

        m_ButtonGo = new GameObject("AssignWorkerButton", typeof(RectTransform), typeof(Image), typeof(Button));
        m_ButtonGo.transform.SetParent(canvas.transform, false);

        m_Rect = m_ButtonGo.GetComponent<RectTransform>();
        m_Rect.sizeDelta = new Vector2(190f, 40f);
        m_Rect.anchorMin = m_Rect.anchorMax = new Vector2(0f, 0f);
        m_Rect.pivot = new Vector2(0.5f, 1f); // hangs below the tracked point

        var img = m_ButtonGo.GetComponent<Image>();
        img.color = new Color(0.85f, 0.7f, 0.25f, 0.97f);

        var button = m_ButtonGo.GetComponent<Button>();
        button.targetGraphic = img;
        button.onClick.AddListener(OnClick);

        var labelGo = new GameObject("Label", typeof(RectTransform));
        labelGo.transform.SetParent(m_ButtonGo.transform, false);
        var lrt = labelGo.GetComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
        lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;

        m_Label = labelGo.AddComponent<TextMeshProUGUI>();
        if (font != null) m_Label.font = font;
        m_Label.fontSize = 16f;
        m_Label.alignment = TextAlignmentOptions.Center;
        m_Label.color = new Color(0.1f, 0.1f, 0.1f, 1f);
        m_Label.raycastTarget = false;

        m_ButtonGo.SetActive(false);
    }

    public void Track(IWorkerAssignable assignable) => m_Target = assignable;

    void Update()
    {
        bool show = m_Target != null && m_Target.AnchorTransform != null && m_Target.NeedsWorker;
        if (m_ButtonGo.activeSelf != show) m_ButtonGo.SetActive(show);
        if (!show) return;

        m_Label.text = m_Target.AssignLabel;

        if (m_Cam == null) m_Cam = Camera.main;
        Vector3 below = m_Target.AnchorTransform.position + Vector3.down * 1.2f;
        m_Rect.position = m_Cam.WorldToScreenPoint(below);
    }

    private void OnClick()
    {
        if (m_Target != null) m_OnAssign?.Invoke(m_Target);
    }
}
