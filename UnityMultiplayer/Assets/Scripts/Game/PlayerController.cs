using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class PlayerController : MonoBehaviourPunCallbacks, IPunInstantiateMagicCallback
{    
    private const string TrailObjectName = "Prefabs\\TrailObject";
    private const string TrailObjectTag = "Trail";
    private const string DeathRPC = "Die";
    private const string GameOverRPC = "GameOver";
    
    [SerializeField] private float speed = 10;
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private Color playerColor;
    [SerializeField] private MeshRenderer meshRenderer;

    private List<GameObject> trailObjects;

    private GameNetworkManager manager;
    private Transform trailObjectsParent;

    public void SetManager(GameNetworkManager gameNetworkManager)
    {
        manager = gameNetworkManager;
        trailObjectsParent = manager.TrailObjectsParent;
    }


    private float lifetimeTimer = 0;
    
    private bool movementEnabled = true;
    
    private Camera cachedCamera;
    private Rigidbody _rigidbody;
    private Vector3 raycastPos;
    private Vector3 movementVector = new Vector3();
    private IEnumerator trailCoroutine;
    
    private void Start()
    {
        if (photonView.IsMine)
        {
            _rigidbody = GetComponent<Rigidbody>();
            cachedCamera = Camera.main;
            trailObjects = new List<GameObject>();
            trailCoroutine = LeaveTrail();
            StartCoroutine(trailCoroutine);
        }
    }

    public void DisablePlayer()
    {
        movementEnabled = false;
    }
    private void FixedUpdate()
    {
        lifetimeTimer += Time.fixedDeltaTime;
        //constantly moves the player forward
        if (photonView.IsMine && movementEnabled)
        {
            _rigidbody.linearVelocity = transform.forward * (speed); 
            
            //rotates the player left or right when he presses the arrow keys
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                transform.Rotate(Vector3.up, -rotationSpeed * Time.fixedDeltaTime, Space.World);
            }
        
            if (Input.GetKey(KeyCode.RightArrow))
            {
                transform.Rotate(Vector3.up, rotationSpeed * Time.fixedDeltaTime, Space.World);
            }
        }
    }
    
    /// <summary>
    /// Coroutine that creates a trail object behind the player every 0.5 seconds
    /// </summary>
    /// <returns></returns>
    private IEnumerator LeaveTrail()
    {
        int trailCount = 0;
        yield return new WaitForSeconds(3f);
        while (true)
        {
            GameObject trailObject = PhotonNetwork.Instantiate(TrailObjectName,
                transform.position - transform.forward.normalized * 1.4f, quaternion.identity, 0,
                new object []{ ColorUtility.ToHtmlStringRGB(playerColor)});
            trailObjects.Add(trailObject);
            
            trailCount++;
            if (trailCount > 20)
            {
                yield return new WaitForSeconds(1f);
                trailCount = 0;
            }
            else
                yield return new WaitForSeconds(0.1f);
        }
    }

    /// <summary>
    /// When collides with something, destroys itself and reports its destruction to all other clients.
    /// </summary>
    /// <param name="other"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void OnCollisionEnter(Collision other)
    {
        if (lifetimeTimer < 3)
        {
            return;
        }
        if (photonView.IsMine)
        {
            //photonView.RPC(DeathRPC, RpcTarget.All, photonView.Owner.ActorNumber);
            Die(photonView.Owner.ActorNumber);
        }
    }


    [PunRPC]
    private void Die(int actorNumber)
    {
        if (actorNumber == photonView.Owner.ActorNumber)
        {
            movementEnabled = false;
            //if only one "PlayerController" prefab remains active in the scene, broadcast the GameOver RPC
            if (FindObjectsByType<PlayerController>((FindObjectsSortMode)FindObjectsInactive.Exclude).Length <= 2)
            {
                photonView.RPC(GameOverRPC, RpcTarget.All);
            }
            manager.AddTrailObjectsToList(trailObjects);
            PhotonNetwork.Destroy(gameObject);
            Debug.Log($"Player {photonView.Owner.ActorNumber} died");
        }
    }
    
    

    [PunRPC]
    private void GameOver()
    {
        //manager.GameOver();
    }
    
    /*
    [PunRPC]
    private void ReceiveDamage(int damageAmount)
    {
        HP -= damageAmount;
        Debug.Log("Hp left is " + HP);
    }*/
    
    
    /// <summary>
    /// Why is player color data not reading correctly??? Plz help
    /// </summary>
    /// <param name="info"></param>
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] instantiationData = info.photonView.InstantiationData;
        if (ColorUtility.TryParseHtmlString("#"+(string)instantiationData[0], out Color color))
        {
            playerColor = color;
            meshRenderer.material.color = color;
            //Debug.Log($"Player color changed to {color}.");
        }
        else
        {
            //Debug.Log("Error with player color data.");
        }
    }

    /// <summary>
    /// TODO: Add a timer to these PowerUps and disable/revert them after the timer runs out
    /// </summary>
    /// <param name="type"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void ApplyPowerUp(PowerUp.PowerUpType type)
    {
        switch (type)
        {
            case PowerUp.PowerUpType.SpeedUp:
                speed += 3;
                break;
            case PowerUp.PowerUpType.SlowDown:
                speed -= 3;
                break;
            case PowerUp.PowerUpType.Invincibility:
                GetComponent<Collider>().enabled = false;
                break;
            case PowerUp.PowerUpType.NoTrail:
                StopCoroutine(trailCoroutine);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
        Debug.Log($"Player {photonView.Owner.ActorNumber} PowerUp applied: {type}");
    }

    // public override void OnPlayerLeftRoom(Player otherPlayer)
    // {
    //     base.OnPlayerLeftRoom(otherPlayer);
    //     if(otherPlayer == photonView.Owner)
    //     {
    //         Debug.Log(photonView.Owner.NickName + " object owner");
    //         PhotonNetwork.Destroy(gameObject);
    //     }
    // }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        base.OnMasterClientSwitched(newMasterClient);
        
    }
}