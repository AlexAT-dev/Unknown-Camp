using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameController;

public class InteractionCatcherManager : InteractionManager
{
    public static InteractionCatcherManager Instance { get; private set; }
    private void Awake()
    {
        if (BaseInstance == null) BaseInstance = this;
        if (Instance == null) Instance = this;
    }

    [Header("Objects")]
    [SerializeField] private Soul soulPrefab;
    [SerializeField, ReadOnly] private PlayerCamper targetCamper;

    [Header("UI Interaction Buttons")]
    [SerializeField] private Button buttonInteract;
    [SerializeField] private Button buttonCatch;
    [SerializeField] private Button buttonPhase;
    [SerializeField] private TextMeshProUGUI textPhase;
    [SerializeField] private TextMeshProUGUI textPhaseCounter;
    [SerializeField] private TextMeshProUGUI textCatchCounter;
    [SerializeField] private Image imagePhase;
    [SerializeField] private Sprite spritePrepare;
    [SerializeField] private Sprite spriteAttack;

    [Header("Counter values")]
    [SerializeField] private Counter prepareCounter;
    [SerializeField] private Counter attackCounter;
    [SerializeField] private Counter catchCD;

    public static Counter PrepareCounter => Instance.prepareCounter;

    private Coroutine farmSoulCoroutine;
    private Coroutine createSoulCoroutine;
    private bool isPrepareAttack;
    private List<Soul> souls = new();

    public static List<Soul> Souls => Instance.souls;

    private void Start()
    {
        prepareCounter.SetMax(LocalCatcher.CharacterCatcher.Traits.preparePhase);
        attackCounter.SetMax(LocalCatcher.CharacterCatcher.Traits.attackPhase);
        catchCD.SetMax(LocalCatcher.CharacterCatcher.Traits.catchCD);
        SetPhase(PhaseState.Prepare);
        UpdateCatchCD();
        CreateSoul(8);
    }

    private void Update()
    {
        if (interactable != null)
        {
            if (Input.GetButtonUp(interactable.InputButton.ToString()))
            {
                Interact();
            }
        }

        if (targetCamper != null)
        {
            if (Input.GetButtonUp(targetCamper.InputButton.ToString()))
            {
                Catch();
            }
        }

        if (Input.GetButtonUp(MasterManager.Input.Action2.ToString()))
        {
            Phase();
        }
    }

    protected override void UpdateInteractiveButton()
    {
        if (GameController.Phase == PhaseState.Prepare) return;
        buttonInteract.interactable = interactable != null;
    }

    public void SetCamper(PlayerCamper camper)
    {
        if (GameController.Phase == PhaseState.Prepare) return;
        targetCamper = camper;
        targetCamper.SetInteractionActive(true);
        ShowActionButton(targetCamper);
        UpdateCatchEnabled();
    }

    public void RemoveCamper(PlayerCamper camper)
    {
        if (targetCamper != camper) return;
        targetCamper.SetInteractionActive(false);
        HideActionButton(targetCamper.InputButton);
        targetCamper = null;
        UpdateCatchEnabled();
    }

    public void Catch()
    {
        if (GameController.Phase == PhaseState.Attack && catchCD.IsMin)
        {
            targetCamper?.Interaction();
            catchCD.SetToMax();
            catchCD.StartCoroutine(this);
            UpdateCatchEnabled();
            UpdateCatchCD();
        }
    }

    //=============PHASE SYSTEM====================
    private void SetPhase(PhaseState phase)
    {
        GameController.Instance.SetPhase(phase);
        prepareCounter.SetToMax();
        attackCounter.SetToMax();
        LocalPlayer.TriggerRefresh();
        
        catchCD.StopCoroutine(this);
        catchCD.SetToMin();
        UpdateCatchEnabled();
        UpdateCatchCD();

        switch (phase)
        {
            case PhaseState.Prepare:
                imagePhase.sprite = spritePrepare;
                textPhase.text = "Phase";
                prepareCounter.StartCoroutine(this);
                farmSoulCoroutine = StartCoroutine(FarmSoulCoroutine());
                createSoulCoroutine = StartCoroutine(CreateSoulCoroutine());
                LocalCatcher.Movement.AddSpeedMultiplayer("Prepare", 3f);
                break;
            case PhaseState.Attack:
                imagePhase.sprite = spriteAttack;
                textPhase.text = "Stop";
                attackCounter.StartCoroutine(this);
                break;
        }
        UpdatePhaseButton();
    }

