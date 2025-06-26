using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class SelectCharacter : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private SelectableButton button;

	public void Initialize(Character character, bool selected)
	{
		image.sprite = character.IconSprite;
		if (selected) button.Select();
        button.OnSelect.AddListener(() =>
        {
            if (character is CharacterCamper camper)
            {
                SelectCharacterController.Instance.SetCamper(camper, this);
            }
            else if (character is CharacterCatcher catcher)
            {
                SelectCharacterController.Instance.SetCatcher(catcher, this);
            }
        });
    }

	public void Select(bool instant)
	{
		button.Select(instant);
	}

	public void Deselect(bool instant)
	{
		button.Deselect(instant);
	}

	public void InvokeSelectEvent()
	{
		button.InvokeSelectEvent();

    }

}
