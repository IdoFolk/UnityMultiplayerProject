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
    
    [Header("Panels")] 
    [SerializeField] private GameObject NetworkButtonsPanel;
    [SerializeField] private GameObject EnterNicknamePanel;
    [SerializeField] private GameObject currentRoomPanel;
    [SerializeField] private GameObject lobbyInfoPanel;
    
    [Header("buttons")] 
    [SerializeField] private Button joinLobbyButton;
    [SerializeField] private Button[] roomButtons;
    [SerializeField] private Button joinRoomByNameButton;
    [SerializeField] private Button startGameButton;
    
    [Header("Text")]
    [SerializeField] private TextMeshProUGUI networkStatusText;
    [SerializeField] private TextMeshProUGUI currentRoomPlayersNumber;
    [SerializeField] private TextMeshProUGUI nicknameErrorText;
    [SerializeField] private TextMeshProUGUI lobbyRoomsInfo;
    
    [Header("Input Fields")]
    [SerializeField] private TMP_InputField lobbyNameInputField;
    [SerializeField] private TMP_InputField nicknameInputField;
    [SerializeField] private TMP_InputField roomNameInputField;
    [SerializeField] private TMP_InputField RoomNameInputField; 
    
    private string roomName = "";
    private string lobbyName;
    private string defaultLobbyName = "DefaultLobby";
    private string defaultRoomName = "DefaultRoom";
    private string gameSceneName = "GameScene";
    
    private Dictionary<string,int> roomListInfo = new Dictionary<string, int>(); //<RoomName, PlayerCount>
    
    private void Start()
    {
        joinLobbyButton.interactable = false;
        ToggleJoinRoomButtonsState(false);
        NetworkButtonsPanel.SetActive(false);
        EnterNicknamePanel.SetActive(false);
        PhotonNetwork.AutomaticallySyncScene = true;
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
        EnterNicknamePanel.SetActive(true);
    }
    
    public void SubmitLobbyName()
    {
        lobbyName = lobbyNameInputField.text;
        if (lobbyName.Length >= 1)
        {
            joinLobbyButton.interactable = true;
        }
        else
        {
            joinLobbyButton.interactable = false;
        }
    }

    public void SubmitRoomName()
    {
        roomName = RoomNameInputField.text;
    }

    public void SubmitNickname()
    {
        var nickname = nicknameInputField.text;
        nicknameErrorText.text = "";
        if (nickname.Length < 1)
        {
            nicknameErrorText.text = "Nickname must be at least 1 character long";
            return;
        }
        PhotonNetwork.NickName = nickname;
        EnterNicknamePanel.SetActive(false);
        NetworkButtonsPanel.SetActive(true);
    }
    public void CreateRoom()
    {
        RoomOptions roomOptions = new RoomOptions()
        {
            MaxPlayers = 4,
        };
        if(roomName.Length >= 1)
            PhotonNetwork.CreateRoom(roomName,roomOptions);
        else
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
        if(lobbyName.Length > 1)
            PhotonNetwork.JoinLobby(new TypedLobby(lobbyName, LobbyType.Default));
        else
            PhotonNetwork.JoinLobby(new TypedLobby(defaultLobbyName, LobbyType.Default));
        
        ToggleJoinRoomButtonsState(true);
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        Debug.Log($"We successfully joined the lobby {PhotonNetwork.CurrentLobby}!");
        joinLobbyButton.interactable = false;
        lobbyInfoPanel.SetActive(true);
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
        lobbyInfoPanel.SetActive(false);
        joinRoomByNameButton.interactable = false;
        

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
        Dictionary<string,int> roomListInfoCopy = new Dictionary<string, int>(roomListInfo);
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
                roomListInfo.Add(newRoomInfo.Name, newRoomInfo.PlayerCount);
            }
        }
    
        lobbyRoomsInfo.text = " ";
        foreach (KeyValuePair<string, int> roomInfo in roomListInfo)
        {
            lobbyRoomsInfo.text += roomInfo.Key + " Players: " + roomInfo.Value + "/4" + "\n";
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
