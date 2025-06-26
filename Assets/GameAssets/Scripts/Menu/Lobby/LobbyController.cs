using ExitGames.Client.Photon;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyController : MonoBehaviourPunCallbacks
{
    [Header("Players")]
    [SerializeField] private PlayerPanelList playerPanelList;
    [SerializeField] private Button btnStart;

    [Header("PlayersCount")]
    [SerializeField] private TextMeshProUGUI textPlayersCount;
    [SerializeField] private TextMeshProUGUI textPlayersCountMax;

    [SerializeField] private TextMeshProUGUI textError;

    [Header("Character")]
    [SerializeField] private Image characterImage;
    [SerializeField] private TextMeshProUGUI textCharacterName;

    [Header("Buttons")]
    [SerializeField] private Button btnChangeCharacter;
    [SerializeField] private Button btnChangeRole;
    [SerializeField] private SelectableButton btnRoleCatcher;
    [SerializeField] private SelectableButton btnRoleCamper;
    [SerializeField] private GameObject loadingMessage;

    private Character.CharacterType currentRole;

    public Character.CharacterType CurrentRole => currentRole;

    private void Start()
    {
        btnRoleCamper.Select();
        UpdateStartButton();
        UpdatePlayersCount();
        RenderRoomPlayers();
        UpdateAvailabilityCatcher();
    }

    #region UpdatesMethods
    private void UpdateStartButton()
    {
        btnStart.gameObject.SetActive(PhotonNetwork.IsMasterClient);
    }

    private void UpdatePlayersCount()
    {
        textPlayersCount.text = PhotonNetwork.CurrentRoom.PlayerCount.ToString();
        textPlayersCountMax.text = PhotonNetwork.CurrentRoom.MaxPlayers.ToString();
    }

    private void RenderRoomPlayers()
    {
        foreach(var player in PhotonNetwork.CurrentRoom.Players)
        {
            playerPanelList.AddPlayer(player.Value);
        }
    }
    #endregion

    #region ButtonEvents
    public void StartGame()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        loadingMessage.SetActive(true);
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.LoadLevel(MasterManager.SceneNames.Game);
    }

    public void Leave()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void SetCharacter(Character character, string skinName)
    {
        Hashtable hashtable = PhotonNetwork.LocalPlayer.CustomProperties;
        hashtable["Character"] = character.Name;
        hashtable["Skin"] = skinName;
        PhotonNetwork.LocalPlayer.SetCustomProperties(hashtable);
        UpdateSelectedCharacter(character, skinName);
    }

    public void SetRole(int characterType)
    {
        Hashtable hashtable = PhotonNetwork.LocalPlayer.CustomProperties;
        hashtable["Role"] = characterType;
        currentRole = (Character.CharacterType) characterType;

        Character character = characterType switch
        {
            0 => SelectCharacterController.Camper,
            1 => SelectCharacterController.Catcher,
            _ => null
        };
        hashtable["Character"] = character.Name;
        
        string skinName = characterType switch
        {
            0 => SelectCharacterController.SkinName,
            _ => ""
        };

        hashtable["Skin"] = skinName;
        UpdateSelectedCharacter(character, skinName);
        SelectCharacterController.Instance.UpdateSelected();

        PhotonNetwork.LocalPlayer.SetCustomProperties(hashtable);
    }
    #endregion

    #region Methods
    private void UpdateSelectedCharacter(Character character, string skinName)
    {
        SkinItem skin = character.FindSkin(skinName);
        
        characterImage.sprite = skin ? skin.StandSprite : character.StandSprite;
        textCharacterName.text = character.Name;
    }

    private void UpdateRoomCatcher()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        bool hasCatcher = false;
        foreach(var player in PhotonNetwork.CurrentRoom.Players)
        {
            if ((int?)player.Value.CustomProperties["Role"] == 1)
            {
                hasCatcher = true;
                break;
            }
        }

        PunManager.SetRoomProperties("HasCatcher", hasCatcher);
    }

    #endregion

    #region Callbacks
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        Debug.Log($"player {newPlayer.NickName} enter");
        playerPanelList.AddPlayer(newPlayer);
        UpdatePlayersCount();
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        playerPanelList.RemovePlayer(otherPlayer);
        UpdateStartButton();
        playerPanelList.UpdatePanels();
        UpdatePlayersCount();
        UpdateRoomCatcher();
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(MasterManager.SceneNames.MainMenu);
    }

    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, Hashtable changedProps)
    {
        playerPanelList.UpdatePanels();
        UpdateRoomCatcher();
    }

    private void UpdateAvailabilityCatcher()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties["HasCatcher"] == null)
        {
            PunManager.SetRoomProperties("HasCatcher", false);
        }

        if ((bool) PhotonNetwork.CurrentRoom.CustomProperties["HasCatcher"])
        {
            UpdateStartButton();
            btnChangeRole.interactable = currentRole == Character.CharacterType.Catcher;
            textError.gameObject.SetActive(false);
            btnRoleCatcher.SetIteractable(currentRole == Character.CharacterType.Catcher);
        }
        else
        {
            btnStart.gameObject.SetActive(false);
            btnChangeRole.interactable = true;
            textError.gameObject.SetActive(true);
            btnRoleCatcher.SetIteractable(true);
        }
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        UpdateAvailabilityCatcher();
    }
    #endregion
}
