using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI TextName;
    [SerializeField] private Button ButtonJoin;
    private RoomInfo room;
    public RoomInfo Room => room;

    public void Initialize(RoomInfo room)
    {
        this.room = room;
        TextName.text = $"{room.Name} ({room.PlayerCount} / {room.MaxPlayers})";

        ButtonJoin.onClick.AddListener(() =>
        {
            PunManager.JoinRoom(room.Name);
        });
    }
}
