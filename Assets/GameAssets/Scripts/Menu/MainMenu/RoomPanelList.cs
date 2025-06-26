using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

public class RoomPanelList : MonoBehaviourPunCallbacks
{
    [SerializeField] private RoomPanel roomPanel;
    [SerializeField] private Transform panelContent;

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("RoomListUpdate");
        ClearList();

        foreach(RoomInfo room in roomList)
        {
            if(room.IsOpen && !room.RemovedFromList)
            {
                AddRoom(room);
            }
        }
    }

    private void ClearList()
    {
        foreach (Transform panel in panelContent)
        {
            Destroy(panel.gameObject);
        }
    }

    private void AddRoom(RoomInfo room)
    {
        RoomPanel panel = Instantiate(roomPanel, panelContent);
        if (panel == null) return;

        panel.Initialize(room);
    }
}
