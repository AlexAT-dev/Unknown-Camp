using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SelectCharacterController : MonoBehaviour
{
	public static SelectCharacterController Instance { get; private set; }
	private void Awake()
	{
		if (Instance == null) Instance = this;

		camper = MasterManager.CharactersList.DefaultCamper;
		catcher = MasterManager.CharactersList.DefaultCatcher;
	}

	[Header("Panel Components")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Transform panelCampers;
    [SerializeField] private Transform panelCatchers;
    [SerializeField] private SelectCharacter selectPrefab;

	[Header("Lobby Component")]
	[SerializeField] private LobbyController lobbyController;

	[Header("Character Values")]
	[SerializeField] private TextMeshProUGUI textCharacterName;
	[SerializeField] private Image imageCharacter;

	[Header("Campers Values")]
	[SerializeField] private GameObject campersTraits;
    [SerializeField] private TextMeshProUGUI textHp;
	[SerializeField] private TextMeshProUGUI textSpeed;
	
	[SerializeField] private TextMeshProUGUI textMind;
	[SerializeField] private Image barMind; 
	
	[SerializeField] private TextMeshProUGUI textStrength;
	[SerializeField] private Image barStrength;

	[SerializeField] private TextMeshProUGUI textStealth;
	[SerializeField] private Image barStealth;

	[SerializeField] private TextMeshProUGUI textAbility;

    [Header("Catchers Values")]
    [SerializeField] private GameObject catchersTraits;
    [SerializeField] private TextMeshProUGUI textSpeedCatcher;
    [SerializeField] private TextMeshProUGUI textCatchCD;
    [SerializeField] private TextMeshProUGUI textPrepare;
    [SerializeField] private TextMeshProUGUI textHunting;
    [SerializeField] private TextMeshProUGUI textAbilityCatcher;

    private CharacterCamper camper;
    private CharacterCatcher catcher;
    private string skinName;
	private int skinIndex = -1;
	public static CharacterCamper Camper => Instance.camper;
    public static CharacterCatcher Catcher => Instance.catcher;
    public static string SkinName => Instance.skinName;

	private SelectCharacter prevCamper;
	private SelectCharacter nowCamper;

    private SelectCharacter prevCatcher;
    private SelectCharacter nowCatcher;

    private void Start()
	{
		Render();
	}

    public void Open()
    {
        panel.SetActive(true);

		bool isCamper = lobbyController.CurrentRole == Character.CharacterType.Camper;
		bool isCatcher = lobbyController.CurrentRole == Character.CharacterType.Catcher;

        panelCampers.gameObject.SetActive(isCamper);
		campersTraits.SetActive(isCamper);

		panelCatchers.gameObject.SetActive(isCatcher);
		catchersTraits.SetActive(isCatcher);	
	}

    public void Close()
    {
        panel.SetActive(false);
    }

    private void Clear()
	{
		foreach (Transform panel in panelCampers)
		{
			Destroy(panel.gameObject);
		}
        foreach (Transform panel in panelCatchers)
        {
            Destroy(panel.gameObject);
        }
    }

	private void Render()
	{
		Clear();
		foreach (CharacterCamper camper in MasterManager.CharactersList.Campers)
		{
			SelectCharacter select = Instantiate(selectPrefab, panelCampers);
			if (select == null) return;

			select.Initialize(camper, this.camper == camper);
			if (this.camper == camper) nowCamper = prevCamper = select;
        }
        foreach (CharacterCatcher catcher in MasterManager.CharactersList.Catchers)
        {
            SelectCharacter select = Instantiate(selectPrefab, panelCatchers);
            if (select == null) return;

            select.Initialize(catcher, this.catcher == catcher);
            if (this.catcher == catcher) nowCatcher = prevCatcher = select;
        }
		UpdateSelected();
    }

	public void UpdateSelected()
	{
		SelectCharacter selchar = lobbyController.CurrentRole == Character.CharacterType.Camper ? nowCamper : nowCatcher;
		selchar.InvokeSelectEvent();
    }

	private void SetBar(Image bar, int value)
	{
		bar.fillAmount = value / 10f;
	}

	public void SetCamper(CharacterCamper camper, SelectCharacter selectCharacter)
    {
        skinIndex = -1;
        skinName = "";

        this.camper = camper;
		nowCamper = selectCharacter;

        textCharacterName.text = camper.Name;
		imageCharacter.sprite = camper.StandSprite;
		
		textHp.text = $"{camper.Traits.hp}";
		textSpeed.text = $"{camper.Traits.speed}";
		
		int mind = camper.Traits.mind;
		textMind.text = $"{mind}/10";
		SetBar(barMind, mind);

		int strength = camper.Traits.strength;
		textStrength.text = $"{strength}/10";
		SetBar(barStrength, strength);

		int stealth = camper.Traits.stealth;
		textStealth.text = $"{stealth}/10";
		SetBar(barStealth, stealth);

		textAbility.text = camper.Traits.abilityText;
        LayoutRebuilder.ForceRebuildLayoutImmediate(textAbility.rectTransform);
    }

    public void SetCatcher(CharacterCatcher catcher, SelectCharacter selectCharacter)
    {
        skinIndex = -1;
        skinName = "";

        this.catcher = catcher;
        nowCatcher = selectCharacter;

        textCharacterName.text = catcher.Name;
        imageCharacter.sprite = catcher.StandSprite;

        textSpeedCatcher.text = $"{catcher.Traits.speed}";
        textCatchCD.text = $"{catcher.Traits.catchCD}";
        textPrepare.text = $"{catcher.Traits.preparePhase}";
        textHunting.text = $"{catcher.Traits.attackPhase}";

        textAbilityCatcher.text = catcher.Traits.abilityText;
        LayoutRebuilder.ForceRebuildLayoutImmediate(textAbility.rectTransform);
    }


    public void Confirm()
	{
        if (lobbyController.CurrentRole == Character.CharacterType.Camper)
		{
            prevCamper = nowCamper;
            lobbyController.SetCharacter(camper, skinName);
        }
		else
		{
			prevCatcher = nowCatcher;
			lobbyController.SetCharacter(catcher, skinName);
		}
	}

	public void Cancel()
	{
        if (lobbyController.CurrentRole == Character.CharacterType.Camper)
        {
            if (nowCamper == prevCamper) return;

            nowCamper.Deselect(true);
            prevCamper.Select(true);
        }
        else
        {
            if (nowCatcher == prevCatcher) return;

            nowCatcher.Deselect(true);
            prevCatcher.Select(true);
        }
    }

    public void TryChangeSkin()
    {
		skinIndex += 1;

		Character character = lobbyController.CurrentRole == Character.CharacterType.Camper ? camper : catcher;

        if (skinIndex < character.Skins.Count)
		{
			if (MasterManager.LocalAccount.Unlockables.Any(x => x.UnlockableId == character.Skins[skinIndex].CodeName))
			{
				imageCharacter.sprite = character.Skins[skinIndex].StandSprite;
				skinName = character.Skins[skinIndex].CodeName;
			}
			else TryChangeSkin();
        }
		else
		{
			skinIndex = -1;
            imageCharacter.sprite = character.StandSprite;
            skinName = "";
        }
    }
}
