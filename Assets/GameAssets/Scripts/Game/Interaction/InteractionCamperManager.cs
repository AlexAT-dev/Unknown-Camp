using UnityEngine;
using UnityEngine.UI;

public class InteractionCamperManager : InteractionManager
{
    public static InteractionCamperManager Instance { get; private set; }
    private void Awake()
    {
        if (BaseInstance == null) BaseInstance = this;
        if (Instance == null) Instance = this;
    }

    [Header("InteractiveObjects")]
    [SerializeField, ReadOnly] private InteractiveHiding hiding;
    [SerializeField, ReadOnly] private InteractiveItem item;

    [Header("UI Interaction Buttons")]
    [SerializeField] private Button buttonInteract;
    [SerializeField] private Button buttonPick;
    [SerializeField] private Button buttonUse;
    [SerializeField] private Button buttonHide;
    [SerializeField] private Button buttonUnhide;
    [SerializeField] private Button buttonEscape;
    private Button[] interactionButtons => new []{buttonInteract, buttonPick, buttonUse, buttonHide, buttonUnhide, buttonEscape};

    [Header("UI Drop Button")]
    [SerializeField] private Button buttonDrop;
    [SerializeField] private Image buttonDropImage;
    [SerializeField] private Sprite defaultDropSprite;

    public static InteractiveHiding Hiding => Instance.hiding;
    public static InteractiveItem Item => Instance.item;

    private void Update()
    {
        if (GameController.isLocalPermanentState) return;
        
        if (hiding != null)
        {
            if (Input.GetButtonUp(hiding.InputButton.ToString()))
            {
                LeaveHiding();
            }
            return;
        }

        if (item != null)
        {
            if (Input.GetButtonUp(MasterManager.Input.Action3.ToString()))
            {
                DropItem();
            }
        }
        
        if (interactable == null) return;
        if (Input.GetButtonUp(interactable.InputButton.ToString()))
        {
            Interact();
        }
    }

    protected override void UpdateInteractiveButton()
    {
        foreach(var button in interactionButtons)
        {
            button.gameObject.SetActive(false);
        }

        if (hiding != null)
        {
            buttonUnhide.gameObject.SetActive(true);
            buttonDrop.interactable = false;
            return;
        }
        if (interactable == null)
        {
            buttonInteract.gameObject.SetActive(true);
            buttonInteract.interactable = false;
            return;
        }
        buttonInteract.interactable = true;
        buttonDrop.interactable = true;

        Button needButton = interactable switch
        {
            InteractiveItem => buttonPick,
            InteractiveHiding => buttonHide,
            InteractiveTask task => task.TaskType == InteractiveTask.TaskTypes.UseItem ? buttonUse : buttonInteract,
            Escapable => buttonEscape,
            _ => buttonInteract,
        };
        needButton.gameObject.SetActive(true);
    }

    public void SetHiding(InteractiveHiding hiding)
    {
        this.hiding = hiding;
        UpdateInteractiveButton();
        hiding.SetInteractionActive(true);
        ShowActionButton(hiding);
    }

    public void RemoveHiding(InteractiveHiding hiding)
    {
        if (this.hiding != hiding) return;

        this.hiding = null;
        UpdateInteractiveButton();
    }

    public void LeaveHiding()
    {
        hiding?.Leave();
    }
    
    private void UpdateDropButton()
    {
        buttonDrop.gameObject.SetActive(item != null);
    }
    
    public void SetItem(InteractiveItem item)
    {
        this.item = item;
        buttonDropImage.sprite = item.Item?.Sprite ?? defaultDropSprite;
        UpdateDropButton();
    }
    
    public void RemoveItem()
    {
        item = null;
        UpdateDropButton();
    }
    
    public void DropItem()
    {
        ProgressBar.Instance.StopProgress();
        GameController.LocalCamper.DropPickedItem();
    }
}
