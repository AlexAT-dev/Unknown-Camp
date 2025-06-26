using UnityEngine;

public interface IInteractable
{
    Vector3 ActionPosition { get; }
    MasterManager.Input InputButton { get; }
    void Interaction();
    void SetInteractionActive(bool active);
}
