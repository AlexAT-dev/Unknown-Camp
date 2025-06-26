using UnityEngine;

public class SemiTransperentObject : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private Collider2D trigger;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag != MasterManager.CustomTagNames.MyCollider) return;

        sprite.color = new Color(1, 1, 1, 0.5f);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag != MasterManager.CustomTagNames.MyCollider) return;

        sprite.color = new Color(1, 1, 1, 1);
    }
}
