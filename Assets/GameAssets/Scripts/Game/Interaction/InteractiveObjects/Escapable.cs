using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Escapable : InteractiveObject
{
    [SerializeField] private List<Task> tasks;
    [SerializeField] private int campersUsed;
    [SerializeField] private int campersCapacity;
    [SerializeField] private UnityEvent onSomeoneEscaped;
    public List<Task> Tasks => tasks;
    public int CampersCapacity => campersCapacity;
    public int CampersUsed => campersUsed;
    public bool IsFree => campersCapacity == 0 || campersUsed < campersCapacity;
    public UnityEvent OnSomeoneEscaped => onSomeoneEscaped;

    public bool IsEscapable
    {
        get
        {
            foreach (var task in tasks)
            {
                if (!task.IsDone) return false;
            }
            return true;
        }
    }

    protected override bool available
    {
        get
        {
            switch (GameController.LocalRole)
            {
                case Character.CharacterType.Camper:
                    return IsEscapable && IsFree;
                default:
                    return false;
            }
        }
    }

    private void Start()
    {
        trigger.enabled = false;
    }

    protected override void InteractionCamper(PlayerCamper player)
    {
        player.Escaped();
        player.DropPickedItem();
        player.transform.position = this.transform.position;
        player.SetMineState(PlayerCamper.StateTypes.Escaped);

        GameController.Instance.Escape();
        photonView.RPC("RPC_Escape", RpcTarget.AllBuffered, player.PhotonView.ViewID);

        GameController.Instance.AddMatches("Escaped", 50);
    }

    public void TaskDone(Task task)
    {
        if (IsEscapable) trigger.enabled = true;
    }

    [PunRPC]
    private void RPC_Escape(int viewID)
    {
        PlayerCamper foundPlayer = GameController.Instance.FindPlayerByPhotonView<PlayerCamper>(viewID);
        if (foundPlayer != null)
        {
            foundPlayer.Escaped();
            campersUsed++;
            onSomeoneEscaped?.Invoke();
        }
    }
}
