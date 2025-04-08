using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController
{
    private float m_PanSpeed;
    private float m_MobilePanSpeed;
    private bool m_IsDragging;

    public bool LockCamera { get; set; }

    public CameraController(float panSpeed, float mobilePanSpeed)
    {
        m_PanSpeed = panSpeed;
        m_MobilePanSpeed = mobilePanSpeed;
    }

    public void SetDragging(bool isDragging)
    {
        m_IsDragging = isDragging;
    }

    public void Update()
    {
        if (LockCamera || m_IsDragging) return;

        // No mover la cámara si el mouse está sobre un elemento de UI
        if (HvoUtils.IsPointerOverUIElement()) return;

        if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            Vector2 touchDeltaPosition = Input.GetTouch(0).deltaPosition;
            Vector2 normalizedDelta = touchDeltaPosition / new Vector2(Screen.width, Screen.height);
            Camera.main.transform.Translate(
                -normalizedDelta.x * m_MobilePanSpeed,
                -normalizedDelta.y * m_MobilePanSpeed,
                0
            );
        }
        else if (Input.touchCount == 0 && Input.GetMouseButton(0))
        {
            Vector2 mouseDeltaPosition = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

            Camera.main.transform.Translate(
                -mouseDeltaPosition.x * Time.deltaTime * m_PanSpeed,
                -mouseDeltaPosition.y * Time.deltaTime * m_PanSpeed,
                0
            );
        }
    }
}