    public void UpdatePhaseButton()
    {
        switch (GameController.Phase)
        {
            case PhaseState.Prepare:
                buttonPhase.interactable = prepareCounter.IsMin;
                textPhaseCounter.enabled = !prepareCounter.IsMin;
                textPhaseCounter.text = prepareCounter.Value.ToString();
                break;

            case PhaseState.Attack:
                buttonPhase.interactable = true;
                textPhaseCounter.enabled = false;
                textPhase.text = $"Stop\n({attackCounter.Value})";
                break;
        }
    }

    public void UpdateCatchCD()
    {
        textCatchCounter.enabled = !catchCD.IsMin;
        textCatchCounter.text = catchCD.Value.ToString();
    }

    private void UpdateCatchEnabled()
    {
        if (GameController.Phase == PhaseState.Prepare)
        {
            buttonCatch.interactable = false;
        }
        else
        {
            buttonCatch.interactable = targetCamper != null && catchCD.IsMin;
        }
        
    }

    public void Phase()
    {
        switch (GameController.Phase)
        {
            case PhaseState.Prepare:
                if (!prepareCounter.IsMin) return;
                StartAttack(2.5f);
                break;
            case PhaseState.Attack:
                EndAttack();
                break;
        }
    }

    public void InstantAttack()
    {
        StartAttack(0);
    }

    private void StartAttack(float time)
    {
        if (time != 0 && !prepareCounter.IsMin || isPrepareAttack) return;

        StopCoroutine(farmSoulCoroutine);
        StopCoroutine(createSoulCoroutine);
        LocalCatcher.Movement.RemoveSpeedMultiplayer("Prepare");

        buttonPhase.interactable = false;
        isPrepareAttack = true;
        LocalCatcher.Movement.AddBlockReason("StartAttack");

        GameController.GameEvents.StartAttack();
        ActivateSouls(false);

        RaiseEventOptions options = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
        PhotonNetwork.RaiseEvent(EventCodes.PHASE_PREPARE_ATTACK, null, options, SendOptions.SendReliable);

        ProgressBar.Instance.StartProgress(time, LocalCatcher.transform.position, false, () =>
        {
            Debug.Log("Attack started");
            SetPhase(PhaseState.Attack);
            isPrepareAttack = false;
            LocalCatcher.Movement.RemoveBlockReason("StartAttack");
            LocalCatcher.UpdateVisibility(true);
        });
    }

    public void EndAttack()
    {
        attackCounter.StopCoroutine(this);

        GameController.GameEvents.StopAttack();
        ActivateSouls(true);

        RaiseEventOptions options = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
        PhotonNetwork.RaiseEvent(EventCodes.PHASE_END_ATTACK, null, options, SendOptions.SendReliable);

        SetPhase(PhaseState.Prepare);
    }

    private void CreateSoul(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (souls.Count >= 15) return;

            Vector3 randomPoint = new Vector3(Random.Range(-24.4f, 17f), Random.Range(-34f, 17f), 0);
            

            souls.Add(Instantiate(soulPrefab, randomPoint, Quaternion.identity));
        }
    }

    private void ActivateSouls(bool activate)
    {
        foreach(var soul in souls)
        {
            soul.gameObject.SetActive(activate);
        }
    }

    private IEnumerator FarmSoulCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(2f);
            ActionPanel.Instance.AddSouls(1);
        }
    }

    private IEnumerator CreateSoulCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(3f);
            CreateSoul(1);
        }
    }
}
