using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MessagePanelController : MonoBehaviour
{
    public static MessagePanelController Instance;
    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public enum MessageType
    {
        Message,
        Error
    }

    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text header;
    [SerializeField] private TMP_Text message;

    //todo: add buttons
    [SerializeField] private Button button;


    public void Show(string header, string message, Action onClick)
    {
        this.header.text = header;
        this.message.text = message;

        button.onClick.AddListener(() =>
        {
            onClick?.Invoke();
            Hide();
        });

        panel.SetActive(true);
    }

    public void Show(string header, string text) => Show(header, text, null);
    
    private void Hide()
    {
        panel.SetActive(false);
    }
}
