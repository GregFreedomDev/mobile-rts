using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using hvo.Scripts.Utils;

public class CameraControllerMobile : MonoBehaviour
{
    public int tileSizePx = 128;
    public int screenWidthPx = 1920;
    public int screenHeightPx = 1080;

    public float dragSpeed = 0.01f;
    public float inertiaDecay = 5f;

    public float minZoom = 2f;
    public float maxZoom = 8f;
    public float zoomSpeed = 0.01f;

    private float minX, maxX;
    private float currentVelocityX = 0f;

    private GameObject highRes;
    private GameObject lowRes;
    private TextMeshProUGUI text;
    private GameObject buttonQuality;
    private QualityTier tier;
    private Camera cam;

    void Start()
    {
        tier = DetectDeviceQuality();
        highRes = GameObject.Find("Tilemap_HighRes");
        lowRes = GameObject.Find("Tilemap_LowRes");
        text = GameObject.Find("Texto").GetComponent<TextMeshProUGUI>();
        buttonQuality = GameObject.Find("QualityButton");


        if (tier == QualityTier.Low || tier == QualityTier.Medium)
        {
            text.text = "Low Quality";
            lowRes.SetActive(true);
            highRes.SetActive(false);
        }
        else
        {
            text.text = "High Quality";
            lowRes.SetActive(false);
            highRes.SetActive(true);
        }

        buttonQuality.GetComponent<Button>().onClick.AddListener(() => UpdateQuality(tier));


        cam = Camera.main;

        // Establecer tamaño ortográfico inicial para 1080 px de alto
        float unitsVisibleY = screenHeightPx / (float)tileSizePx;
        cam.orthographicSize = unitsVisibleY / 2f;

        float unitsVisibleX = screenWidthPx / (float)tileSizePx;

        minX = -14f + unitsVisibleX / 2f;
        maxX = 49f - unitsVisibleX / 2f;

        float totalTiles = 49f - (-14f) + 1f; // = 64
        float centerX = -14f + (totalTiles / 2f) - 0.5f; // = 17.5
        float centerY = 8f / 2f; // = 3.5

        cam.transform.position = new Vector3(centerX, centerY, -10f);
    }

    void Update()
    {
        HandleDrag();
        HandleZoom();
    }

    private void HandleDrag()
    {
        // Don't pan while the player is dragging a unit onto the grid.
        if (DragState.IsDraggingUnit)
        {
            currentVelocityX = 0f;
            return;
        }

        Vector2 delta = Vector2.zero;

        // Touch
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved)
            {
                delta = touch.deltaPosition;
            }
        }

        // Mouse
        if (Application.isEditor && Input.GetMouseButton(0))
        {
            delta = new Vector2(Input.GetAxis("Mouse X"), 0) * 100f;
        }

        // Movimiento + inercia
        if (delta != Vector2.zero)
        {
            currentVelocityX = -delta.x * dragSpeed;
        }
        else
        {
            currentVelocityX = Mathf.Lerp(currentVelocityX, 0, Time.deltaTime * inertiaDecay);
        }

        if (Mathf.Abs(currentVelocityX) > 0.001f)
        {
            Vector3 newPos = cam.transform.position + new Vector3(currentVelocityX, 0, 0);
            newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
            cam.transform.position = newPos;
        }
    }

    private void HandleZoom()
    {
        float zoomChange = 0f;

        // Pinch (Touch)
        if (Input.touchCount == 2)
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            Vector2 prev0 = t0.position - t0.deltaPosition;
            Vector2 prev1 = t1.position - t1.deltaPosition;

            float prevDist = Vector2.Distance(prev0, prev1);
            float currDist = Vector2.Distance(t0.position, t1.position);

            zoomChange = (prevDist - currDist) * zoomSpeed;
        }

        // Scroll (Editor)
        if (Application.isEditor)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            zoomChange = -scroll * 5f;
        }

        if (Mathf.Abs(zoomChange) > 0.001f)
        {
            float newSize = Mathf.Clamp(cam.orthographicSize + zoomChange, minZoom, maxZoom);
            cam.orthographicSize = newSize;
        }
    }


    enum QualityTier
    {
        High,
        Medium,
        Low
    }

    QualityTier DetectDeviceQuality()
    {
        int ram = SystemInfo.systemMemorySize;
        int vram = SystemInfo.graphicsMemorySize;

        if (ram < 3000 || vram < 500)
            return QualityTier.Low;
        else if (ram < 6000 || vram < 1000)
            return QualityTier.Medium;
        else
            return QualityTier.High;
    }

    private void UpdateQuality(QualityTier tier)
    {
        if (tier == QualityTier.High)
        {
            this.tier = QualityTier.Low;
            text.text = "Low Quality";
            lowRes.SetActive(true);
            highRes.SetActive(false);
        }
        else
        {
            this.tier = QualityTier.High;
            text.text = "High Quality";
            lowRes.SetActive(false);
            highRes.SetActive(true);
        }
    }
}