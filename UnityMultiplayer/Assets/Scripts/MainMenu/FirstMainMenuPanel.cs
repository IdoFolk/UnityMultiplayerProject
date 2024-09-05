using System;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FirstMainMenuPanel : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject panelHolder;
    
    [SerializeField] private MainMenuNetworkManager mainMenuNetworkManager;
    
    [SerializeField] private Button EnterLobbyButton;
    
    [SerializeField] private TMP_InputField nicknameInputText;
    [SerializeField] private TMP_InputField lobbyNameInputText;
    
    [SerializeField] private TextMeshProUGUI networkStatusText;

    private bool _hasNickname;
    private bool _hasLobbyName;

    private void Start()
    {
        panelHolder.SetActive(false);
    }

    private void Update()
    {
        if(panelHolder.activeSelf)
            networkStatusText.text = PhotonNetwork.NetworkClientState.ToString();
    }

    public void SubmitNickname()
    {
        string nickname = nicknameInputText.text;
        if (nickname.Length > 0)
            _hasNickname = true;
        else
            _hasNickname = false;
        CheckIfCanEnterLobby();
    }
    
    public void SubmitLobbyName()
    {
        string lobbyName = lobbyNameInputText.text;
        if (lobbyName.Length > 0)
            _hasLobbyName = true;
        else
            _hasLobbyName = false;
        CheckIfCanEnterLobby();
        
    }

    private void CheckIfCanEnterLobby()
    {
        if (_hasNickname && _hasLobbyName)
        {
            EnterLobbyButton.interactable = true;
        }
        else
        {
            EnterLobbyButton.interactable = false;
        }
    }

    public void EnterLobby()
    {
        panelHolder.SetActive(false);
        
        mainMenuNetworkManager.SubmitNickname(nicknameInputText.text);
        mainMenuNetworkManager.SubmitLobbyName(lobbyNameInputText.text);
        
        PhotonNetwork.JoinLobby(new TypedLobby(lobbyNameInputText.text, LobbyType.Default));
    }
    
    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        panelHolder.SetActive(true);
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        networkStatusText.text = "Joined lobby";
    }
}
