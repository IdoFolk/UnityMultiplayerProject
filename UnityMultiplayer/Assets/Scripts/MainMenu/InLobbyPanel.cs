using System;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MainMenu
{
    public class InLobbyPanel : MonoBehaviourPunCallbacks
    {
        [SerializeField] private GameObject inLobbyPanelHolder;
        
        [SerializeField] private Button joinRoomByNameButton;
        
        [SerializeField] private TMP_InputField roomNameForJoiningRoomInputField;
        

        private void Start()
        {
            inLobbyPanelHolder.SetActive(false);
            joinRoomByNameButton.interactable = false;
        }
        
        public void JoinRoomByName()
        {
            PhotonNetwork.JoinRoom(roomNameForJoiningRoomInputField.text);
        }
        
        public void TryToJoinRandomRoom()
        {
            PhotonNetwork.JoinRandomRoom();
        }

        public void SubmitRoomNameForJoiningRoom()
        {
            if(roomNameForJoiningRoomInputField.text.Length > 0)
                joinRoomByNameButton.interactable = true;
            else
                joinRoomByNameButton.interactable = false;
        }

        public override void OnJoinedLobby()
        {
            base.OnJoinedLobby();
            inLobbyPanelHolder.SetActive(true);
        }

        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();
            inLobbyPanelHolder.SetActive(false);
        }
    }
}