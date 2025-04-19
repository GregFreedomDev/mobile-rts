using UnityEngine;

public class CameraResizer : MonoBehaviour
{
    public int gridWidth = 14;
    public int gridHeight = 5;
    public float tileSize = 1f; // tamaño de cada tile (normalmente 1 unidad)

    void Start()
    {
        ResizeCamera();
    }

    void ResizeCamera()
    {
        Camera cam = Camera.main;
        float screenRatio = (float)Screen.width / Screen.height;
        float targetRatio = (float)gridWidth / gridHeight;

        if (screenRatio >= targetRatio)
        {
            // Pantalla más ancha que la grilla → altura es limitante
            cam.orthographicSize = (gridHeight * tileSize) / 2f + 0.5f;
        }
        else
        {
            // Pantalla más alta que la grilla → ancho es limitante
            float differenceInSize = targetRatio / screenRatio;
            cam.orthographicSize = (gridHeight * tileSize) / 2f * differenceInSize + 0.5f;
        }

        // Centrar la cámara en la grilla
        float camX = (gridWidth * tileSize) / 2f - 0.5f;
        float camY = (gridHeight * tileSize) / 2f - 0.5f;
        cam.transform.position = new Vector3(camX, camY, -10f);
    }
}