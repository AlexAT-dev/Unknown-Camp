using Newtonsoft.Json.Linq;
using Photon.Pun;
using Photon.Realtime;
using SimpleJSON;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnknownCamp.API;

public class StartupController : MonoBehaviourPunCallbacks
{
    [Header("Base")]
    [SerializeField] private TextMeshProUGUI textVersion;
    [SerializeField] private TextMeshProUGUI textNickName;
    [SerializeField] private GameObject ConnectionHUD;

    [Header("Guest")]
    [SerializeField] private TextMeshProUGUI textError;

    [Header("Login")]
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private LoginInputs loginInputs;


    [Serializable]
    public class LoginInputs
    {
        public TMP_InputField Login;
        public TMP_InputField Password;
    }

    [Header("Registration")]
    [SerializeField] private GameObject regPanel;
    [SerializeField] private RegInputs regInputs;


    [Serializable]
    public class RegInputs
    {
        public TMP_InputField Email;
        public TMP_InputField Nickname;
        public TMP_InputField Password;
        public TMP_InputField Password2;
    }

    private void Start()
    {
        textVersion.text = "Game Version: " + MasterManager.Version;

        LoadingScreenController.Instance.Show();
        WebRequest.Get(this, "AppConfig/GameVersion", (UnityWebRequest request) =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                LoadingScreenController.Instance.Hide();
                string version = request.downloadHandler.text;
                if (version != MasterManager.Version)
                {
                    MessagePanelController.Instance.Show("Error", "Game version mismatch. Please update the game.", () =>
                    {
                        Application.Quit();
                    });
                }
            }
            else
            {
                LoadingScreenController.Instance.Hide();
                MessagePanelController.Instance.Show("Error", "No Server Connection!", () =>
                {
                    Application.Quit();
                });
            }
        });
    }

    public void OnClick_Login()
    {
        if (textNickName.text.Length < 5)
        {
            textError.gameObject.SetActive(true);
            Invoke("HideErrorText", 2f);
            return;
        }

        textError.gameObject.SetActive(false);
        Connect(textNickName.text);
    }

    private void Connect(string name)
    {
        LoadingScreenController.Instance.Show("Connecting...");
        PunManager.Connect(name);
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master!");
        PhotonNetwork.JoinLobby();
        SceneManager.LoadScene(MasterManager.SceneNames.MainMenu);
    }

    public override void OnDisconnected(DisconnectCause cause) 
    {
        Debug.Log($"Disconnected ({cause})");
    }

    private void HideErrorText()
    {
        textError.gameObject.SetActive(false);
    }

    public void ShowLogin()
    {
        loginPanel.SetActive(true);
        regPanel.SetActive(false);
    }

    public void ShowReg()
    {
        regPanel.SetActive(true);
        loginPanel.SetActive(false);
    }

    public void ClickLogin()
    {
        if(Utils.IsAnyInputEmpty(loginInputs.Login, loginInputs.Password))
        {
            MessagePanelController.Instance.Show("Error", "Not all fields are filled");
            return;
        }

        AccountLoginDTO accountLoginDTO = new AccountLoginDTO()
        {
            Email = loginInputs.Login.text,
            Password = loginInputs.Password.text
        };

        LoadingScreenController.Instance.Show();
        WebRequest.Post(this, "Account/Login", JsonUtility.ToJson(accountLoginDTO), (UnityWebRequest request) =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                JObject jsonResponse = JObject.Parse(request.downloadHandler.text);

                string token = jsonResponse["Token"]?.ToString();
                JObject accountObj = jsonResponse["Account"] as JObject;

                Account account = accountObj != null
                    ? accountObj.ToObject<Account>()
                    : null;

                Debug.Log("Token: " + token);
                WebRequest.SetBearerToken(token);
                PlayerPrefs.SetString("Token", token);
                if (account != null)
                {
                    Debug.Log(account.Id);
                    MasterManager.SetAccount(account);
                    Connect(account.Name);
                }
                else
                {
                    MessagePanelController.Instance.Show("Error", "Account data is missing.");
                }
            }
            else if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                LoadingScreenController.Instance.Hide();
                MessagePanelController.Instance.Show("Network error", "Could not connect to server.");
            }
            else
            {
                try
                {
                    Debug.Log(request.downloadHandler.text);
                    JObject errorResponse = JObject.Parse(request.downloadHandler.text);
                    string errorMessage = errorResponse["Message"]?.ToString();
                    MessagePanelController.Instance.Show("Error", errorMessage);
                }
                catch (System.Exception ex)
                {
                    MessagePanelController.Instance.Show("Error", ex.Message);
                }
                LoadingScreenController.Instance.Hide();
            }


        });
    }

    public void ClickReg()
    {
        if (Utils.IsAnyInputEmpty(regInputs.Email, regInputs.Nickname, regInputs.Password, regInputs.Password2))
        {
            MessagePanelController.Instance.Show("Error", "Not all fields are filled");
            return;
        }

        if (regInputs.Password.text != regInputs.Password2.text)
        {
            MessagePanelController.Instance.Show("Error", "Different Passwords");
            return;
        }

        AccountRegDTO accountRegDTO = new AccountRegDTO()
        {
            Email = regInputs.Email.text,
            Name = regInputs.Nickname.text,
            Password = regInputs.Password.text
        };

        LoadingScreenController.Instance.Show();
        WebRequest.Post(this, "Account/Create", JsonUtility.ToJson(accountRegDTO), (UnityWebRequest request) =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                JObject jsonResponse = JObject.Parse(request.downloadHandler.text);
                string message = jsonResponse["Message"]?.ToString();
                Debug.Log("Message from server: " + message);
                Debug.Log(request.downloadHandler.text);
                MessagePanelController.Instance.Show("Confirm email", "Confirm your email in your email and then login", ShowLogin);
            }
            else if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                LoadingScreenController.Instance.Hide();
                MessagePanelController.Instance.Show("Network error", "Could not connect to server.");
            }
            else
            {
                Debug.LogError("Request failed: " + request.error);

                // Try to parse server error message
                if (!string.IsNullOrEmpty(request.downloadHandler.text))
                {
                    try
                    {
                        Debug.Log(request.downloadHandler.text);
                        JObject errorResponse = JObject.Parse(request.downloadHandler.text);
                        string errorMessage = errorResponse["Message"]?.ToString();
                        Debug.LogError("Server Error Message: " + errorMessage);
                        MessagePanelController.Instance.Show("Error", errorMessage);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError("Failed to parse error response: " + ex.Message);
                        MessagePanelController.Instance.Show("Error", ex.Message);
                    }
                }
                else
                {
                    MessagePanelController.Instance.Show("Error", request.error);
                }
            }
            LoadingScreenController.Instance.Hide();
        });
    }
}
