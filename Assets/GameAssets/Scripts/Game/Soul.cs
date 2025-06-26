using UnityEngine;

public class Soul : MonoBehaviour
{
    private void Start()
    {
        if (!GameController.Instance || GameController.IsCamper)
            Destroy(gameObject);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        InteractionCatcherManager.Souls.Remove(this);
        ActionPanel.Instance.AddSouls(5);
        Destroy(gameObject);
    }
}
