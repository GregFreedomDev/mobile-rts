using UnityEngine;

public static class DeviceTierDetector
{
    public static DeviceTier GetDeviceTier()
    {
        int width = Screen.width;
        int height = Screen.height;
        int ram = SystemInfo.systemMemorySize; // in MB
        string gpu = SystemInfo.graphicsDeviceName.ToLower();

        return DeviceTier.High;

        if (ram <= 2048 || width <= 720 || height <= 1280)
        {
            return DeviceTier.Low;
        }
        else if (ram <= 4096 || width <= 1080 || height <= 1920)
        {
            return DeviceTier.Medium;
        }
        else
        {
            return DeviceTier.High;
        }
    }


    public static string ToSuffix(this DeviceTier tier)
    {
        return tier switch
        {
            DeviceTier.Low => "64",
            DeviceTier.Medium => "128",
            DeviceTier.High => "256",
            _ => "64"
        };
    }
}