using System;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.Serialization;

public class MainMenuNetworkManager : MonoBehaviourPunCallbacks
{
    [Header("Settings")]
    [SerializeField] private int minimumPlayers = 2;
    
    [Header("Panels")] 
    [SerializeField] private GameObject EnterNicknamePanel;
    [FormerlySerializedAs("currentRoomPanel")] [SerializeField] private GameObject StartGamePanel;
    [SerializeField] private GameObject lobbyInfoPanel;
    
    [Header("buttons")] 
    [SerializeField] private Button startGameButton;
    
    [Header("Text")]
    [SerializeField] private TextMeshProUGUI networkStatusText;
    [SerializeField] private TextMeshProUGUI currentRoomPlayersNumber;
    [SerializeField] private TextMeshProUGUI lobbyRoomsInfo;
    
     
    
    private string roomName = "";
    private string _lobbyName;
    private string defaultLobbyName = "DefaultLobby";
    private string defaultRoomName = "DefaultRoom";
    private string gameSceneName = "GameScene";
    
    
    private Dictionary<string,float> roomListInfo = new Dictionary<string, float>(); //<RoomName, PlayerCount>
    
    
    
    
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
        
    }

    ////////////////////////////////////////////////
    
    /*private void SetMaxPlayerNumberText(int obj)
    {
        maxPlayerNumberInRoom = obj;
    }*/
    
    /*public void CreateRoom()
    {
        RoomOptions roomOptions = new RoomOptions()
        {
            MaxPlayers = maxPlayerNumberInRoom,
        };
        if(roomName.Length >= 1)
            PhotonNetwork.CreateRoom(roomName,roomOptions);
        else
            PhotonNetwork.CreateRoom(defaultRoomName,roomOptions);
    }*/
    

    /*public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        Debug.Log($"We successfully joined the lobby {PhotonNetwork.CurrentLobby}!");
        joinLobbyButton.interactable = false;
        lobbyInfoPanel.SetActive(true);
    }*/

    /*public void JoinOrCreateRoom()
    {
        PhotonNetwork.JoinOrCreateRoom(roomNameInputField.text, null, null);
        
    }*/

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        RefreshCurrentRoomInfo();
        
    }
    
    /*public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        RefreshCurrentRoomInfo();
        Debug.Log("We successfully joined the room " + PhotonNetwork.CurrentRoom);
        lobbyInfoPanel.SetActive(false);
        joinRoomByNameButton.interactable = false;
        

    }*/

    /*public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        Debug.LogError($"We couldn't join the room because {message} return code is {returnCode}");
        ToggleJoinRoomButtonsState(true);
    }*/

    

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        base.OnRoomListUpdate(roomList);
        Dictionary<string,float> roomListInfoCopy = new Dictionary<string, float>(roomListInfo);
        foreach (var newRoomInfo in roomList)
        {
            bool roomIsNew = true;
            foreach (var oldRoomInfo in roomListInfoCopy)
            {
                if (oldRoomInfo.Key == newRoomInfo.Name)
                {
                    roomIsNew = false;
                    roomListInfo[oldRoomInfo.Key] = newRoomInfo.PlayerCount;
                }
            }
            if (roomIsNew)
            {
                roomListInfo.Add(newRoomInfo.Name, newRoomInfo.PlayerCount + newRoomInfo.MaxPlayers/10f);
            }
        }
    
        lobbyRoomsInfo.text = " ";
        foreach (KeyValuePair<string, float> roomInfo in roomListInfo)
        {
            lobbyRoomsInfo.text += roomInfo.Key + " Players: " + (int)roomInfo.Value + "/" + roomInfo.Value % 1 * 10 + "\n";
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

    /*private void ToggleJoinRoomButtonsState(bool active)
    {
        
        foreach (Button joinRoomButton in roomButtons)
        {
            joinRoomButton.interactable = active;
        }
    }*/

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
}
