using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = System.Random;


public class GameNetworkManager : MonoBehaviourPunCallbacks
{
    #region Singleton

    public static GameNetworkManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    #endregion

    [SerializeField] private GameObject characterPickPanel;
    [SerializeField] private GameObject chatPanel;
    [SerializeField] private CharacterPick[] characterPicks;
    [SerializeField] private List<Transform> powerupSpawnPositions;
    [SerializeField] private Button grantMasterClientButton;
    [SerializeField] private Button startRoundButton;
    [SerializeField] private TMP_Text nicknameText;
    private Vector3 nextPowerUpSpawnPosition;
    [field: SerializeField] public Transform TrailObjectsParent { get; private set; }
    public static int CharacterPickedID;
    public static Color CharacterColor;
    public static PlayerController currentPlayer { get; private set; }

    private const string PlayerPrefabName = "Prefabs\\PlayerPrefab";
    private const string PowerUpPrefabName = "Prefabs\\PowerUpPrefab";
    private const string CLIENT_PICKED_CHARACTER = nameof(SendCharacterPicked);
    private const string CHARACTER_WAS_PICKED = nameof(CharacterWasPicked);
    private const string RESPAWN_CHARACTER = nameof(RespawnCharacter);
    private const string GAME_STARTED = nameof(GameStarted);
    private const string GET_NEXT_POWERUP_SPAWN_POSITION = nameof(GetNextPowerUpSpawnPosition);

    private const string GameOverRPC = "GameOver";

    private IEnumerator SpawnPowerupsCoroutine;

    private int pickedPlayerCounter;


    private void Start()
    {
        PhotonNetwork.CurrentRoom.PlayerTtl = 30;
        chatPanel.SetActive(false);
        if (!PhotonNetwork.IsMasterClient)
        {
            grantMasterClientButton.gameObject.SetActive(false);
            startRoundButton.gameObject.SetActive(false);
        }
        startRoundButton.interactable = false;
        pickedPlayerCounter = 0;
        for (int i = 0; i < characterPicks.Length; i++)
        {
            characterPicks[i].ID = i;
            characterPicks[i].OnPick += SendCharacterPickedToMaster;
        }
    }
    
    public void SendCharacterPickedToMaster(int CharacterPickedID, Color characterColor)
    {
        photonView.RPC(CLIENT_PICKED_CHARACTER, RpcTarget.MasterClient, CharacterPickedID, characterColor.ToRGBHex());
        SpawnCharacter(CharacterPickedID, characterColor);
        nicknameText.color = characterColor;
        nicknameText.text = $"Playing as: {PhotonNetwork.NickName}";
        characterPickPanel.SetActive(false);
    }
    
