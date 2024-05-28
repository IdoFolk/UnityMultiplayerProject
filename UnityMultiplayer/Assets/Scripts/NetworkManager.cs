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

    public void JoinLobby()
    {
        PhotonNetwork.JoinLobby();
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

    #endregion
}
