using Photon.Pun;
using UnityEngine;

public class InteractiveHiding : InteractiveObject
{
    [Header("Hiding Info")]
    [SerializeField, ReadOnly] private PlayerCamper hider;
    [SerializeField, ReadOnly] private Vector3 prevPos;

    protected override bool available
    {
        get 
        {
            switch (GameController.LocalRole)
            {
                case Character.CharacterType.Camper: 
                    return hider == null;
                case Character.CharacterType.Catcher: 
                    return GameController.Phase == GameController.PhaseState.Attack;
                default:
                    return false;
            }
        }
    }

    protected override void InteractionCamper(PlayerCamper player)
    {
        prevPos = player.transform.position;
        SetHider(player);
        InteractionCamperManager.Instance.SetHiding(this);
        player.Movement.AddBlockReason("hide");
        photonView.RPC("RPC_SetHider", RpcTarget.OthersBuffered, player.PhotonView.ViewID);
    }

    protected override void InteractionCatcher(PlayerCatcher player)
    {
        hider?.Interaction();
    }

    private void SetHider(PlayerCamper player)
    {
        hider = player;
        hider.HideCamper(this);
        
    }

    public void Leave()
    {
        if (hider.IsMine)
        {
            RemoveHider();
            InteractionCamperManager.Instance.RemoveHiding(this);
            GameController.LocalPlayer.Movement.RemoveBlockReason("hide");
            photonView.RPC("RPC_RemoveHider", RpcTarget.OthersBuffered);
        }
    }

    private void RemoveHider()
    {
        hider.transform.position = prevPos;
        hider.UnhideCamper();
        hider = null;
    }

    [PunRPC]
    private void RPC_SetHider(int viewID)
    {
        PlayerCamper foundPlayer = GameController.Instance.FindPlayerByPhotonView<PlayerCamper>(viewID);
        if (foundPlayer != null)
        {
            SetHider(foundPlayer);
        }
    }
    [PunRPC]
    private void RPC_RemoveHider()
    {
        RemoveHider();
    }
}
