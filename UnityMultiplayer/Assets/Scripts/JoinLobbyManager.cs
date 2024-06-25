using Photon.Pun;
using TMPro;
using UnityEngine;

public class JoinLobbyManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private TMP_InputField _lobbyNameInput;
    [SerializeField] private GameObject _joinLobbyHolder;
    [SerializeField] private GameObject _lobbyRoomHolder;
    [SerializeField] private TextMeshProUGUI errorText;

    public override void OnEnable()
    {
        base.OnEnable();
        errorText.text = "";
    }

    public void JoinLobby()
    {
        NetworkManager.Instance.JoinLobby(_lobbyNameInput.text);
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        _joinLobbyHolder.SetActive(false);
        _lobbyRoomHolder.SetActive(true);
        
    }
}
