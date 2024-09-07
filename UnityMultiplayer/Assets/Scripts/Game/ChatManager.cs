using System;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ChatManager : MonoBehaviourPunCallbacks, IPointerEnterHandler, IPointerExitHandler
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
    private const string RecieveChatMessageRPC = nameof(RecieveChatMessage);

    private void Start()
    {
        ChatNameAlphaValue = _chatNameAlphaValue;
        ChatMessageAlphaValue = _chatMessageAlphaValue;
        _fullChatPanel.SetActive(false);
    }
    public void SendChatMessage()
    {
        var name = PhotonNetwork.NickName;
        var message = _inputField.text;
        var color = GameNetworkManager.CharacterColor.ToRGBHex();
        var chatMessageData = new ChatMessageData(name, message, color);
        var messageJson = JsonUtility.ToJson(chatMessageData);
        photonView.RPC(RecieveChatMessageRPC, RpcTarget.All, messageJson);
        _inputField.text = "";
    }

    public void OpenChat()
    {
        if (_chatIsOpen) return;
        _lastEntryChatPanel.SetActive(false);

        if (ChatLog.Count > 0)
        {
            _fullChatText.text = "";
            foreach (var chatMessage in ChatLog)
            {
                _fullChatText.text += chatMessage.Name + chatMessage.Message + "\n";
            }
        }
        
        _fullChatPanel.SetActive(true);
        _chatIsOpen = true;
    }
    

    public void RefreshChat()
    {
        if (!_chatIsOpen)
        {
            _lastEntryChatText.text = ChatLog[^1].Name + ChatLog[^1].Message;
        }
        else
        {
            _fullChatText.text = "";
            foreach (var chatMessage in ChatLog)
            {
                _fullChatText.text += chatMessage.Name + chatMessage.Message + "\n";
            }
        }
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
    public void RecieveChatMessage(string messageJson)
    {
        var chatMessageData = JsonUtility.FromJson<ChatMessageData>(messageJson);
        var chatMessage = new ChatMessage(chatMessageData);
        ChatLog.Add(chatMessage);
        RefreshChat();
    }

    #endregion

    #region Callbacks
    
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        base.OnMasterClientSwitched(newMasterClient);
        var message = newMasterClient.NickName + " is the new master client!";
        var color = GameNetworkManager.CharacterColor.ToRGBHex();
        photonView.RPC(nameof(RecieveChatMessage), RpcTarget.All, message, color);
        
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        var message = newPlayer.NickName + " has joined the room";
        var color = GameNetworkManager.CharacterColor.ToRGBHex();
        photonView.RPC(nameof(RecieveChatMessage), RpcTarget.All, message, color);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        var message = otherPlayer.NickName + " has left the room";
        var color = GameNetworkManager.CharacterColor.ToRGBHex();
        photonView.RPC(nameof(RecieveChatMessage), RpcTarget.All, message, color);
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
            var color = _messageData.Color.FromHexToColor();
            color.a = ChatManager.ChatNameAlphaValue;
            var str = _messageData.Name + ": ";
            return str.SetColor(color);
        }
    }

    public string Message
    {
        get
        {
            var color = _messageData.Color.FromHexToColor();
            color.a = ChatManager.ChatMessageAlphaValue;
            return _messageData.Message.SetColor(Color.white);
        }
    }

    private readonly ChatMessageData _messageData;

    public ChatMessage(ChatMessageData data)
    {
        _messageData = data;
    }
}

public struct ChatMessageData
{
    public string Name;
    public string Message;
    public string Color;

    public ChatMessageData(string name, string message, string color)
    {
        Name = name;
        Message = message;
        Color = color;
    }
}