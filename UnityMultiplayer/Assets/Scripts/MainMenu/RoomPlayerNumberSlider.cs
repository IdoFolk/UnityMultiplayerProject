using System;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomPlayerNumberSlider : MonoBehaviourPunCallbacks
{
   [SerializeField] private TextMeshProUGUI maxPlayerNumberText;
   [SerializeField] private Slider playerNumberSlider;
   
   public static event Action<int> OnMaxPlayerNumberChanged ;
   
   public void SetMaxPlayerNumberText()
   {
       maxPlayerNumberText.text = playerNumberSlider.value.ToString();
       OnMaxPlayerNumberChanged?.Invoke((int)playerNumberSlider.value);
   }

   public override void OnJoinedRoom()
   {
       base.OnJoinedRoom();
       playerNumberSlider.interactable = false;
   }
}
