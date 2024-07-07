using System;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class MainMenuManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private TextMeshProUGUI _splashText;
    [SerializeField] private TextMeshProUGUI _loadingText;
    [SerializeField] private GameObject _mainMenuHolder;
    [SerializeField] private GameObject _LobbyRoomHolder;

    private bool flag = false;

    private void Start()
    {
        _splashText.gameObject.SetActive(true);
        _loadingText.gameObject.SetActive(false);
        _mainMenuHolder.SetActive(false);
        InputSystem.onAnyButtonPress.CallOnce(ConnectToServer);
    }

    private void ConnectToServer(InputControl obj)
    {
        _splashText.gameObject.SetActive(false);
        _loadingText.gameObject.SetActive(true);
        NetworkManager.Instance.Connect();
    }

    public void EnterLobbyRoom()
    {
        _mainMenuHolder.SetActive(false);
        _LobbyRoomHolder.SetActive(true);
        
    }

    public override void OnConnected()
    {
        if (!flag)
        {
            _loadingText.gameObject.SetActive(false);
            _mainMenuHolder.SetActive(true);
            base.OnConnected();
            flag = true;
        }
    }
}
