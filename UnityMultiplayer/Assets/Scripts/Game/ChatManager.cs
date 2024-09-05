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

    private void Start()
    {
        ChatNameAlphaValue = _chatNameAlphaValue;
        ChatMessageAlphaValue = _chatMessageAlphaValue;
        _fullChatPanel.SetActive(false);
    }
    public void SendChatMessage()
    {
        var message = _inputField.text;
        var color = GameNetworkManager.CharacterColor.ToRGBHex();
        photonView.RPC(nameof(RecieveChatMessage), RpcTarget.All, message, color);
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
    public void RecieveChatMessage(string message,string color, PhotonMessageInfo messageInfo)
    {
        var chatMessage = new ChatMessage(messageInfo.Sender.NickName, message, color.FromHexToColor());
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
            var color = Color;
            color.a = ChatManager.ChatNameAlphaValue;
            var str = name + ": ";
            return str.SetColor(color);
        }
    }

    public string Message
    {
        get
        {
            var color = Color;
            color.a = ChatManager.ChatMessageAlphaValue;
            return message.SetColor(Color.white);
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