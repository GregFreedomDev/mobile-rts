using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class TimeAPIHelper : MonoBehaviour
{
    public static TimeAPIHelper Instance;

    // Offset between trusted server time and the local clock, measured ONCE per session.
    private static TimeSpan s_ServerOffset = TimeSpan.Zero;
    private static bool s_SyncStarted = false;

    /// <summary>Local UTC time corrected by the server offset. Safe to call synchronously and offline.</summary>
    public static DateTime TrustedUtcNow => DateTime.UtcNow + s_ServerOffset;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this); // component only — this may share its GameObject with the GameManager
            return;
        }
        Instance = this;

        // Sync the server offset a single time for the whole session.
        if (!s_SyncStarted)
        {
            s_SyncStarted = true;
            StartCoroutine(SyncServerOffset());
        }
    }

    private IEnumerator SyncServerOffset()
    {
        yield return GetServerTime(
            serverTime => s_ServerOffset = serverTime - DateTime.UtcNow,
            error => Debug.LogWarning($"No se pudo sincronizar la hora del servidor ({error}). Se usará hora local.")
        );
    }

    public IEnumerator GetServerTime(Action<DateTime> onSuccess, Action<string> onError)
    {
        string apiUrl = "https://worldtimeapi.org/api/timezone/Etc/UTC";
        using (UnityWebRequest www = UnityWebRequest.Get(apiUrl))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success)
            {
                var json = www.downloadHandler.text;
                var data = JsonUtility.FromJson<TimeAPIResponse>(json);

                if (!string.IsNullOrEmpty(data.datetime))
                {
                    DateTime serverTime = DateTime.Parse(data.datetime, null, System.Globalization.DateTimeStyles.AdjustToUniversal);
                    Debug.Log($"[SERVER TIME] Parsed serverTime (UTC): {serverTime}");
                    onSuccess?.Invoke(serverTime);
                }
                else
                {
                    onError?.Invoke("Respuesta inválida del servidor");
                }
            }
            else
            {
                onError?.Invoke(www.error);
            }
        }
    }
    [Serializable]
    private class TimeAPIResponse
    {
        public string datetime;
    }
}