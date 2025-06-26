using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SpectatePanel : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI textNickname;

    private List<PlayerCamper> AvailableCampers => GameController.AvailableCampers;
    private int index;
    private PlayerCamper Current => AvailableCampers[index];

    public void Activate()
    {
        if (AvailableCampers.Count == 0) return;

        panel.SetActive(true);
        Spectate();
    }

    public void Next()
    {
        index++;
        if (index >= AvailableCampers.Count) index = 0;
        Spectate();
    }

    public void Prev()
    {
        index--;
        if (index < 0) index = AvailableCampers.Count - 1;
        Spectate();
    }

    private void Spectate()
    {
        CameraMovement.Instance.SetTarget(Current.CameraTarget.transform, false);
        textNickname.text = Current.Owner.NickName;
    }
}
