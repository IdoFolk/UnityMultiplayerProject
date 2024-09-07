using System;
using System.Collections;
using System.Linq;
using Game;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Random = UnityEngine.Random;


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
    [SerializeField] private Button grantMasterClientButton;
    [SerializeField] private Button startRoundButton;
    [SerializeField] private TMP_Text nicknameText;
    [SerializeField] private SpriteRenderer ArenaBorder;
    private Vector2 nextPowerUpSpawnPosition;
    private TMP_Text startRoundButtonText;
    [field: SerializeField] public Transform TrailObjectsParent { get; private set; }
    public static int CharacterPickedID;
    public static Color CharacterColor;
    public PlayerController currentPlayer { get; set; }

    private const string PlayerPrefabName = "Prefabs\\PlayerPrefab";
    private const string PowerUpPrefabName = "Prefabs\\PowerUpPrefab";
    
    private const string PlayerDisconnectedRPC = nameof(PlayerDisconnected);
    private const string ClientPickedCharacterRPC = nameof(SendCharacterPicked);
    private const string CharacterSlotTakenRPC = nameof(CharacterSlotTaken);
    private const string PreparePlayerForNewRoundRPC = nameof(PreparePlayerForNewRound);
    private const string RoundStartedRPC = nameof(RoundStarted);
    private const string GetNextPowerupSpawnPositionRPC = nameof(GetNextPowerUpSpawnPosition);
    public const string DestroyPowerupRPC = nameof(DestroyPowerUp);
    public const string GameOverRPC = nameof(GameOver);
    public const string PlayerDeathRPC = nameof(OnPlayerDeath);


    private IEnumerator SpawnPowerupsCoroutine;

    private int pickedPlayerCounter;
    private int playersAlive;
    private bool gameEnded;


    private void Start()
    {
        PhotonNetwork.CurrentRoom.PlayerTtl = 30;
        chatPanel.SetActive(false);
        startRoundButtonText = startRoundButton.GetComponentInChildren<TMP_Text>();
        startRoundButtonText.text = "Start Round";
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
    
    public void SendCharacterPickedToMaster(int characterPickedID, Color characterColor)
    {
        photonView.RPC(ClientPickedCharacterRPC, RpcTarget.MasterClient, characterPickedID, characterColor.ToRGBHex());
        CharacterPickedID = characterPickedID;
        CharacterColor = characterColor;
        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable() {{"Color", characterColor.ToRGBHex()}});
        nicknameText.color = characterColor;
        nicknameText.text = $"Playing as: {PhotonNetwork.NickName}";
        characterPickPanel.SetActive(false);
        PreparePlayerForNewRound();
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
                photonView.RPC(CharacterSlotTakenRPC, RpcTarget.All, pickedID);
            }
        }
    }

    [PunRPC]
    private void CharacterSlotTaken(int CharacterPickedID)
    {
        Debug.Log("CharacterSlotTaken: " + CharacterPickedID);
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
    private void RoundStarted()
    {
        currentPlayer.EnablePlayer();
        SpawnPowerupsCoroutine = SpawnPowerUps();
        StartCoroutine(SpawnPowerupsCoroutine);
    }

    [PunRPC]
    private void PreparePlayerForNewRound()
    {
        gameEnded = false;
        startRoundButtonText.text = "Start Round";
        playersAlive = PhotonNetwork.CurrentRoom.PlayerCount;
        SpawnCharacter(CharacterPickedID, CharacterColor);
    }

    [PunRPC]
    public void OnPlayerDeath(string userID)
    {
        playersAlive--;
        //Debug.Log($"player {userID} Died.");
        
        if (PhotonNetwork.IsMasterClient)
        {
            ScoreHandler.Instance.SendPlayerDeathRPC(userID);
            if (playersAlive <= 1)
            {
                photonView.RPC(GameOverRPC, RpcTarget.All);
                //ScoreHandler.Instance.SendGameOverRPC();
            }
        }
    }
    
    [PunRPC]
    public void GameOver()
    {
        if (currentPlayer != null)
            currentPlayer.DisablePlayer();
        if (SpawnPowerupsCoroutine != null)
            StopCoroutine(SpawnPowerupsCoroutine);

        gameEnded = true;
        startRoundButton.interactable = true;
        startRoundButtonText.text = "Next Round";
    }

    [PunRPC]
    private void GetNextPowerUpSpawnPosition(Vector2 position)
    {
        nextPowerUpSpawnPosition = position;
    }

    [PunRPC]
    public void DestroyPowerUp(int viewID)
    {
        PhotonNetwork.Destroy(PhotonView.Find(viewID).gameObject);
    }
    
    [PunRPC]
    public void PlayerDisconnected()
    {
        
    }
    #endregion

    #region RPC Sendings

    public void SendDestroyPowerUpRPC(int viewID)
    {
        photonView.RPC(DestroyPowerupRPC, RpcTarget.MasterClient, viewID);
    }

    public void SendPlayerDeathRPC()
    {
        photonView.RPC(PlayerDeathRPC, RpcTarget.All, SystemInfo.deviceUniqueIdentifier);
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
                spawnPosition = new Vector3(-8, 4, 0); //top left
                spawnRotation = Quaternion.Euler(new Vector3(0, 0, -90)); //face right
                break;
            }
            case 1:
            {
                spawnPosition = new Vector3(8, -4, 0); //bottom right
                spawnRotation = Quaternion.Euler(new Vector3(0, 0, 90)); //face left
                break;
            }
            case 2:
            {
                spawnPosition = new Vector3(-8, -4, 0); //bottom left
                spawnRotation = Quaternion.Euler(new Vector3(0, 0, -90)); //face right
                break;
            }
            case 3:
            {
                spawnPosition = new Vector3(8, 4, 0); //top right
                spawnRotation = Quaternion.Euler(new Vector3(0, 0, 90)); //face left
                break;
            }
            case 4:
            {
                spawnPosition = new Vector3(0, -8, 0); //bottom middle
                spawnRotation = Quaternion.Euler(new Vector3(0, 0, 0)); //face up
                break;
            }
            case 5:
            {
                spawnPosition = new Vector3(0, 8, 0); //top middle
                spawnRotation = Quaternion.Euler(new Vector3(0, 0, 180)); //face down
                break;
            }
            default: 
            {
                spawnPosition = new Vector3(0, 0, 0); //middle
                spawnRotation = Quaternion.Euler(new Vector3(0, 0, 0)); //face up
                break;
            }
        }
        
        PlayerController player = PhotonNetwork.Instantiate(PlayerPrefabName, spawnPosition, spawnRotation,
            group: 0, new object[] { ColorUtility.ToHtmlStringRGB(characterColor) }).GetComponent<PlayerController>();
        chatPanel.SetActive(true);
    }

    /// <summary>
    /// TODO: ADD COUNTDOWN FOR ALL PLAYERS
    /// </summary>
    public void StartGameButtonOnClick()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            ScoreHandler.Instance.SendRoundBeginsRPC();
            if (!gameEnded) //"Start Round"
            {
                photonView.RPC(RoundStartedRPC, RpcTarget.All);
                startRoundButton.interactable = false;
            }
            else //"Next Round"
            {
                CleanBoard();
                photonView.RPC(PreparePlayerForNewRoundRPC, RpcTarget.All);
            }
        }

    }
    
    /// <summary>
    /// The master client spawns random powerups at pre-determined positions every 10 seconds.
    /// If the master client changed, the positions will be transferred to the new master.
    /// Given that all clients run this at the same time, it should synchronize it so if the master client changes mid-round, the spawn interval won't reset.
    /// </summary>
    /// <returns></returns>
    private IEnumerator SpawnPowerUps()
    {
        while (true)
        {
            //if (!PhotonNetwork.IsMasterClient) yield return null;
            if (PhotonNetwork.IsMasterClient)
            {
                Vector2 nextPowerUpSpawnPositionTemp = new Vector2(Random.Range(-9f, 9f), Random.Range(-9f, 9f));
                photonView.RPC(GetNextPowerupSpawnPositionRPC, RpcTarget.All, nextPowerUpSpawnPositionTemp);
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

    private void CleanBoard()
    {
        PhotonNetwork.DestroyAll();
    }
    
    [ContextMenu("leveRoom")]
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene(0);
    }

    public void ToggleBorder(bool on)
    {
        ArenaBorder.gameObject.SetActive(on);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        if (cause == DisconnectCause.ApplicationQuit) return;
        photonView.RPC(PlayerDisconnectedRPC,RpcTarget.MasterClient);
        
        
    }
}