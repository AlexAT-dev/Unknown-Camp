using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class InteractionManager : MonoBehaviour
{
    public static InteractionManager BaseInstance { get; protected set; }
    protected abstract void UpdateInteractiveButton();

    [Header("InteractiveObjects")]
    [SerializeField] protected RectTransform actionButton1;
    [SerializeField] protected RectTransform actionButton2;
    [SerializeField] protected RectTransform actionButton3;
    [Header("Read Only")]
    [SerializeField, ReadOnly] protected List<IInteractable> interactables = new();

    protected IInteractable interactable => interactables.LastOrDefault();

    public void Interact()
    {
        interactable?.Interaction();
    }

    protected RectTransform GetActionButton(MasterManager.Input input)
    {
        return input switch
        {
            MasterManager.Input.Action1 => actionButton1,
            MasterManager.Input.Action2 => actionButton2,
            MasterManager.Input.Action3 => actionButton3,
            _ => null
        };
    }

    protected void ShowActionButton(IInteractable interactable)
    {
        if (Application.isMobilePlatform) return;

        RectTransform actionButton = GetActionButton(interactable.InputButton);

        actionButton.gameObject.SetActive(true);
        actionButton.position = interactable.ActionPosition;
    }

    protected void HideActionButton(MasterManager.Input input)
    {
        if (Application.isMobilePlatform) return;

        RectTransform actionButton = GetActionButton(input);
        actionButton.gameObject.SetActive(false);
    }

    public void AddInteractable(IInteractable other)
    {
        if (interactables.Contains(other)) return;

        interactables.Add(other);
        UpdateInteractiveObject();
    }

    public void RemoveInteractiveObject(IInteractable other)
    {
        if (!interactables.Contains(other)) return;

        interactables.Remove(other);
        other.SetInteractionActive(false);

        UpdateInteractiveObject();
    }

    public void UpdateInteractiveObject()
    {
        if (interactables.Count > 0)
        {
            for (int i = 0; i < interactables.Count - 1; i++)
            {
                interactables[i].SetInteractionActive(false);
            }
            interactable.SetInteractionActive(true);

            ShowActionButton(interactable);
        }
        else
        {
            HideActionButton(MasterManager.Input.Action1);
        }
        UpdateInteractiveButton();
    }
}
