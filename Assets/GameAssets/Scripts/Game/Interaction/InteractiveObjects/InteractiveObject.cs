using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

public class InteractiveObject : MonoBehaviour, IInteractable
{
    [Header("InteractiveObject Components")]
    [SerializeField] protected PhotonView photonView;
    [SerializeField] protected SpriteRenderer sprite;
    [SerializeField] protected Collider2D collision;
    [SerializeField] protected Collider2D trigger;

    [Header("InteractiveObject Events")]
    [SerializeField] protected UnityEvent onInteraction;
    
    protected Color prevColor = Color.white;
    protected virtual bool available => true;

    public Collider2D Collision => collision;
    public Collider2D Trigger => trigger;
    public UnityEvent OnInteraction => onInteraction;
    public SpriteRenderer Sprite => sprite;

    public Vector3 ActionPosition => new Vector3(transform.position.x, trigger.bounds.center.y + trigger.bounds.extents.y, 0);
    public MasterManager.Input InputButton => MasterManager.Input.Action1;

    private void Awake()
    {
        prevColor = sprite ? sprite.color : Color.white;
    }

    protected void OnTriggerEnter2D(Collider2D other)
    {
        if (!MasterManager.ItsPlayerTag(other.tag)
            || !available || !other.isTrigger) return;

        Player player = other.GetComponent<Player>();
        if (!player?.IsMine ?? true) return;

        InteractionManager.BaseInstance.AddInteractable(this);
    }

    protected void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag != MasterManager.CustomTagNames.PlayerCamper &&
            other.tag != MasterManager.CustomTagNames.PlayerCatcher) return;

        Player player = other.GetComponent<Player>();
        if(!player?.IsMine ?? true) return;

        InteractionManager.BaseInstance.RemoveInteractiveObject(this);
    }

    public void TriggerRefresh()
    {
        if (trigger?.enabled ?? false)
        {
            trigger.enabled = false;
            trigger.enabled = true;
        }
    }

    public virtual void Interaction()
    {
        InteractionManager.BaseInstance.RemoveInteractiveObject(this);

        if (GameController.LocalPlayer is PlayerCamper camper)
        {
            InteractionCamper(camper);
        }
        else if (GameController.LocalPlayer is PlayerCatcher catcher)
        {
            InteractionCatcher(catcher);
        }

        onInteraction?.Invoke();
    }

    public void SetInteractionActive(bool active)
    {
        if (sprite == null) return;
        sprite.color = active ? Color.yellow : prevColor;
    }

    protected virtual void InteractionCamper(PlayerCamper player) { }
    protected virtual void InteractionCatcher(PlayerCatcher player) { }
}
