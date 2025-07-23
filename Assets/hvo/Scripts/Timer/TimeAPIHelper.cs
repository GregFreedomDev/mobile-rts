using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class TimeAPIHelper : MonoBehaviour
{
    public static TimeAPIHelper Instance;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
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