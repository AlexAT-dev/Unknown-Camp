using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamper : Player, IInteractable
{
    public enum StateTypes
    {
        Default,
        Caught,
        Left,
        Eliminated,
        Escaped
    }

    public readonly static List<StateTypes> PermanentStates = new() {StateTypes.Left, StateTypes.Escaped, StateTypes.Eliminated};

    [Header("Camper props")]
    [SerializeField] private InteractiveItem pickedItem;
    [SerializeField] private Transform boneHand;
    [SerializeField] private StateTypes state;

    private bool isHiding = false;

    public InteractiveItem PickedItem => pickedItem;
    public Transform BoneHand => boneHand;
    public StateTypes State => state;
    public bool IsHiding => isHiding;
    public CharacterCamper CharacterCamper => character as CharacterCamper;
    public override Character.CharacterType Role => Character.CharacterType.Camper;

    public int Hp => (int)photonView.Owner.CustomProperties["Hp"];
    public bool InPermanentState => PermanentStates.Contains(state);

    public Vector3 ActionPosition => cameraTarget.transform.position + new Vector3(0, 1.5f, 0);
    public MasterManager.Input InputButton => MasterManager.Input.Action3;

    public void SetInteractionActive(bool active)
    {
        Color color = active ? Color.yellow : Color.white;

        foreach(var sprite in sprites)
        {
            sprite.color = color;
        }
    }

    public void Interaction()
    {
        if (GameController.IsCatcher)
        {
            if ((int)Owner.CustomProperties["Hp"] == 1)
            {
                ActionPanel.Instance.AddSouls(30);
                GameController.Instance.AddMatches("Eliminated Campers", 20);
            }
            else
            {
                ActionPanel.Instance.AddSouls(10);
                GameController.Instance.AddMatches("Catched Campers", 10);
            }

            photonView.RPC("RPC_Catched", RpcTarget.OthersBuffered);
        }
        
    }

    #region Items
    public void PickItem(InteractiveItem pickedItem)
    {
        this.pickedItem = pickedItem;

        if (IsMine)
        {
            InteractionCamperManager.Instance.SetItem(pickedItem);
        }
    }

    public void RemovePickedItem()
    {
        pickedItem = null;
    }

    public void DropPickedItem()
    {
        pickedItem?.Drop(transform.position + new Vector3(flipVisual ? 1f : -0.5f, 0.6f, 0));
        if (IsMine)
        {
            InteractionCamperManager.Instance.RemoveItem();
        }
    }
    #endregion

    public void FindBoneHand()
    {
        boneHand = characterObject.transform.Find("BonePelvis/BoneStomach/BoneChest/BoneShoulderR/BoneArmR/BoneForearmR/BoneHandR");
    }

    public void SetMineState(StateTypes state)
    {
        if (!IsMine || PermanentStates.Contains(this.state)) return;
        PunManager.SetPlayerProperties("State", state);
    }

    public void SyncState()
    {
        state = (StateTypes)photonView.Owner.CustomProperties["State"];
    }

    public void Eliminated()
    {
        Hide();
        Disable();

        if (IsMine)photonView.RPC("RPC_Eliminated", RpcTarget.OthersBuffered);
	}

    public void HideCamper(InteractiveHiding hiding)
    {
        isHiding = true;

        visual.SetActive(false);
        if (collision) collision.enabled = false;
        trigger.enabled = false;
        transform.position = hiding.transform.position;
    }

    public void UnhideCamper()
    {
        isHiding = false;

        if (collision) collision.enabled = true;
        trigger.enabled = true;
        visual.SetActive(true);
    }

    public void Escaped()
    {
        Disable();
    }

    public override void LeftRoom()
    {
        if (PhotonNetwork.IsMasterClient) DropPickedItem();

        int index = GameController.Campers.FindIndex(camper => camper && camper == this);
        if (index != -1)
            GameController.Campers[index] = null;

        CamperIconList.Instance.SetLeftIcon(this);
        base.LeftRoom();
    }

    #region PunRPC
    [PunRPC]
    private void RPC_Catched()
    {
        if (IsMine)
        {
            InteractionCamperManager.Hiding?.Leave();
            DropPickedItem();
            GameController.Instance.CatchLocalCamper();
        }
        else
        {
            GameController.GameEvents.CamperCatched(this);
        }
    }

    [PunRPC]
    private void RPC_Eliminated()
    {
        Eliminated();
    }
    #endregion

}
