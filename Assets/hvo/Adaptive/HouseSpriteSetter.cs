using UnityEngine;

public class HouseSpriteSetter : MonoBehaviour
{
    [Header("Sprite base name (without _64/128/256 or Off)")]
    public string spriteBaseName = "RTS_New_Home";

    [Header("Is the house turned off?")]
    public bool off = false;

    [Header("Target SpriteRenderer (optional)")]
    public SpriteRenderer targetRenderer;

    void Start()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<SpriteRenderer>();

        if (targetRenderer == null)
        {
            Debug.LogError("No SpriteRenderer found on this GameObject.");
            return;
        }

        // Detectar calidad
        DeviceTier tier = DeviceTierDetector.GetDeviceTier();
        string suffix = tier.ToSuffix();

        // Determinar nombre final del recurso
        string fullName = off ? $"{spriteBaseName}Off_{suffix}" : $"{spriteBaseName}_{suffix}";
        string path = $"House/{fullName}";

        Sprite loaded = Resources.Load<Sprite>(path);

        if (loaded != null)
        {
            targetRenderer.sprite = loaded;
        }
        else
        {
            Debug.LogWarning($"[AdaptiveHouseSpriteSelector] Sprite not found at path: {path}");
        }
    }
}
