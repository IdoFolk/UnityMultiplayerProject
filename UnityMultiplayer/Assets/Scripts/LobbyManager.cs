using System.Collections.Generic;
using ExitGames.Client.Photon.StructWrapping;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    private const string noRoomFoundErrorCode= "32758";
    [SerializeField] private GameObject _lobbyRoomHolder;
    [SerializeField] private TextMeshProUGUI lobbyTitle;
    [SerializeField] private TextMeshProUGUI errorText;
    [SerializeField] private TMP_InputField roomNameInput, roomMaxPlayersInput;
    [SerializeField] private Button createRoomButton,joinRoomButton;
    [SerializeField] private RectTransform RoomListViewport;
    [SerializeField] private RectTransform RoomPrefab;

    private bool queuedRoomToJoin = false;
    private bool queuedRoomToCreate = false;

    public override void OnEnable()
    {
        base.OnEnable();
        lobbyTitle.text = "Lobby Name: " + PhotonNetwork.CurrentLobby.Name;
        errorText.text = "";
        RoomInputTextChange();
    }

    public void ConnectToRoom()
    {
        if (PhotonNetwork.CurrentRoom != null)
        {
            if (PhotonNetwork.CurrentRoom.Name == roomNameInput.text)
            {
                errorText.text = "Name identical to your current room.";
                return;
            }

            queuedRoomToJoin = true;
            PhotonNetwork.LeaveRoom();
            return;
        }
        
        NetworkManager.Instance.JoinRoom(roomNameInput.text);
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        //PhotonNetwork.CurrentRoom.Players[0].ActorNumber;
    }

    public void CreateRoom()
    {
        if (PhotonNetwork.CurrentRoom != null)
        {
            if (PhotonNetwork.CurrentRoom.Name == roomNameInput.text)
            {
                errorText.text = "Name identical to your current room.";
                return;
            }
            queuedRoomToCreate = true;
            PhotonNetwork.LeaveRoom();
            return;
        }
        
        if (uint.TryParse(roomMaxPlayersInput.text, out var maxPlayers))
        {
            NetworkManager.Instance.CreateRoom(roomNameInput.text, maxPlayers);
        }
        else
        {
            errorText.text = "Invalid max players value.";
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        if (returnCode.ToString() == noRoomFoundErrorCode)
        {
            errorText.text = "No room with that name.";
        }
        
    }

    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
        errorText.text = $"New room created: {PhotonNetwork.CurrentRoom.Name}";
        TMP_Text roomText = Instantiate(RoomPrefab, RoomListViewport).GetComponentInChildren<TMP_Text>();
        roomText.text = $"{PhotonNetwork.CurrentRoom.Name}: {PhotonNetwork.CurrentRoom.PlayerCount} / {PhotonNetwork.CurrentRoom.MaxPlayers} online";
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        base.OnRoomListUpdate(roomList);
        foreach (GameObject room in RoomListViewport)
        {
            Destroy(room);
        }
        foreach (var t in roomList)
        {
            TMP_Text roomText = Instantiate(RoomPrefab, RoomListViewport).GetComponentInChildren<TMP_Text>();
            roomText.text = $"{t.Name}: {t.PlayerCount} / {t.MaxPlayers} online";
        }
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        if (queuedRoomToJoin)
        {
            ConnectToRoom();
            queuedRoomToJoin = false;
        }

        if (queuedRoomToCreate)
        {
            CreateRoom();
            queuedRoomToCreate = false;
        }
    }

    public void RoomInputTextChange()
    {
        if (roomNameInput.text.Length > 0)
        {
            joinRoomButton.interactable = true;
            createRoomButton.interactable = roomMaxPlayersInput.text.Length > 0;
        }
        else
        {
            joinRoomButton.interactable = false;
            createRoomButton.interactable = false;
        }
    }
}
