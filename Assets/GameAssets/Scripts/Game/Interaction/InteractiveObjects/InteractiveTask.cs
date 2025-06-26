using Photon.Pun;
using System;
using UnityEngine;
using UnityEngine.Events;

public class InteractiveTask : InteractiveObject
{
    public enum TaskTypes
    {
        UseItem,
        Itemless,
        Other
    }

    [Header("Task Info")]
    [SerializeField] private Task task;
    [SerializeField] private float useTime;
    [SerializeField] private int progress;
    [SerializeField] private int maxProgress;
    [SerializeField] private TaskTypes taskType;
    [SerializeField] private Item needItem;

    [Header("Task Events")]
    [SerializeField] private UnityEvent onDone;

    //[Header("Read Only")]
    //[SerializeField, ReadOnly] private InteractiveItem currentItem;

    public bool IsCurrent => this == task.CurrentSubtask;
    public bool IsDone => progress >= maxProgress;
    public int Progress => progress;
    public int MaxProgress => maxProgress;
    public TaskTypes TaskType => taskType;

    private int Precent => 50;
    private int Mind => GameController.LocalCamperTraits.mind;
    private float Formula => Precent - Mind * Precent * 0.2f;
    private float UseTimeFormula => useTime + useTime * (Formula / 100f); 

    protected override bool available
    {
        get
        {
            switch (GameController.LocalRole)
            {
                case Character.CharacterType.Camper:
                    return IsCurrent &&
                        !(taskType == TaskTypes.UseItem && GameController.LocalCamper.PickedItem?.Item != needItem);
                default:
                    return false;
            }
        }
    }

    protected override void InteractionCamper(PlayerCamper player)
    {
        if (taskType == TaskTypes.UseItem)
        {
            InteractiveItem pickedItem = GameController.LocalCamper.PickedItem;
            if (pickedItem == null) return;

            ProgressBar.Instance.StartProgress(UseTimeFormula, ActionPosition, true, () =>
            {
                if (pickedItem.Item != needItem) return;

                pickedItem.Use();
                AddProgress(1);
                GameController.Instance.AddMatches("Tasks", 5);
            });
        }
        else if (taskType == TaskTypes.Itemless)
        {
            ProgressBar.Instance.StartProgress(UseTimeFormula, ActionPosition, true, () =>
            {
                AddProgress(1);
                GameController.Instance.AddMatches("Tasks", 5);
            });
        }
    }

    public void AddProgress(int add)
    {
        SetProgress(progress + add);
    }

    public void ProgressFinish()
    {
        SetProgress(maxProgress);
    }

    public void SetProgress(int newProgress)
    {
        if (task.IsPersonal) photonView.RPC("RPC_SetProgress", PhotonNetwork.LocalPlayer, newProgress);
        else photonView.RPC("RPC_SetProgress", RpcTarget.AllBuffered, newProgress);
    }

    public void OnDone()
    {
        onDone?.Invoke();
        task.NextSubtask();
    }

    [PunRPC]
    private void RPC_SetProgress(int newProgress)
    {
        progress = Math.Min(newProgress, maxProgress);
        ActionPanel.Instance.UpdateNode(task);

        if (IsDone)
        {
            OnDone();
        }
    }
}
