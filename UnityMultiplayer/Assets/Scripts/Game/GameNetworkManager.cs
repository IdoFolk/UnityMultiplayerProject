using Photon.Pun;
using UnityEngine;


    public class GameNetworkManager : MonoBehaviourPun
    {
        [SerializeField] private GameObject characterPickPanel;
        [SerializeField] private CharacterPick[] characterPicks;
        [SerializeField] private int characterPickedID;

        private const string CLIENT_PICKED_CHARACTER = nameof(SendCharacterPicked);
        private const string CHARACTER_WAS_PICKED_ = nameof(CharacterWasPicked);
        
        
        public void SendCharacterPickedToMaster(int CharacterPickedID)
        {
            photonView.RPC(CLIENT_PICKED_CHARACTER, RpcTarget.MasterClient, CharacterPickedID);
            characterPickPanel.gameObject.SetActive(false);
            SpawnIn();
        }
        
        [PunRPC]
        private void SendCharacterPicked(int CharacterPickedID)
        {
            Debug.Log("Mater SendCharacterPicked: " + CharacterPickedID);
            foreach (var character in characterPicks)
            {
                if (CharacterPickedID == character.ID)
                {
                    character.Take();
                    characterPickedID = character.ID;
                    photonView.RPC(CHARACTER_WAS_PICKED_, RpcTarget.All, CharacterPickedID);
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
        
        private void SpawnIn()
        {
            // enter game start logic here
        }
    }
    
