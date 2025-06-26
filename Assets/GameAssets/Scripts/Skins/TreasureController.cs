using UnityEngine.UI;
using TMPro;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;
using SimpleJSON;
using System;

public class TreasureController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private TextMeshProUGUI matchesCount;
    [SerializeField] private TextMeshProUGUI boxmatchesCount;
    [SerializeField] private GameObject panel;
    [SerializeField] private GameObject warningPanel;
    [SerializeField] private GameObject errorPanel;
    [SerializeField] private GameObject thanksPanel;
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private TextMeshProUGUI dublicate;
    [SerializeField] private Image itemImage;
    [SerializeField] private GameObject clickBlocker;


    [Header("Treasure")]
    [SerializeField] private SkinsTreasure treasure;
    [SerializeField] private List<SkinsTreasure> treasures;
    [SerializeField] private Image skin1;
    [SerializeField] private Image skin2;
    [SerializeField] private Image skin3;

    [Header("TEMP_VALUES")]
    public List<Color> Colors;

    private void Start()
    {
        int thanks = 1; // PlayerPrefs.GetInt("thanks", 0);
        if (thanks == 0)
        {
            thanksPanel.SetActive(true);
            PlayerPrefs.SetInt("boxmatches", PlayerPrefs.GetInt("boxmatches") + 20);
            PlayerPrefs.SetInt("thanks", 1);
        }

        UpdateValues();

        string tmp = "";

        foreach (var item in treasures[0].Items)
        {
            tmp += item.CodeName + "\n";
        }
        
        Debug.Log(tmp);
    }

    private void UpdateValues()
    {
        MasterManager.Instance.UpdateAccount(() =>
        {
            matchesCount.text = MasterManager.LocalAccount.Currency.Matches.ToString();
            boxmatchesCount.text = MasterManager.LocalAccount.Currency.Boxes.ToString();
        });
    }

    public void StartFire()
    {
        clickBlocker.SetActive(true);
        WebRequest.Post(this, $"Account/OpenTreasure/{treasure.Id}", null, (request) =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                JSONNode jsonData = JSON.Parse(request.downloadHandler.text);
                SetUnlockable(jsonData["UnlockableId"], jsonData["Ashes"]);
                UpdateValues();
                clickBlocker.SetActive(false);
            }
            else if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                clickBlocker.SetActive(false);
                MessagePanelController.Instance.Show("Network error", "Could not connect to server.");
            }
            else
            {
                try
                {
                    Debug.Log(request.downloadHandler.text);
                    ServerResponse responce = JsonUtility.FromJson<ServerResponse>(request.downloadHandler.text);
                    string errorMessage = responce.Message;

                    if (errorMessage == "NoTreasure")
                    {
                        ShowWarning();
                    }

                }
                catch (System.Exception ex)
                {
                    MessagePanelController.Instance.Show("Error", ex.Message);

                }
                clickBlocker.SetActive(false);
            }
        });
    }


    public void StartFire2()
    {
        TryBuyBoxmatches(StartFire);
    }

    public void TryBuyBoxmatches(Action onSuccess = null)
    {
        clickBlocker.SetActive(true);
        WebRequest.Post(this, $"Account/BuyMatchBox", null, (request) =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                UpdateValues();
                onSuccess?.Invoke();
                clickBlocker.SetActive(false);
            }
            else if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                clickBlocker.SetActive(false);
                MessagePanelController.Instance.Show("Network error", "Could not connect to server.");
            }
            else
            {
                try
                {
                    Debug.Log(request.downloadHandler.text);
                    ServerResponse responce = JsonUtility.FromJson<ServerResponse>(request.downloadHandler.text);
                    string errorMessage = responce.Message;

                    if (errorMessage == "NoMatches")
                    {
                        ShowError();
                    }

                }
                catch (System.Exception ex)
                {
                    MessagePanelController.Instance.Show("Error", ex.Message);

                }
                clickBlocker.SetActive(false);
            }
        });
    }

    public void BuyBoxmatches()
    {
        TryBuyBoxmatches();
    }

    private void SetUnlockable(string itemID, string ashes)
    {
        SkinItem item = treasure.Items.Find(x => x.CodeName == itemID);
        if (!item) return;

        bool isDublicate = ashes != "0";

        panel.SetActive(true);
        itemName.text = item.Name;
        itemName.color = Colors[(int)item.Rarity];
        itemImage.sprite = item.StandSprite;
        itemImage.color = isDublicate ? Color.gray : Color.white;

        dublicate.gameObject.SetActive(isDublicate);
    }

    private void ShowWarning()
    {
        warningPanel.SetActive(true);
    }

    private void ShowError()
    {
        errorPanel.SetActive(true);
    }

    public void ClosePanel()
    {
        panel.SetActive(false);
    }

    public void CloseWarning()
    {
        warningPanel.SetActive(false);
    }

    public void CloseError()
    {
        errorPanel.SetActive(false);
    }

    public void AddMatches()
    {
        clickBlocker.SetActive(true);
        WebRequest.Post(this, $"Account/AddMatches/200", null, (request) =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                clickBlocker.SetActive(false);
                UpdateValues();
            }
            else if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                clickBlocker.SetActive(false);
                MessagePanelController.Instance.Show("Network error", "Could not connect to server.");
            }
            else
            {
                clickBlocker.SetActive(false);
            }
        });
    }

    public void SetTreasure(string name)
    {
        SkinsTreasure t = treasures.Find(x => x.name == name);
        if (t == null) return;

        treasure = t;

        skin1.sprite = t.Items[0].StandSprite;
        skin2.sprite = t.Items[1].StandSprite;
        skin3.sprite = t.Items[2].StandSprite;
    }
}
