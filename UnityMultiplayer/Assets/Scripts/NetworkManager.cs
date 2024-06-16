using System;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviourPunCallbacks
{
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

    public void Connect()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public void JoinLobby(string lobbyName)
    {
        PhotonNetwork.JoinLobby(new TypedLobby(lobbyName,LobbyType.Default));
    }
    public void CreateRoom(string roomName)
    {
        PhotonNetwork.CreateRoom(roomName,null,null);
    }

    public void JoinRoom(string roomName)
    {
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
