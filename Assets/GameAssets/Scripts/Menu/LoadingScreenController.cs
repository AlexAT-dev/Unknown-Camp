using TMPro;
using UnityEngine;

public class LoadingScreenController : MonoBehaviour
{
    public static LoadingScreenController Instance;
    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    [SerializeField] private GameObject screen;
    [SerializeField] private TMP_Text text;

    public void Show(string text)
    {
        screen.SetActive(true);
        this.text.text = text;
    }
    public void Show() => Show("Connecting...");

    public void Hide()
    {
        screen.SetActive(false);
    }
}
