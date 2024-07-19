using ExitGames.Client.Photon;
using Photon.Pun;
using Unity.Collections;
using UnityEngine;


public class GameNetworkManager : MonoBehaviourPun
{
    [SerializeField] private GameObject characterPickPanel;
    [SerializeField] private CharacterPick[] characterPicks;
    [ReadOnly] private int characterPickedID;

    private const string CLIENT_PICKED_CHARACTER = nameof(SendCharacterPicked);
    private const string CHARACTER_WAS_PICKED = nameof(CharacterWasPicked);
    private const string SPAWN_CHARACTER = nameof(SpawnCharacter);


    public void SendCharacterPickedToMaster(int CharacterPickedID)
    {
        photonView.RPC(CLIENT_PICKED_CHARACTER, RpcTarget.MasterClient, CharacterPickedID);
        characterPickPanel.gameObject.SetActive(false);
        //Color
    }


    [PunRPC]
    private void SendCharacterPicked(int CharacterPickedID, PhotonMessageInfo messageInfo)
    {
        Debug.Log("Master SendCharacterPicked: " + CharacterPickedID);
        foreach (var character in characterPicks)
        {
            if (CharacterPickedID == character.ID && !character.IsTaken)
            {
                photonView.RPC(SPAWN_CHARACTER, messageInfo.Sender, CharacterPickedID);
                photonView.RPC(CHARACTER_WAS_PICKED, RpcTarget.All, CharacterPickedID);
            }
        }
    }

    [PunRPC]
    private void CharacterWasPicked(int CharacterPickedID)
    {
        Debug.Log("CharacterWasPicked: " + CharacterPickedID);
        foreach (var character in characterPicks)
        {
            if (CharacterPickedID == character.ID)
            {
                character.Take();
            }
        }
    }

    [PunRPC]
    private void SpawnCharacter(int characterId)
    {
        characterPickedID = characterId;
    }
}