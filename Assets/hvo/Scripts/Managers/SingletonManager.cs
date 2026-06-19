


using UnityEngine;

public abstract class SingletonManager<T> : MonoBehaviour where T: MonoBehaviour
{
    protected virtual void Awake()
    {
        T[] managers = FindObjectsByType<T>(FindObjectsSortMode.None);
        if (managers.Length > 1)
        {
            Destroy(gameObject);
            return;
        }
    }

    public static T Get()
    {
        var tag = typeof(T).Name;
        GameObject managerObject = GameObject.FindWithTag(tag);
        if (managerObject != null)
        {
            var component = managerObject.GetComponent<T>();
            if (component != null) return component;
        }

        // Tag missing/misconfigured, or the tagged object lacks the component:
        // find any existing instance by type instead of creating a broken one.
        var existing = FindAnyObjectByType<T>();
        if (existing != null) return existing;

        // None exists. Auto-create only for concrete types — AddComponent on an
        // abstract type (e.g. BaseGameManager) returns null and leaves a junk object.
        if (typeof(T).IsAbstract) return null;

        GameObject go = new(tag);
        go.tag = tag;
        return go.AddComponent<T>();
    }
}