    public void GrantMasterClientToNextPlayer()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1) return;
        
        var nextPlayer = PhotonNetwork.CurrentRoom.Players
            .Values
            .FirstOrDefault(player => player.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber + 1) ?? PhotonNetwork.CurrentRoom.Players
            .Values
            .First(player => player.ActorNumber != PhotonNetwork.LocalPlayer.ActorNumber);

        PhotonNetwork.SetMasterClient(nextPlayer);
    }

    #region PunCallbacks

    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        Debug.Log($"Player #{newMasterClient.ActorNumber} is now the Room Manager");

        if (PhotonNetwork.IsMasterClient)
        {
            grantMasterClientButton.gameObject.SetActive(true);
            startRoundButton.gameObject.SetActive(true);
            StartCoroutine(SpawnPowerupsCoroutine);
        }
        else
        {
            grantMasterClientButton.gameObject.SetActive(false);
            startRoundButton.gameObject.SetActive(false);
        }
    }

    #endregion

    #region RPCs

    [PunRPC]
    private void SendCharacterPicked(int pickedID, string characterColor, PhotonMessageInfo messageInfo)
    {
        Debug.Log("Master SendCharacterPicked: " + pickedID);
        foreach (var character in characterPicks)
        {
            if (pickedID == character.ID && !character.IsTaken)
            {
                //photonView.RPC(SPAWN_CHARACTER, messageInfo.Sender, CharacterPickedID,characterColor);
                photonView.RPC(CHARACTER_WAS_PICKED, RpcTarget.All, pickedID);
            }
        }
    }

    [PunRPC]
    private void CharacterWasPicked(int CharacterPickedID)
    {
        Debug.Log("CharacterWasPicked: " + CharacterPickedID);
        foreach (var characterPick in characterPicks)
        {
            if (CharacterPickedID == characterPick.ID)
            {
                characterPick.Take();
            }
        }

        pickedPlayerCounter++;
        if (pickedPlayerCounter >= PhotonNetwork.CurrentRoom.PlayerCount)
        {
            startRoundButton.interactable = true;
        }
    }

    [PunRPC]
    private void GameStarted()
    {
        currentPlayer.EnablePlayer();
        SpawnPowerupsCoroutine = SpawnPowerUps();
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(SpawnPowerupsCoroutine);
        }
    }

    [PunRPC]
    private void RespawnCharacter()
    {
        SpawnCharacter(CharacterPickedID, CharacterColor);
    }

    /// <summary>
    /// Deletes all trail objects and destroys the player
    /// </summary>
    [PunRPC]
    public void GameOver()
    {
        //StopCoroutine(SpawnPowerupsCoroutine);

        if (null != currentPlayer)
        {
            PhotonNetwork.Destroy(currentPlayer.gameObject);
        }

        //TODO: change this temp
        photonView.RPC(RESPAWN_CHARACTER, RpcTarget.All);
        startRoundButton.interactable = true;
    }

    [PunRPC]
    private void GetNextPowerUpSpawnPosition(Vector3 position)
    {
        nextPowerUpSpawnPosition = position;
    }

    #endregion
    
    private void SpawnCharacter(int characterId, Color characterColor)
    {
        CharacterPickedID = characterId;
        CharacterColor = characterColor;
        characterPickPanel.gameObject.SetActive(false);
        Vector3 spawnPosition;
        Quaternion spawnRotation;
        switch (CharacterPickedID)
        {
            case 0:
            {
                spawnPosition = new Vector3(-8, 4, 0);
                spawnRotation = Quaternion.LookRotation(Vector2.right);
                break;
            }
            case 1:
            {
                spawnPosition = new Vector3(8, -4, 0);
                spawnRotation = Quaternion.LookRotation(Vector2.left);
                break;
            }
            case 2:
            {
                spawnPosition = new Vector3(-8, -4, 0);
                spawnRotation = Quaternion.LookRotation(Vector2.right);
                break;
            }
            case 3:
            {
                spawnPosition = new Vector3(8, 4, 0);
                spawnRotation = Quaternion.LookRotation(Vector2.left);
                break;
            }
            case 4:
            {
                spawnPosition = new Vector3(0, -8, 0);
                spawnRotation = Quaternion.LookRotation(Vector2.up);
                break;
            }
            case 5:
            {
                spawnPosition = new Vector3(0, 8, 0);
                spawnRotation = Quaternion.LookRotation(Vector2.down);
                break;
            }
            default: 
            {
                spawnPosition = new Vector3(0, 0, 0);
                spawnRotation = Quaternion.LookRotation(Vector2.up);
                break;
            }
        }
        
        PlayerController player = PhotonNetwork.Instantiate(PlayerPrefabName, spawnPosition, spawnRotation,
            group: 0, new object[] { ColorUtility.ToHtmlStringRGB(characterColor) }).GetComponent<PlayerController>();
        player.SetManager(this);
        chatPanel.SetActive(true);
        currentPlayer = player;
    }

    /// <summary>
    /// TODO: ADD COUNTDOWN FOR ALL PLAYERS
    /// </summary>
    public void StartGameButtonOnClick()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC(GAME_STARTED, RpcTarget.All);
        }

        startRoundButton.interactable = false;
    }
    
    /// <summary>
    /// The master client spawns random powerups at pre-determined positions every 10 seconds.
    /// If the master client changed, the positions will be transferred to the new master
    /// </summary>
    /// <returns></returns>
    private IEnumerator SpawnPowerUps()
    {
        while (true)
        {
            if (!PhotonNetwork.IsMasterClient) yield return null;
            if (PhotonNetwork.IsMasterClient)
            {
                var nextPowerUpSpawnPositiontemp = powerupSpawnPositions[UnityEngine.Random.Range(0, powerupSpawnPositions.Count)].position;
                photonView.RPC(GET_NEXT_POWERUP_SPAWN_POSITION, RpcTarget.All, nextPowerUpSpawnPositiontemp);
            }
            
            yield return new WaitForSeconds(5f);
            
            if (PhotonNetwork.IsMasterClient)
            {   
                
                GameObject powerUp = PhotonNetwork.InstantiateRoomObject(PowerUpPrefabName,
                    nextPowerUpSpawnPosition, quaternion.identity, 0,
                    new object []{ UnityEngine.Random.Range(0, Enum.GetValues(typeof(PowerUp.PowerUpType)).Length)});
            }
           
        }
    
    }
    
    
    
    [ContextMenu("leveRoom")]
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene(0);
    }
}