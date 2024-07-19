using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using System.Collections.Generic;

public class MainMenuNetworkManager : MonoBehaviourPunCallbacks
{
    [Header("Settings")]
    [SerializeField] private int minimumPlayers = 2;

    [Header("buttons")] 
    [SerializeField] private Button joinLobbyButton;
    [SerializeField] private Button[] roomButtons;
    [SerializeField] private Button joinRoomByNameButton;
    [SerializeField] private Button startGameButton;
    
    [Header("other")]
    [SerializeField] private TextMeshProUGUI networkStatusText;
    [SerializeField] private TMP_InputField roomNameInputField;
    [SerializeField] private GameObject currentRoomPanel;
    [SerializeField] private TextMeshProUGUI currentRoomPlayersNumber;
    
    
    private string defaultLobbyName = "DefaultLobby";
    private string defaultRoomName = "DefaultRoom";
    //private string defaultMinimumPlayersString = "2";
    private string gameSceneName = "GameScene";
    
    private void Start()
    {
        joinLobbyButton.interactable = false;
        ToggleJoinRoomButtonsState(false);
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.NickName = "MyName";
        PhotonNetwork.ConnectUsingSettings();
    }

    private void Update()
    {
        networkStatusText.text = PhotonNetwork.NetworkClientState.ToString();
        if(roomNameInputField.text.Length == 0)
            joinRoomByNameButton.interactable = false;
        else
            joinRoomByNameButton.interactable = true;
            
    }
   

    public override void OnConnectedToMaster()
    {
        Debug.Log("We connected to Photon");
        base.OnConnectedToMaster();
        joinLobbyButton.interactable = true;

    }

    public void CreateRoom()
    {
        RoomOptions roomOptions = new RoomOptions()
        {
            MaxPlayers = 4,
        };
        PhotonNetwork.CreateRoom(defaultRoomName,roomOptions);
        ToggleJoinRoomButtonsState(false);
    }

    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
        Debug.Log("Room created successfully!");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
        ToggleJoinRoomButtonsState(true);
    }

    public void JoinLobby()
    {
        PhotonNetwork.JoinLobby(new TypedLobby(defaultLobbyName, LobbyType.Default));
        ToggleJoinRoomButtonsState(true);
        
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        Debug.Log($"We successfully joined the lobby {PhotonNetwork.CurrentLobby}!");
        joinLobbyButton.interactable = false;
    }

    public void JoinRandomRoom()
    {
        PhotonNetwork.JoinRandomRoom();
        ToggleJoinRoomButtonsState(false);
    }

    public void JoinRoomByName()
    {
        PhotonNetwork.JoinRoom(roomNameInputField.text);
        ToggleJoinRoomButtonsState(false);
    }

    public void JoinOrCreateRoom()
    {
        PhotonNetwork.JoinOrCreateRoom(roomNameInputField.text, null, null);
        ToggleJoinRoomButtonsState(false);
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        RefreshCurrentRoomInfo();
        ToggleJoinRoomButtonsState(false);
    }
    
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        RefreshCurrentRoomInfo();
        Debug.Log("We successfully joined the room " + PhotonNetwork.CurrentRoom);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        Debug.LogError($"We couldn't join the room because {message} return code is {returnCode}");
        ToggleJoinRoomButtonsState(true);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        base.OnJoinRandomFailed(returnCode, message);
        ToggleJoinRoomButtonsState(true);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        base.OnRoomListUpdate(roomList);
        foreach (RoomInfo roomInfo in roomList)
        {
            Debug.Log(roomInfo.Name);
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

    

    private void ToggleJoinRoomButtonsState(bool active)
    {
        foreach (Button joinRoomButton in roomButtons)
        {
            joinRoomButton.interactable = active;
        }
    }

    private void RefreshCurrentRoomInfo()
    {
        if (PhotonNetwork.InRoom)
        {
            currentRoomPanel.SetActive(true);
            currentRoomPlayersNumber.SetText(PhotonNetwork.CurrentRoom.PlayerCount + " / " + PhotonNetwork.CurrentRoom.MaxPlayers);
            
            startGameButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
            startGameButton.interactable = PhotonNetwork.CurrentRoom.PlayerCount >= minimumPlayers;
        }
        else
        {
            currentRoomPanel.SetActive(false);
        }
    }
}
