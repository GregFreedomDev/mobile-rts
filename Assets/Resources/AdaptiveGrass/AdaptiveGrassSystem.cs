using UnityEngine;

public enum DeviceTier
{
    Low,
    Medium,
    High
}

public static class DeviceTierDetector
{
    public static DeviceTier GetDeviceTier()
    {
        int width = Screen.width;
        int height = Screen.height;
        int ram = SystemInfo.systemMemorySize; // in MB
        string gpu = SystemInfo.graphicsDeviceName.ToLower();

        return DeviceTier.Low;

        if ((width < 900 || height < 600) || ram < 3000 || gpu.Contains("intel"))
            return DeviceTier.Low;
        else if ((width < 1600 || height < 1200) || ram < 6000)
            return DeviceTier.Medium;
        else
            return DeviceTier.High;
    }
}

public static class AdaptiveSpriteLoader
{
    public static Sprite Load(string baseName)
    {
        DeviceTier tier = DeviceTierDetector.GetDeviceTier();
        string path = $"AdaptiveGrass/{baseName}_";
        switch (tier)
        {
            case DeviceTier.Low:
                path += "64";
                break;
            case DeviceTier.Medium:
                path += "128";
                break;
            case DeviceTier.High:
                path += "256";
                break;
        }
        Sprite sprite = Resources.Load<Sprite>(path);
        if (sprite == null)
        {
            Debug.LogWarning($"Sprite not found at path: {path}. Falling back to 64px version.");
            sprite = Resources.Load<Sprite>($"AdaptiveGrass/{baseName}_64");
        }
        return sprite;
    }
}