using UnityEngine;

public class CameraController
{
    private float m_PanSpeed;
    private float m_MobilePanSpeed;
    private float m_ZoomSpeed;
    private float m_MinZoom;
    private float m_MaxZoom;

    public bool LockCamera { get; set; }

    private float m_InitialDistance;
    private Vector3 m_InitialPosition;
    private float m_InitialOrthographicSize;

    private bool m_IsZooming = false;
    private Vector2 m_LastMousePosition;

    public CameraController(float panSpeed, float mobilePanSpeed, float zoomSpeed, float minZoom, float maxZoom)
    {
        m_PanSpeed = panSpeed;
        m_MobilePanSpeed = mobilePanSpeed;
        m_ZoomSpeed = zoomSpeed;
        m_MinZoom = minZoom;
        m_MaxZoom = maxZoom;
    }

    public void Update()
    {
        if (LockCamera) return;

        HandlePan();
        HandleZoom();
    }

    private void HandlePan()
    {
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

    private void HandleZoom()
    {
        // Touch-based zooming (for mobile)
        if (Input.touchCount == 2)
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            if (touchZero.phase == TouchPhase.Began || touchOne.phase == TouchPhase.Began)
            {
                m_InitialDistance = Vector2.Distance(touchZero.position, touchOne.position);
                m_InitialOrthographicSize = Camera.main.orthographicSize;
            }
            else if (touchZero.phase == TouchPhase.Moved || touchOne.phase == TouchPhase.Moved)
            {
                float currentDistance = Vector2.Distance(touchZero.position, touchOne.position);
                float difference = currentDistance - m_InitialDistance;

                Zoom(difference * m_ZoomSpeed);
            }
        }

        // Mouse scroll wheel zooming (for additional testing)
        if (Input.mouseScrollDelta.y != 0)
        {
            Zoom(Input.mouseScrollDelta.y * m_ZoomSpeed);
        }
    }

    private void Zoom(float increment)
    {
        if (Camera.main.orthographic)
        {
            Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize - increment, m_MinZoom, m_MaxZoom);
        }
        else
        {
            Camera.main.fieldOfView = Mathf.Clamp(Camera.main.fieldOfView - increment, m_MinZoom, m_MaxZoom);
        }
    }
}
