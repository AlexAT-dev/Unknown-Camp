using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public static GameController Instance;
    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public struct EventCodes
    {
        public const byte PHASE_CHANGE = 1;
        public const byte PHASE_PREPARE_ATTACK = 2;
        public const byte PHASE_END_ATTACK = 3;
    }

    [Header("Components")]
    [SerializeField] private SpectatePanel spectatePanel;
    [SerializeField] private FixedJoystick joystick;
    [SerializeField] private GameEvents gameEvents;

    [Header("Prefabs")]
    [SerializeField] private PlayerCamper camperPrefab;
    [SerializeField] private PlayerCatcher catcherPrefab;

    [Header("Players")]
    //[SerializeField] private List<PlayerCamper> catchedCampers;
    [SerializeField] private List<PlayerCamper> campers;
    [SerializeField] private List<PlayerCatcher> catchers; //idk maybe for Duo game mode???

    [Header("Game Objects")]
    [SerializeField] private Transform campersSpawner;
    [SerializeField] private Transform catcherSpawner;
    [SerializeField] private Transform catchZone;

    [Header("Roles")]
    [SerializeField] private GameObject campersManager;
    [SerializeField] private GameObject campersButtons;

    [SerializeField] private GameObject catcherManager;
    [SerializeField] private GameObject catcherButtons;

    [Header("End Game")]
    [SerializeField] private TextMeshProUGUI endGameHeader;
    [SerializeField] private Transform endGameUI;
    [SerializeField] private TextMeshProUGUI endGameMatches;
    [SerializeField] private Button spectateButton;
    [SerializeField] private bool endGameVisited;

    [Header("PhaseState")]
    [SerializeField, ReadOnly] private PhaseState phase;

    private bool iControllAI = false;

    public enum PhaseState
    {
        Prepare,
        Attack
    }

    //Local values
    private Player localPlayer;
    [SerializeField, ReadOnly] private CharacterCamper.CamperTraits localCamperTraits;
    [SerializeField, ReadOnly] private CharacterCatcher.CatcherTraits localCatcherTraits;
    private Dictionary<string, int> matches = new();
    //Player values
    public static Player LocalPlayer => Instance.localPlayer;
    public static PlayerCamper LocalCamper => LocalPlayer as PlayerCamper;
    public static PlayerCatcher LocalCatcher => LocalPlayer as PlayerCatcher;
    public static PlayerCamper.StateTypes LocalState => LocalCamper.State;
    public static bool isLocalPermanentState => LocalCamper.InPermanentState;
    public static Character.CharacterType LocalRole => LocalPlayer.Character.Type;
    public static bool IsCamper => LocalRole == Character.CharacterType.Camper;
    public static bool IsCatcher => LocalRole == Character.CharacterType.Catcher;
    public static ref CharacterCamper.CamperTraits LocalCamperTraits => ref Instance.localCamperTraits;
    public static ref CharacterCatcher.CatcherTraits LocalCatcherTraits => ref Instance.localCatcherTraits;
    public static GameObject CameraTarget => LocalPlayer.CameraTarget;

    //Game values
    public static PhaseState Phase => Instance.phase;

    //Players
    public static List<PlayerCamper> Campers => Instance.campers;
    public static List<PlayerCatcher> Catchers => Instance.catchers;

    public static List<PlayerCamper> AvailableCampers => Campers.FindAll(camper => camper != null && !camper.InPermanentState);

    public static FixedJoystick Joystick => Instance.joystick;
    public static GameEvents GameEvents => Instance.gameEvents;

    public static bool IControllAI => Instance.iControllAI;


    private void Start()
    {
        iControllAI = PhotonNetwork.IsMasterClient;

        CreatePlayer();
        ActionPanel.Instance.RenderTasks();

        bool isCamper = LocalRole == Character.CharacterType.Camper;
        bool isCatcher = LocalRole == Character.CharacterType.Catcher;

        campersManager.SetActive(isCamper);
        campersButtons.SetActive(isCamper);

        catcherManager.SetActive(isCatcher);
        catcherButtons.SetActive(isCatcher);
        ActionPanel.ShopButton.gameObject.SetActive(isCatcher);

        if (isCamper)
        {
            ActionPanel.Instance.SetPanel(ActionPanel.ActionPanelType.Tasks);
        }
        else if (isCatcher)
        {
            ActionPanel.Instance.SetPanel(ActionPanel.ActionPanelType.SoulsShop);
        }
    }

    #region Setup
    private void CreatePlayer()
    {
        object[] data = 
        { 
            PhotonNetwork.LocalPlayer.CustomProperties["Character"],
            PhotonNetwork.LocalPlayer.CustomProperties["Skin"]
        };

        Character.CharacterType role = (Character.CharacterType)((int?)PhotonNetwork.LocalPlayer.CustomProperties["Role"] ?? 0);
        
        if (role == Character.CharacterType.Camper)
        {
            CharacterCamper character = MasterManager.CharactersList.FindCharacterCamper(PhotonNetwork.LocalPlayer.CustomProperties["Character"]?.ToString());
            PunManager.SetLocalCamperHpAndState(character.Traits.hp, PlayerCamper.StateTypes.Default);
            localCamperTraits = character.Traits;
        }
        else
        {
            CharacterCatcher character = MasterManager.CharactersList.FindCharacterCatcher(PhotonNetwork.LocalPlayer.CustomProperties["Character"]?.ToString());
            localCatcherTraits = character.Traits;
        }
        
        Transform spawner = role switch
        {
            Character.CharacterType.Camper => campersSpawner,
            Character.CharacterType.Catcher => catcherSpawner,
            _ => null
        };
        string prefabName = role switch
        {
            Character.CharacterType.Camper => camperPrefab.name,
            Character.CharacterType.Catcher => catcherPrefab.name,
            _ => null
        };

        Vector3 pos = new Vector3(spawner.position.x + Random.Range(-5f, 5f), spawner.position.y + Random.Range(-5f, 5f));
        GameObject newPlayer = PhotonNetwork.Instantiate(prefabName, pos, Quaternion.identity, 0, data);
        localPlayer = role switch
        {
            Character.CharacterType.Camper => newPlayer.GetComponent<PlayerCamper>(),
            Character.CharacterType.Catcher => newPlayer.GetComponent<PlayerCatcher>(),
            _ => null
        };
    }

    public void InstantiateItem(InteractiveItem item, Vector3 pos, Vector2 zone)
    {
        Vector3 newPos = (zone == Vector2.zero) ? pos : new Vector3(pos.x + Random.Range(-zone.x, zone.x), pos.y + Random.Range(-zone.y, zone.y));
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Instantiate(Path.Combine("Items", item.name), newPos, Quaternion.identity);
        }
    }
    #endregion

    #region PhaseAndCatchSystem
    public void SetPhase(PhaseState phase)
    {
        SetPhaseLocal(phase);

        RaiseEventOptions options = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
        PhotonNetwork.RaiseEvent(EventCodes.PHASE_CHANGE, phase, options, SendOptions.SendReliable);
    }

    private void SetPhaseLocal(PhaseState phase)
    {
        this.phase = phase;

        if (phase == PhaseState.Prepare)
        {
            ShowCatcher(false);

            if (IsCamper && LocalCamper.State == PlayerCamper.StateTypes.Caught)
            {
                LocalCamper.SetMineState(PlayerCamper.StateTypes.Default);
                Vector3 pos = new Vector3(campersSpawner.position.x + Random.Range(-5f, 5f), campersSpawner.position.y + Random.Range(-5f, 5f));
                LocalCamper.Teleport(pos);
            }
        }
    }

    public void CatchLocalCamper()
    {
        int hp = (int)LocalCamper.Owner.CustomProperties["Hp"] - 1;
        PlayerCamper.StateTypes state;
        if (hp > 0)
        {
            state = PlayerCamper.StateTypes.Caught;
            Vector3 pos = new Vector3(catchZone.position.x + Random.Range(-4f, 4f), catchZone.position.y + Random.Range(-1f, 1f));
            LocalCamper.Teleport(pos);
        }
        else
        {
            state = PlayerCamper.StateTypes.Eliminated;
            LocalCamper.Eliminated();
            Eliminated();
        }
        PunManager.SetLocalCamperHpAndState(hp, state);
    }

    public void ShowCatcher(bool visible)
    {
        foreach (PlayerCatcher catcher in catchers)
        {
            catcher.UpdateVisibility(visible);
        }
    }

    #endregion

    #region EndGame
    public void Spectate()
    {
        if (AvailableCampers.Count == 0)
        {
            spectateButton.interactable = false;
            return;
        }

        campersButtons.SetActive(false);
        spectatePanel.Activate();
    }

    public void Eliminated()
    {
        ShowEndGameUI("Eliminated");
    }

    public void Escape()
    {
        Invoke(nameof(ShowEndGameUIEscaped), 2f);
    }

    private void ShowEndGameUIEscaped()
    {
        ShowEndGameUI("Escaped!");
    }

    public void ShowEndGameUI(string reason)
    {
        if (!endGameVisited)
        { 
            endGameVisited = true;
            endGameHeader.text = reason;
            endGameMatches.text = "";
            spectateButton.interactable = AvailableCampers.Count > 0;
            int value = 0;
            foreach(var item in matches)
            {
                value += item.Value;
                endGameMatches.text += $"<sprite=1>x{item.Value} - {item.Key}\n";
            }
            endGameMatches.text += $"\nTotal: <sprite=1>x{value}";
            //PlayerPrefs.SetInt("matches", PlayerPrefs.GetInt("matches") + value);
            WebRequest.Post(this, $"Account/AddMatches/{value}", null, (request) => {});
        }
        endGameUI.gameObject.SetActive(true);
    }
    
    public void AddMatches(string reason, int value)
    {
        if(matches.ContainsKey(reason)) matches[reason] = matches[reason] + value;
        else matches[reason] = value;
    }
    #endregion

    #region CallbacksEvents
    public void OnEvent(EventData photonEvent)
    {
        switch (photonEvent.Code)
        {
            case EventCodes.PHASE_CHANGE:
                SetPhaseLocal((PhaseState)photonEvent.CustomData);
                break;
            case EventCodes.PHASE_PREPARE_ATTACK:
                gameEvents.StartAttack();
                ShowCatcher(true);
                break;
            case EventCodes.PHASE_END_ATTACK:
                gameEvents.StopAttack();
                break;
        }
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(MasterManager.SceneNames.MainMenu);
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        FindPlayerByPunPlayer(otherPlayer)?.LeftRoom();
        CheckForEnd(otherPlayer);
    }

    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, Hashtable changedProps)
    {
        FindCamperByPunPlayer(targetPlayer)?.SyncState();
        CamperIconList.Instance.UpdateCamperIcon(targetPlayer);
        CheckForEnd(targetPlayer);
    }

    private void CheckForEnd(Photon.Realtime.Player targetPlayer)
    {
        if (Campers.Count != 0 && AvailableCampers.Count == 0 && !targetPlayer.IsLocal)
        {
            ShowEndGameUI(IsCatcher ? "Ending" : "");
            spectateButton.interactable = false;
        }
    }
    #endregion

    #region FindPlayers
    public T FindPlayerByPhotonView<T>(int viewID) where T : Player
    {
        T[] players = FindObjectsOfType<T>();
        foreach (var player in players)
        {
            if (player.PhotonView.ViewID == viewID)
            {
                return player;
            }
        }
        return null;
    }

    public PlayerCamper FindCamperByPunPlayer(Photon.Realtime.Player player)
    {
        foreach (var camper in campers)
        {
            if (camper?.Owner == player) return camper;
        }
        return null;
    }

    public PlayerCatcher FindCatcherByPunPlayer(Photon.Realtime.Player player)
    {
        foreach (var catcher in catchers)
        {
            if (catcher.Owner == player) return catcher;
        }
        return null;
    }

    public Player FindPlayerByPunPlayer(Photon.Realtime.Player player)
    {
        Player foundPlayer = FindCamperByPunPlayer(player);
        foundPlayer ??= FindCatcherByPunPlayer(player);
        return foundPlayer;
    }
    #endregion

    #region ButtonEvents
    public void ButtonBack()
    {
        if (endGameVisited) ShowEndGameUI("");
        else ButtonLeave();
    }

    public void ButtonLeave()
    {
        PhotonNetwork.LeaveRoom();
    }
    #endregion
}
