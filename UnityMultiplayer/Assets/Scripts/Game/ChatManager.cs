using System;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ChatManager : MonoBehaviourPun, IPointerEnterHandler, IPointerExitHandler
{
    public List<ChatMessage> ChatLog { get; private set; } = new();

    [SerializeField] private GameObject _fullChatPanel;
    [SerializeField] private TextMeshProUGUI _fullChatText;
    [SerializeField] private GameObject _lastEntryChatPanel;
    [SerializeField] private TextMeshProUGUI _lastEntryChatText;
    [SerializeField] private TMP_InputField _inputField;
    [SerializeField] private float _chatNameAlphaValue;
    [SerializeField] private float _chatMessageAlphaValue;

    public static float ChatNameAlphaValue;
    public static float ChatMessageAlphaValue;
    private bool _chatIsOpen = false;
    private bool _mouseOnUI = false;

    private void Start()
    {
        ChatNameAlphaValue = _chatNameAlphaValue;
        ChatMessageAlphaValue = _chatMessageAlphaValue;
        _fullChatPanel.SetActive(false);
    }
    

    public void OpenChat()
    {
        if (_chatIsOpen) return;
        _lastEntryChatPanel.SetActive(false);

        if (ChatLog.Count > 0)
        {
            var chatHistory = "";
            foreach (var chatMessage in ChatLog)
            {
                chatHistory += chatMessage.Name + chatMessage.Message + "\n";
            }
            _fullChatText.text = chatHistory;
        }
        
        _fullChatPanel.SetActive(true);
        _chatIsOpen = true;
    }

    public void CloseChat()
    {
        if (!_chatIsOpen) return;
        _fullChatPanel.SetActive(false);
        _lastEntryChatPanel.SetActive(true);
        _lastEntryChatText.text = ChatLog[^1].Name + ChatLog[^1].Message;
        _chatIsOpen = false;
    }

    #region RPC
    [PunRPC]
    public void SendChatMessage(string message, PhotonMessageInfo messageInfo)
    {
        //var chatMessage = new ChatMessage(message, message, ColorLogHelper.SetColor(message, Color.green));
    }
    

    #endregion

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !_mouseOnUI && _chatIsOpen) CloseChat();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _mouseOnUI = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _mouseOnUI = false;
    }
}

public readonly struct ChatMessage
{
    public string Name
    {
        get
        {
            var color = Color;
            color.a = ChatManager.ChatNameAlphaValue;
            return name.SetColor(color) + ": ";
        }
    }

    public string Message
    {
        get
        {
            var color = Color;
            color.a = ChatManager.ChatMessageAlphaValue;
            return message.SetColor(color);
        }
    }

    private readonly string name;
    private readonly string message;
    public readonly Color Color;

    public ChatMessage(string name, string message, Color color)
    {
        this.name = name;
        this.message = message;
        Color = color;
    }
}