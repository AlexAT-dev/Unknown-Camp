using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerPanelList : MonoBehaviour
{
    [SerializeField] private PlayerPanel playerPanel;
    [SerializeField] private Transform panelContent;
    
    private List<PlayerPanel> panels = new List<PlayerPanel>();

    public void AddPlayer(Photon.Realtime.Player player)
    {
        PlayerPanel panel = Instantiate(playerPanel, panelContent);
        if (panel == null) return;

        panel.Initialize(player);
        panels.Add(panel);
    }

    public void RemovePlayer(Photon.Realtime.Player player)
    {
        PlayerPanel panel = panels.Find(x => x.Player == player);
        if (panel == null) return;

        panels.Remove(panel);
        Destroy(panel.gameObject);
    }

    public void UpdatePanels()
    {
        foreach (var panel in panels)
        {
            panel.UpdatePanel();
        }
    }
}
