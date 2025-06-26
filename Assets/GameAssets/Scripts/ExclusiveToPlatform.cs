using UnityEngine;
public class ExclusiveToPlatform : MonoBehaviour
{
    private enum Platform
    {
        Desktop,
        Mobile
    }

    [SerializeField] private Platform platform;

    void Start()
    {
        bool active = (Application.isMobilePlatform && platform == Platform.Mobile)
                || (!Application.isMobilePlatform && platform == Platform.Desktop);

        gameObject.SetActive(active);
    }
}
