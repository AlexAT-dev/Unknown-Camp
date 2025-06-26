using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

public class InteractiveUseItem : InteractiveObject
{
    [Header("Item Info")]
    [SerializeField] private Item needItem;
    [SerializeField] private bool destroyItem;
    [SerializeField] private float useTime;
    [SerializeField] private int useValue;
    [SerializeField] private int useMax;

    [Header("Events")]
    [SerializeField] protected UnityEvent onDoneSelf;
    [SerializeField] protected UnityEvent onDone;

    protected override bool available
    {
        get
        {
            switch (GameController.LocalRole)
            {
                case Character.CharacterType.Camper:
                    return (useMax == 0 || useValue < useMax) && (needItem == null || GameController.LocalCamper.PickedItem?.Item == needItem);
                default:
                    return false;
            }
        }
    }

    protected override void InteractionCamper(PlayerCamper player)
    {
        ProgressBar.Instance.StartProgress(useTime, ActionPosition, true, () => 
        {
            InteractiveItem pickedItem = GameController.LocalCamper.PickedItem;
            if (pickedItem == null) return;

            if (destroyItem)
            {
                pickedItem.Use();
            }

            GameController.LocalCamper.TriggerRefresh();
            onDoneSelf?.Invoke();
            photonView.RPC("RPC_Done", RpcTarget.AllBuffered);
        });
    }

    [PunRPC]
    private void RPC_Done()
    {
        useValue++;
        onDone?.Invoke();
    }

}
