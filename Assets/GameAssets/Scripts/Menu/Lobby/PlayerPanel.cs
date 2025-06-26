using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textNickName;
    [SerializeField] private Image characterFrame;
    [SerializeField] private Image characterPicture;
    [SerializeField] private GameObject masterIcon;
    [SerializeField] private Button btnKickPlayer;

    private Character character;
    private Photon.Realtime.Player player;
    public Photon.Realtime.Player Player => player;

    private readonly Color32 COLOR_CAMPER = new Color32(49, 76, 122, 255);
    private readonly Color32 COLOR_CATCHER = new Color32(104, 57, 53, 255);

    public void Initialize(Photon.Realtime.Player player)
    {
        this.player = player;

        btnKickPlayer.onClick.AddListener(() =>
        {
            if(PhotonNetwork.LocalPlayer.IsMasterClient)
                PhotonNetwork.CloseConnection(player);
        });

        UpdatePanel();
    }

    public void UpdatePanel()
    {
        character = MasterManager.CharactersList.FindCharacter(player.CustomProperties["Character"]?.ToString());
        SkinItem skin = character.FindSkin(player.CustomProperties["Skin"]?.ToString());

        textNickName.text = player.NickName;
        if (player.IsLocal) textNickName.color = Color.yellow;
        characterFrame.color = character.Type == Character.CharacterType.Camper ? COLOR_CAMPER : COLOR_CATCHER; 
        characterPicture.sprite = skin ? skin.IconSprite : character.IconSprite;
        masterIcon.SetActive(player.IsMasterClient);
        btnKickPlayer.gameObject.SetActive(PhotonNetwork.LocalPlayer.IsMasterClient && !player.IsLocal);
    }
}
