using UnityEngine;

public class CoroutineRunner : MonoBehaviour
{
    public static CoroutineRunner Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // Persistence is already handled. Remove only THIS component — never Destroy the
            // GameObject, because we may be sharing it with another component (e.g. GameManager).
            Destroy(this);
            return;
        }

        // If this runner shares its GameObject with other components (Transform + this + extras),
        // don't DontDestroyOnLoad it: that would keep the whole GameObject (GameManager) alive
        // across scene loads. Instead spin up a dedicated persistent host and drop this component.
        bool sharesGameObject = GetComponents<Component>().Length > 2;
        if (sharesGameObject)
        {
            new GameObject(nameof(CoroutineRunner)).AddComponent<CoroutineRunner>();
            Destroy(this);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
