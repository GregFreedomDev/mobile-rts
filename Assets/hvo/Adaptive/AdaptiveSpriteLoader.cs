using UnityEngine;

public static class AdaptiveSpriteLoader
{
    public static Sprite Load(string folder, string baseName)
    {
        DeviceTier tier = DeviceTierDetector.GetDeviceTier();
        string path = $"{folder}/{baseName}_{tier.ToSuffix()}";

        Sprite sprite = Resources.Load<Sprite>(path);

        if (sprite == null)
        {
            Debug.LogWarning($"[AdaptiveSpriteLoader] Sprite not found at: {path}. Falling back to Low tier.");
            sprite = Resources.Load<Sprite>($"{folder}/{baseName}_64");
        }

        return sprite;
    }
}