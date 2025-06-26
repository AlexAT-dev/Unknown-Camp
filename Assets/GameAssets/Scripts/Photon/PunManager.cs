using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;

public class PunManager : MonoBehaviourPunCallbacks
{
    public static PunManager Instance;
    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public static void Connect(string nickName)
    {
        PhotonNetwork.GameVersion = MasterManager.Version;
        PhotonNetwork.NickName = nickName;

        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.EnableCloseConnection = true;
        PhotonNetwork.ConnectUsingSettings();
    }

    public static void CreateRoom(string name, bool isPrivate = false)
    {
        if (!PhotonNetwork.IsConnected) return;
        PhotonNetwork.CreateRoom(name, new RoomOptions()
        {
            MaxPlayers = 8,
            IsVisible = !isPrivate
        });
    }

    public static void JoinRoom(string name)
    {
        if (!PhotonNetwork.IsConnected) return;

        PhotonNetwork.JoinRoom(name);
    }

    public static void SetRoomProperties(object key, object value, Room room = null)
    {
        room ??= PhotonNetwork.CurrentRoom;
        Hashtable hashtable = room.CustomProperties;
        hashtable[key] = value;
        room.SetCustomProperties(hashtable);
    }

    public static void SetPlayerProperties(object key, object value, Photon.Realtime.Player player = null)
    {
        player ??= PhotonNetwork.LocalPlayer;
        Hashtable hashtable = player.CustomProperties;
        hashtable[key] = value;
        player.SetCustomProperties(hashtable);
    }

    public static void SetLocalCamperHpAndState(int hp, PlayerCamper.StateTypes state)
    {
        Hashtable hashtable = PhotonNetwork.LocalPlayer.CustomProperties;
        hashtable["Hp"] = hp;
        hashtable["State"] = (int)state;
        PhotonNetwork.LocalPlayer.SetCustomProperties(hashtable);
    }
}
