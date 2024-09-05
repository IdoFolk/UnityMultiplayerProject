using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomInfoItem : MonoBehaviour
{
    [SerializeField] private TMP_Text tmpText;
    [SerializeField] private Button button;

    private string roomName;
    
    public void Init(string roomName,string text,bool canJoin)
    {
        tmpText.text = text;
        button.interactable = canJoin;
        this.roomName = roomName;
    }
    
    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(roomName);
    }
}
