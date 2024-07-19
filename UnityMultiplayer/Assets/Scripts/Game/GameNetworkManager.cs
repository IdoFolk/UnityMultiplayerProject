using System;
using ExitGames.Client.Photon;
using Photon.Pun;
using Unity.Collections;
using UnityEngine;


public class GameNetworkManager : MonoBehaviourPun
{
    [SerializeField] private GameObject characterPickPanel;
    [SerializeField] private GameObject chatPanel;
    [SerializeField] private CharacterPick[] characterPicks;
    public static int CharacterPickedID;
    public static Color CharacterColor;

    private const string CLIENT_PICKED_CHARACTER = nameof(SendCharacterPicked);
    private const string CHARACTER_WAS_PICKED = nameof(CharacterWasPicked);
    private const string SPAWN_CHARACTER = nameof(SpawnCharacter);

    private void Start()
    {
        chatPanel.SetActive(false);
        for (int i = 0; i < characterPicks.Length; i++)
        {
            characterPicks[i].ID = i;
            characterPicks[i].OnPick += SendCharacterPickedToMaster;
        }
    }

    public void SendCharacterPickedToMaster(int CharacterPickedID,Color characterColor)
    {
        photonView.RPC(CLIENT_PICKED_CHARACTER, RpcTarget.MasterClient, CharacterPickedID,characterColor.ToRGBHex());
    }


    [PunRPC]
    private void SendCharacterPicked(int CharacterPickedID, string characterColor, PhotonMessageInfo messageInfo)
    {
        Debug.Log("Master SendCharacterPicked: " + CharacterPickedID);
        foreach (var character in characterPicks)
        {
            if (CharacterPickedID == character.ID && !character.IsTaken)
            {
                photonView.RPC(SPAWN_CHARACTER, messageInfo.Sender, CharacterPickedID,characterColor);
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
    private void SpawnCharacter(int characterId, string characterColor)
    {
        CharacterPickedID = characterId;
        characterPickPanel.gameObject.SetActive(false);
        CharacterColor = characterColor.FromHexToColor();
        chatPanel.SetActive(true);
    }
}