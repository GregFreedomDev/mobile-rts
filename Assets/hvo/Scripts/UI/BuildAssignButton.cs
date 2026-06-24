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
    private enum Mode { None, Assign, Release }
    private static readonly Color AssignColor = new Color(0.85f, 0.7f, 0.25f, 0.97f);
    private static readonly Color ReleaseColor = new Color(0.8f, 0.32f, 0.3f, 0.97f);

    private Camera m_Cam;
    private RectTransform m_Rect;
    private GameObject m_ButtonGo;
    private Image m_Image;
    private TextMeshProUGUI m_Label;
    private IWorkerAssignable m_Target;
    private Mode m_Mode;
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

        m_Image = m_ButtonGo.GetComponent<Image>();
        m_Image.color = AssignColor;

        var button = m_ButtonGo.GetComponent<Button>();
        button.targetGraphic = m_Image;
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
        m_Mode = Mode.None;
        if (m_Target != null && m_Target.AnchorTransform != null)
        {
            if (m_Target.NeedsWorker) m_Mode = Mode.Assign;
            else if (m_Target.HasWorker) m_Mode = Mode.Release;
        }

        bool show = m_Mode != Mode.None;
        if (m_ButtonGo.activeSelf != show) m_ButtonGo.SetActive(show);
        if (!show) return;

        m_Label.text = m_Mode == Mode.Assign ? m_Target.AssignLabel : "Detener trabajo";
        m_Image.color = m_Mode == Mode.Assign ? AssignColor : ReleaseColor;

        if (m_Cam == null) m_Cam = Camera.main;
        Vector3 below = m_Target.AnchorTransform.position + Vector3.down * 1.2f;
        m_Rect.position = m_Cam.WorldToScreenPoint(below);
    }

    private void OnClick()
    {
        if (m_Target == null) return;

        if (m_Mode == Mode.Assign) m_OnAssign?.Invoke(m_Target);
        else if (m_Mode == Mode.Release) m_Target.ReleaseWorker();
    }
}
