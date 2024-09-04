using System;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MainMenu
{
    public class CreateRoomPanel : MonoBehaviourPunCallbacks
    {
        [SerializeField] private TMP_InputField roomNameInputField;
        [SerializeField] private Button createRoomButton;
        [SerializeField] private Slider roomPlayerNumberSlider;
        [SerializeField] private TMP_Text roomPlayerNumberText;

        private void Update()
        {
            roomPlayerNumberText.text = roomPlayerNumberSlider.value.ToString();
        }

        public void SubmitRoomName()
        {
            if (roomNameInputField.text.Length > 0)
                createRoomButton.interactable = true;
            else
                createRoomButton.interactable = false;
        }
        
        public void CreateRoom()
        {
            RoomOptions roomOptions = new RoomOptions()
            {
                MaxPlayers = (int)roomPlayerNumberSlider.value,
            };
            
            PhotonNetwork.CreateRoom(roomNameInputField.text,roomOptions);
            
        }
    }
}