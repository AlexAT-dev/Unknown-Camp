using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class SelectableButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private ColorBlock colors = new ColorBlock()
    {
        normalColor = new Color(0.6f, 0.6f, 0.6f),
        highlightedColor = new Color(0.8f, 0.8f, 0.8f),
        pressedColor = new Color(0.9f, 0.9f, 0.9f),
        selectedColor = new Color(1f, 1f, 1f),
        disabledColor = new Color(0.4f, 0.4f, 0.4f, 0.4f),
    };
    [SerializeField] private Graphic targetGraphic;
    [SerializeField] private bool selectedOnStart;
    [SerializeField, ReadOnly] private bool isSelected;
    [SerializeField] private UnityEvent onSelect;
    [SerializeField] private UnityEvent onDeselect;

    private GameObject parent => transform.parent.gameObject;
    [SerializeField] private bool interactable = true;

    public UnityEvent OnSelect => onSelect;
    public UnityEvent OnDeselect => onDeselect;
    public bool IsSelected => isSelected;
    private void Awake()
    {
        if (selectedOnStart)
        {
            isSelected = true;
        }
        else
        {
            CrossFadeColor(colors.normalColor, true);
        }
    }

    private void OnEnable()
    {
        if (isSelected) CrossFadeColor(colors.selectedColor, true);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Select();
    }

    private void CrossFadeColor(Color color, bool instant)
    {
        targetGraphic.CrossFadeColor(color, instant ? 0f : colors.fadeDuration, true, true);
    }

    public void Select(bool instant = false)
    {
        if (isSelected || !interactable) return;
        isSelected = true;
        CrossFadeColor(colors.selectedColor, instant);

        foreach (SelectableButton button in parent.GetComponentsInChildren<SelectableButton>())
        {
            if(button != this && button.isSelected)
            {
                button.Deselect();
            }
        }
        onSelect?.Invoke();
    }

    public void InvokeSelectEvent()
    {
        if (isSelected) onSelect?.Invoke();
    }

    public void Deselect(bool instant = false)
    {
        isSelected = false;
        CrossFadeColor(colors.normalColor, instant);
        onDeselect?.Invoke();
    }

    public void SetIteractable(bool value)
    {
        interactable = value;

        if (interactable)
        {
            CrossFadeColor(isSelected ? colors.selectedColor : colors.normalColor, true);
        }
        else
        {
            CrossFadeColor(colors.disabledColor, true);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isSelected || !interactable) return;

        CrossFadeColor(colors.highlightedColor, false);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isSelected || !interactable) return;

        CrossFadeColor(colors.normalColor, false);
    }
}
