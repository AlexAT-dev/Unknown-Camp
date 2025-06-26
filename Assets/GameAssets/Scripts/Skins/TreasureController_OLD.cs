using UnityEngine.UI;
using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class TreasureController_OLD : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private TextMeshProUGUI matchesCount;
    [SerializeField] private TextMeshProUGUI boxmatchesCount;
    [SerializeField] private GameObject panel;
    [SerializeField] private GameObject warningPanel;
    [SerializeField] private GameObject errorPanel;
    [SerializeField] private GameObject thanksPanel;
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private Image itemImage;

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
        int thanks = PlayerPrefs.GetInt("thanks", 0);
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
        matchesCount.text = PlayerPrefs.GetInt("matches").ToString();
        boxmatchesCount.text = PlayerPrefs.GetInt("boxmatches").ToString();
    }

    public void StartFire()
    {
        int boxmatches = PlayerPrefs.GetInt("boxmatches");
        if (boxmatches >= 1)
        {
            PlayerPrefs.SetInt("boxmatches", boxmatches - 1);
            UpdateValues();
            Fire();
        }
        else
        {
            ShowWarning();
        }
    }

    public void StartFire2()
    {
        if (TryBuyBoxmatches())
        {
            StartFire();
        }
    }

    public bool TryBuyBoxmatches()
    {
        int matches = PlayerPrefs.GetInt("matches");
        int boxmatches = PlayerPrefs.GetInt("boxmatches");
        if (matches >= 45)
        {
            PlayerPrefs.SetInt("matches", matches - 45);
            PlayerPrefs.SetInt("boxmatches", boxmatches + 1);
            UpdateValues();
            return true;
        }
        else
        {
            ShowError();
            return false;
        }
    }

    public void BuyBoxmatches()
    {
        TryBuyBoxmatches();
    }

    private void Fire()
    {
        panel.SetActive(true);
        SkinItem item = treasure.GetRandomItem();
        itemName.text = item.Name;
        itemName.color = Colors[(int)item.Rarity];
        itemImage.sprite = item.StandSprite;

        if (item.CodeName != "") PlayerPrefs.SetInt(item.CodeName, 1);
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
        PlayerPrefs.SetInt("matches", PlayerPrefs.GetInt("matches") + 100);
        UpdateValues();
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
