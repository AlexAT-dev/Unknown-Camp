using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

public class InteractiveItem : InteractiveObject
{
    [Header("Item Info")]
    [SerializeField] private Item item;
    [SerializeField] private float weight;
    [SerializeField] private UnityEvent onUse;
    [Header("Read Only")]
    [SerializeField, ReadOnly] private PlayerCamper owner;
    [SerializeField, ReadOnly] private GameObject parent;

    public Item Item => item;
    private void Start()
    {
        parent = transform.parent?.gameObject;
    }

    protected override bool available
    {
        get
        {
            switch (GameController.LocalRole)
            {
                case Character.CharacterType.Camper:
                    return owner == null;
                default:
                    return false;
            }
        }
    }

    protected override void InteractionCamper(PlayerCamper player)
    {
        if (player.PickedItem != null)
        {
            ProgressBar.Instance.StopProgress();
            player.PickedItem.Drop(transform.position);
        }

        player.TriggerRefresh();
        player.Movement.AddSpeedMultiplayer("PickedItem", -Mathf.Max(0, weight - GameController.LocalCamperTraits.strength) * 0.2f);
        SetOwner(player);
        photonView.RPC("RPC_SetOwner", RpcTarget.OthersBuffered, player.PhotonView.ViewID);
    }

    private void SetOwner(PlayerCamper player)
    {
        owner = player;
        owner.PickItem(this);

        transform.SetParent(owner.BoneHand);
        transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.Euler(0, 0, 110));
        transform.localScale = Vector3.one;
        trigger.enabled = false;
        if (collision) collision.enabled = false;
    }

    public void Drop(Vector3 pos)
    {
        owner.TriggerRefresh();
        owner.Movement?.RemoveSpeedMultiplayer("PickedItem");
        DropItem(pos);
        photonView.RPC("RPC_DropItem", RpcTarget.OthersBuffered, pos);
    }

    private void DropItem(Vector3 pos)
    {
        owner.RemovePickedItem();
        owner = null;
        transform.SetParent(parent?.transform, false);
        transform.localPosition = pos;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
        trigger.enabled = true;
        if(collision) collision.enabled = true;
    }

    public void Use()
    {
        photonView.RPC("RPC_Use", RpcTarget.AllBuffered);
        owner.Movement?.RemoveSpeedMultiplayer("PickedItem");
        InteractionCamperManager.Instance.RemoveItem();
    }

    [PunRPC]
    private void RPC_SetOwner(int viewID)
    {
        PlayerCamper foundPlayer = GameController.Instance.FindPlayerByPhotonView<PlayerCamper>(viewID);
        if (foundPlayer != null)
        {
            SetOwner(foundPlayer);
        }
    }
    [PunRPC]
    private void RPC_DropItem(Vector3 pos)
    {
        DropItem(pos);
    }
    [PunRPC]
    private void RPC_Use()
    {
        onUse?.Invoke();
        owner.RemovePickedItem();
        Destroy(gameObject);
    }
}
