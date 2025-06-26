using UnityEngine;
using TMPro;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviourPunCallbacks
{
    [SerializeField] private TextMeshProUGUI textUsername;
    [SerializeField] private TextMeshProUGUI textCreateRoom;
    [SerializeField] private TextMeshProUGUI textJoinRoomName;
    [SerializeField] private Toggle privateRoom;
    [SerializeField] private TextMeshProUGUI textError;
    [SerializeField] private GameObject loadingMessage;

    private void Start()
    {
        textUsername.text = PhotonNetwork.NickName;
    }

    public override void OnConnectedToMaster()
    {
        if(!PhotonNetwork.InLobby)
        {
            Debug.Log("Lobby joined!");
            PhotonNetwork.JoinLobby();
        }
    }

    public void CreateRoom()
    {
        loadingMessage.SetActive(true);
        PunManager.CreateRoom(textCreateRoom.text, privateRoom.isOn);
    }

    public void JoinRoom()
    {
        loadingMessage.SetActive(true);
        PunManager.JoinRoom(textJoinRoomName.text);
    }

    public void UpdateRooms()
    {
        PhotonNetwork.JoinLobby();
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    #region Callbacks
    public override void OnCreatedRoom()
    { 
        Debug.Log("Room created");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        loadingMessage.SetActive(false);
        Debug.LogWarning($"Create room failed ({message})[{returnCode}]");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Room joined");
        PhotonNetwork.LoadLevel(MasterManager.SceneNames.Lobby);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        loadingMessage.SetActive(false);
        Debug.LogWarning($"Join room failed ({message})[{returnCode}]");

        textError.gameObject.SetActive(true);
        textError.text = message;
        Invoke(nameof(HideErrorText), 2f);
    }

    private void HideErrorText()
    {
        textError.gameObject.SetActive(false);
    }
    #endregion
}
