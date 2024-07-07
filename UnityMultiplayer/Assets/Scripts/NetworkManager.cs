using System;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    private const string appIDPun = "50f5dde3-92b-445d-bf32-c2a347c5dd23a";
    public static NetworkManager Instance;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        Connect();
    }

    public void Connect()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public void JoinLobby(string lobbyName)
    {
        PhotonNetwork.JoinLobby(new TypedLobby(lobbyName,LobbyType.Default));
    }
    public void CreateRoom(string roomName, uint maximumPlayers)
    {
        RoomOptions newRoomOptions = new RoomOptions();
        newRoomOptions.MaxPlayers = (int)maximumPlayers;
        PhotonNetwork.CreateRoom(roomName, newRoomOptions,null);
    }

    public void JoinRoom(string roomName)
    {
        if (PhotonNetwork.CurrentRoom != null)
        {
            PhotonNetwork.LeaveRoom();
        }
        PhotonNetwork.JoinRoom(roomName);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    #region Callbacks
    public override void OnConnected()
    {
        Debug.Log("connected to master server");
        base.OnConnected();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        SceneManager.LoadScene(0);
        Debug.Log($"Disconnected from server. cause: {cause}");
        base.OnDisconnected(cause);
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("connected to lobby");
        base.OnJoinedLobby();
    }
    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
        Debug.Log("created room " + PhotonNetwork.CurrentRoom.Name);
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        Debug.Log("Joined room " + PhotonNetwork.CurrentRoom.Name);
    }
    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        Debug.Log("you left the room");
    }


    #endregion
}
