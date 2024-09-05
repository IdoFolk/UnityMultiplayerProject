
/*
using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class MultiplayerGameManager : MonoBehaviourPunCallbacks
{
    private const string PlayerPrefabName = "Prefabs\\Player Prefab";
    private const string PhysicalObjectPrefabName = "Prefabs\\Physical Obstacle";

    [Header("Spawn Points")] [SerializeField]
    private bool randomizeSpawnPoint;

    [SerializeField] private SpawnPoint[] randomPowerUpSpawnPoints;

    [SerializeField] private SpawnPoint defaultSpawnPoint;

    [Header("UI")] [SerializeField] private Button readyButton;

    private const string CLIENT_IS_READY_RPC = nameof(ClientIsReady);
    private const string SET_SPAWN_POINT_RPC = nameof(SetSpawnPoint);
    private const string GAME_STARTED_RPC = nameof(GameStarted);

    private PlayerController myPlayerController;

    private int playersReady = 0;

    [SerializeField] List<int> randomWeaponsToSpawn;

    private string roomName;

    public void SendReadyToMasterClient()
    {
        photonView.RPC(CLIENT_IS_READY_RPC, RpcTarget.MasterClient);
        readyButton.interactable = false;
    }

    [ContextMenu("Switch Master Client")]
    public void ChangeMasterClient()
    {
        Player candidateMC = PhotonNetwork.LocalPlayer.GetNext();

        bool success = PhotonNetwork.SetMasterClient(candidateMC);
        Debug.Log("set master client result " + success);
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        base.OnMasterClientSwitched(newMasterClient);
        Debug.Log("New master client is " + newMasterClient + ", all hail him!");
    }
    
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        Debug.Log($"Player {otherPlayer} has left the room with inactive {otherPlayer.IsInactive}");
    }
    
    public void SpawnNextRandomWeapon()
    {
        if (randomWeaponsToSpawn.Count > 0)
        {
            int nextWeaponIndex = randomWeaponsToSpawn[0];
            randomWeaponsToSpawn.RemoveAt(0);
            Debug.Log("Imagine we spawn here weapon index" + nextWeaponIndex);
        }
    }
    
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom(true);
    }

    public void RejoinRoom()
    {
        PhotonNetwork.RejoinRoom(roomName);
    }
    
    private void Start()
    {
        roomName = PhotonNetwork.CurrentRoom.Name;
        SendReadyToMasterClient();

        if (PhotonNetwork.IsMasterClient)
        {
            SpawnRoomObjectsMethodB();
        }
    }

    private void SpawnRoomObjectsMethodA()
    {
        PhotonNetwork.Instantiate(PhysicalObjectPrefabName, Vector3.zero, Quaternion.identity);
    }

    private void SpawnRoomObjectsMethodB()
    {
        PhotonNetwork.InstantiateRoomObject(PhysicalObjectPrefabName, Vector3.zero, Quaternion.identity);
    }


    SpawnPoint GetRandomSpawnPoint()
    {
        List<SpawnPoint> availableSpawnPoints = new List<SpawnPoint>();

        foreach (var spawnPoint in randomPowerUpSpawnPoints)
        {
            if (!spawnPoint.IsTaken)
            {
                availableSpawnPoints.Add(spawnPoint);
            }
        }

        if (availableSpawnPoints.Count == 0)
        {
            Debug.LogError("All spawn points are taken!");
        }

        int index = Random.Range(0, availableSpawnPoints.Count);
        return availableSpawnPoints[index];
    }

    void SpawnPlayer(SpawnPoint targetSpawnPoint)
    {
        targetSpawnPoint.Take();
        GameObject playerGO = PhotonNetwork.Instantiate(PlayerPrefabName,
            targetSpawnPoint.transform.position, targetSpawnPoint.transform.rotation);
        myPlayerController = playerGO.GetComponent<PlayerController>();
    }

    //Method A
    // [PunRPC]
    // private void ClientIsReady()
    // {
    //     Debug.Log("A Client is ready!");
    // }

    //Method B

    #region RPCS

    [PunRPC]
    private void ClientIsReady(PhotonMessageInfo messageInfo)
    {
        Debug.Log(messageInfo.Sender + " Is ready");
        SpawnPoint randomSpawnPoint = GetRandomSpawnPoint();
        randomSpawnPoint.Take();

        messageInfo.photonView.RPC(SET_SPAWN_POINT_RPC, messageInfo.Sender, randomSpawnPoint.ID);

        playersReady++;
        if (playersReady >= PhotonNetwork.CurrentRoom.PlayerCount)
        {
            photonView.RPC(GAME_STARTED_RPC, RpcTarget.All);
        }
    }

    /// <summary>
    /// This will be invoked on the client
    /// </summary>
    /// <param name="spawnPoint"></param>
    [PunRPC]
    private void SetSpawnPoint(int spawnPointID)
    {
        foreach (var spawnPoint in randomPowerUpSpawnPoints)
        {
            if (spawnPoint.ID == spawnPointID)
            {
                SpawnPlayer(spawnPoint);
                break;
            }
        }
    }

    [PunRPC]
    private void GameStarted()
    {
        myPlayerController.enabled = true;
        Debug.Log("The might master client has the Game Started");
    }

    [PunRPC]
    void RPCTest()
    {
        Debug.Log("RPC TEST");
    }
   #endregion
   
   [ContextMenu("SendRPCTest")]
   void SendRPCTest()
   {
       photonView.RPC("RPCTest", RpcTarget.AllBuffered);
   }
}
*/

