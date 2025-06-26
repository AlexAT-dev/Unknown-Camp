using Photon.Pun;
using Photon.Realtime;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CamperIcon : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textNickName;
    [SerializeField] private Image characterPicture;
    [SerializeField] private TextMeshProUGUI textHp;
    [SerializeField] private PlayerCamper.StateTypes state;
    private PlayerCamper camper;
    public PlayerCamper Camper => camper;
    public Character Character => camper.Character;

    public void Initialize(PlayerCamper camper)
    {
        this.camper = camper;
        textNickName.text = camper.Owner.NickName;
        if (camper.IsMine) textNickName.color = Color.yellow;
        characterPicture.sprite = camper.Skin ? camper.Skin.IconSprite : Character.IconSprite;
        UpdateHp();
    }

    public void UpdateHp()
    {
        textHp.text = camper.Owner.CustomProperties["Hp"]?.ToString();
    }

    public void UpdateState()
    {
        if (PlayerCamper.PermanentStates.Contains(state)) return;
        
        state = camper?.State ?? PlayerCamper.StateTypes.Left;
        characterPicture.color = new Color32(255, 255, 255, 255);
        switch (state)
        {
            case PlayerCamper.StateTypes.Default:
                
                break;
            case PlayerCamper.StateTypes.Caught:
                characterPicture.color = new Color32(34, 0, 111, 255);
                break;
            case PlayerCamper.StateTypes.Left:
                characterPicture.sprite = CamperIconList.StateSpriteLeft;
                break;
            case PlayerCamper.StateTypes.Eliminated:
                characterPicture.sprite = CamperIconList.StateSpriteEliminated;
                break;
            case PlayerCamper.StateTypes.Escaped:
                characterPicture.sprite = CamperIconList.StateSpriteEscaped;
                break;
        }
    }

    public void SetLeft()
    {
        camper = null;
        UpdateState();
    }
}
