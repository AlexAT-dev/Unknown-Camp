using ExitGames.Client.Photon;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public class CamperIconList : MonoBehaviour
{
    public static CamperIconList Instance;
    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    [SerializeField] private CamperIcon iconPrefab;
    [SerializeField] private Transform panelContent;
    private List<CamperIcon> icons = new();

    [Header("State Sprites")]
    [SerializeField] private Sprite left;
    [SerializeField] private Sprite eliminated;
    [SerializeField] private Sprite escaped;

    public static Sprite StateSpriteLeft => Instance.left;
    public static Sprite StateSpriteEliminated => Instance.eliminated;
    public static Sprite StateSpriteEscaped => Instance.escaped;

    public void CreateCamper(PlayerCamper camper)
    {
        CamperIcon icon = Instantiate(iconPrefab, panelContent);
        if (icon == null) return;

        icon.Initialize(camper);
        icons.Add(icon);
    }

    public void UpdateCamperIcon(Photon.Realtime.Player targetPlayer)
    {
        CamperIcon icon = icons.Find(x => x.Camper?.Owner == targetPlayer);
        if (icon == null) return;

        icon.UpdateHp();
        icon.UpdateState();
    }

    public void SetLeftIcon(PlayerCamper camper)
    {
        CamperIcon icon = icons.Find(x => x.Camper == camper);
        if (icon == null) return;

        icon.SetLeft();
    }
}
