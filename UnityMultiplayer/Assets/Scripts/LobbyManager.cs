using Photon.Pun;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject _lobbyRoomHolder;
    [SerializeField] private TextMeshProUGUI lobbyTitle;
    [SerializeField] private TextMeshProUGUI errorText;

    public override void OnEnable()
    {
        base.OnEnable();
        lobbyTitle.text = "Lobby Name: " + PhotonNetwork.CurrentLobby.Name;
        errorText.text = "";
    }
}
