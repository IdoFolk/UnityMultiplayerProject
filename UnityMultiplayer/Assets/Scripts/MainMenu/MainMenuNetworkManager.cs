using System;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using UnityEditor;
using UnityEngine.Serialization;

public class MainMenuNetworkManager : MonoBehaviourPunCallbacks
{
    [Header("Settings")]
    [SerializeField] private int minimumPlayers = 2;
    
    [Header("Panels")] 
    [SerializeField] private GameObject EnterNicknamePanel;
    [SerializeField] private GameObject StartGamePanel;
    [SerializeField] private GameObject lobbyInfoPanel;
    [SerializeField] private GameObject inLobbyPanelHolder;
    
    [Header("buttons")] 
    [SerializeField] private Button startGameButton;
    
    [Header("Text")]
    [SerializeField] private TextMeshProUGUI networkStatusText;
    [SerializeField] private TextMeshProUGUI currentRoomPlayersNumber;
    [SerializeField] private TextMeshProUGUI lobbyRoomsInfo;

    [Header("GameObjects")] 
    [SerializeField] private GameObject roomListObject;
    [SerializeField] private RoomInfoItem roomInfoItemPrefab;
     
    
    private string roomName = "";
    private string _lobbyName;
    private string defaultLobbyName = "DefaultLobby";
    private string defaultRoomName = "DefaultRoom";
    private string gameSceneName = "GameScene";
    
    private List<RoomData> CurrentShownRoomData = new List<RoomData>();
    //private Dictionary<string,float> roomListInfo = new Dictionary<string, float>(); //<RoomName, PlayerCount>
    private List<GameObject> listOfRoomInfoItems = new();
    
    
    
    
    private void Start()
    {
        StartGamePanel.SetActive(false);
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();
        
    }
    
    public void SubmitLobbyName(string lobbyName) => _lobbyName = lobbyName;
    
    public void SubmitNickname(string nickname) => PhotonNetwork.NickName = nickname;
    
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        networkStatusText.text = "Cant join a room";
        Debug.Log("cant join room");
    }
    
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        base.OnJoinRandomFailed(returnCode, message);
        networkStatusText.text = "Can't join that room";
    }
    
    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        networkStatusText.text = PhotonNetwork.NetworkClientState.ToString();
    }
    
    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
        networkStatusText.text = "Room created";
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        networkStatusText.text = "Joined room";
        RefreshCurrentRoomInfo();
        
        CurrentShownRoomData.Clear();
        
        foreach (var VARIABLE in listOfRoomInfoItems)
        {
            Destroy(VARIABLE);
        }
        listOfRoomInfoItems.Clear();
        
        // foreach (var player in PhotonNetwork.CurrentRoom.Players)
        // {
        //     if (player.Value.CustomProperties["ID"] as string == SystemInfo.deviceUniqueIdentifier)
        //     {
        //         PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable(){{"ID", SystemInfo.deviceUniqueIdentifier + PhotonNetwork.LocalPlayer.ActorNumber}});
        //         return;
        //     }
        // }
        // PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable(){{"ID", SystemInfo.deviceUniqueIdentifier}});
    }



    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        RefreshCurrentRoomInfo();
        
    }
    
    
    public override void OnRoomListUpdate(List<RoomInfo> changedRoomList)
    {
        base.OnRoomListUpdate(changedRoomList);
        
        List<RoomData> newListOfRoomData = new List<RoomData>(CurrentShownRoomData);
        foreach (var changedRoomInfo in changedRoomList)
        {
            bool roomIsNew = true;

            if (changedRoomInfo.RemovedFromList)
            {
                foreach (var VARIABLE in CurrentShownRoomData)
                {
                    if(changedRoomInfo.Name == VARIABLE.roomName)
                        newListOfRoomData.Remove(VARIABLE);
                }
                continue;
            }
            
            foreach (var oldRoomInfo in newListOfRoomData)
            {
                if (oldRoomInfo.roomName == changedRoomInfo.Name)
                {
                    roomIsNew = false;
                    oldRoomInfo.currentPlayers = changedRoomInfo.PlayerCount;
                }
            }
            if (roomIsNew )
            {
                newListOfRoomData.Add(new RoomData(changedRoomInfo.Name, changedRoomInfo.PlayerCount, changedRoomInfo.MaxPlayers, changedRoomInfo.CustomProperties["difficulty"] as string));
            }
        }
        
        CurrentShownRoomData.Clear();
        CurrentShownRoomData = newListOfRoomData;
        
        foreach (var VARIABLE in listOfRoomInfoItems)
        {
            Destroy(VARIABLE);
        }
        listOfRoomInfoItems.Clear();
        
        foreach (var newRoomInfo in CurrentShownRoomData)
        {
            string roomText = newRoomInfo.roomName + " Players: " + newRoomInfo.currentPlayers + "/" + newRoomInfo.maxPlayers + " difficulty: " + newRoomInfo.difficulty;

            bool canJoin = newRoomInfo.currentPlayers < newRoomInfo.maxPlayers;
            
            var roomInfoItem = Instantiate(roomInfoItemPrefab,roomListObject.transform.position,Quaternion.identity,roomListObject.transform);
            roomInfoItem.Init(newRoomInfo.roomName,roomText,canJoin);
            listOfRoomInfoItems.Add(roomInfoItem.gameObject);
        }
    }
    
   
    
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        RefreshCurrentRoomInfo();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        RefreshCurrentRoomInfo();
    }
    
    public void StartGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(gameSceneName);
        }
    }
    
    private void RefreshCurrentRoomInfo()
    {
        if (PhotonNetwork.InRoom)
        {
            StartGamePanel.SetActive(true);
            currentRoomPlayersNumber.SetText(PhotonNetwork.CurrentRoom.PlayerCount + " / " + PhotonNetwork.CurrentRoom.MaxPlayers);
            
            startGameButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
            startGameButton.interactable = PhotonNetwork.CurrentRoom.PlayerCount >= minimumPlayers;
        }
        else
        {
            StartGamePanel.SetActive(false);
        }
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        inLobbyPanelHolder.SetActive(true);
    }
}

public class RoomData
{
    public string roomName;
    public int currentPlayers;
    public int maxPlayers;
    public string difficulty;
    
    public RoomData(string roomName, int currentPlayers, int maxPlayers, string difficulty)
    {
        this.roomName = roomName;
        this.currentPlayers = currentPlayers;
        this.maxPlayers = maxPlayers;
        this.difficulty = difficulty;
    }
}
