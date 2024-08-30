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
    private const string TrailObjectName = "Prefabs\\TrailPrefab";
    private const string TrailObjectTag = "Trail";
    private const string DeathRPC = "Die";
    private const string GameOverRPC = "GameOver";
    
    [SerializeField] private float speed = 10;
    [SerializeField] private float rotationSpeed = 90f;
    [field: SerializeField] private float gapTimer = 3f;
    [field: SerializeField] private float gapDuration = 0.75f;
    [SerializeField] private Color playerColor;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private TrailCollider trailPrefab;
    
    private TrailCollider currentTrailPrefab;

    private List<GameObject> trails;

    private GameNetworkManager manager;
    private Transform trailObjectsParent;
    private float _gapCountdownTimer;

    public void SetManager(GameNetworkManager gameNetworkManager)
    {
        manager = gameNetworkManager;
        trailObjectsParent = manager.TrailObjectsParent;
    }


    private float lifetimeTimer = 0;
    
    private bool movementEnabled = true;
    
    private Camera cachedCamera;
    private Rigidbody2D rb2d;
    private IEnumerator trailCoroutine;
    private float horizontalInput;
    
    private void Start()
    {
        if (photonView.IsMine)
        {
            rb2d = GetComponent<Rigidbody2D>();
            cachedCamera = Camera.main;
            _gapCountdownTimer = gapTimer;
            trails = new List<GameObject>();
            SpawnTrail();
        }
    }

    public void DisablePlayer()
    {
        movementEnabled = false;
    }

    private void Update()
    {
        if (photonView.IsMine && movementEnabled)
        {
            horizontalInput = Input.GetAxis("Horizontal");
            transform.Rotate(Vector3.forward * (-horizontalInput * rotationSpeed * Time.fixedDeltaTime), Space.Self);
            if (_gapCountdownTimer > 0 && currentTrailPrefab != null)
            {
                currentTrailPrefab.UpdateTrail();
            
                _gapCountdownTimer -= Time.deltaTime;
                if (_gapCountdownTimer <= 0)
                {
                    StartCoroutine(DetachTrail());
                }
            }
            
        }
    }

    private void FixedUpdate()
    {
        if (photonView.IsMine)
        {
            if (movementEnabled)
                rb2d.velocity = transform.up * (speed * Time.fixedDeltaTime);
            else
                rb2d.velocity = Vector2.zero;
        }
    }
    
    private IEnumerator DetachTrail()
    {
        currentTrailPrefab = null;
        yield return new WaitForSeconds(gapDuration);
        SpawnTrail();
        _gapCountdownTimer = gapTimer;
    }

    private void SpawnTrail()
    {
        currentTrailPrefab = Instantiate(trailPrefab, Vector3.zero, quaternion.identity);
        /*currentTrailPrefab = PhotonNetwork.Instantiate(TrailObjectName, transform.position, quaternion.identity, 0,
            new object []{ ColorUtility.ToHtmlStringRGB(playerColor)}).GetComponent<TrailCollider>();*/
        currentTrailPrefab.player = transform;
        trails.Add(currentTrailPrefab.gameObject);
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
            trails.Add(trailObject);
            
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Collision");
        if (photonView.IsMine && other.CompareTag("PlayerCollision"))
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
    
    
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] instantiationData = info.photonView.InstantiationData;
        if (ColorUtility.TryParseHtmlString("#"+(string)instantiationData[0], out Color color))
        {
            playerColor = color;
            spriteRenderer.color = color;
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