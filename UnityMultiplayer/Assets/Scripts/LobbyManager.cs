using Photon.Pun;
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
    [SerializeField] private TMP_InputField roomNameInput;
    [SerializeField] private Button createRoomButton,joinRoomButton;

    public override void OnEnable()
    {
        base.OnEnable();
        lobbyTitle.text = "Lobby Name: " + PhotonNetwork.CurrentLobby.Name;
        errorText.text = "";
        RoomInputTextChange();
    }

    public void ConnectToRoom()
    {
        NetworkManager.Instance.JoinRoom(roomNameInput.text);
    }
    public void CreateRoom()
    {
        NetworkManager.Instance.CreateRoom(roomNameInput.text);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        if (returnCode.ToString() == noRoomFoundErrorCode)
        {
            errorText.text = "No Room With That Name";
        }
        
    }

    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
        errorText.text = "You Created \"" + PhotonNetwork.CurrentRoom.Name + "\" Room";
    }

    public void RoomInputTextChange()
    {
        if (roomNameInput.text.Length == 0)
        {
            createRoomButton.interactable = false;
            joinRoomButton.interactable = false;
        }
        else
        {
            createRoomButton.interactable = true;
            joinRoomButton.interactable = true;
        }
            
    }
}
