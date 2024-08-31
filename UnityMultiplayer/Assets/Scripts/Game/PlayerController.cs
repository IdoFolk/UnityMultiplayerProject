using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class PlayerController : MonoBehaviourPunCallbacks, IPunInstantiateMagicCallback, IPunObservable
{    
    private const string TrailObjectName = "Prefabs\\TrailPrefab";
    private const string TrailObjectTag = "Trail";
    private const string DeathRPC = "Die";
    private const string GameOverRPC = "GameOver";
    
    [SerializeField] private float speed = 10;
    [SerializeField] private float rotationSpeed = 90f;
    [field: SerializeField] private float gapTimer = 3f;
    [field: SerializeField] private float gapDuration = 0.75f;
    [field: SerializeField] private LayerMask TrailLayer;
    [field: SerializeField] private LayerMask BorderLayer;
    [SerializeField] private Color playerColor;
    [SerializeField] private Collider2D playerCollisionCollider;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private TrailCollider trailPrefab;
    
    private TrailCollider currentTrailPrefab;

    private List<GameObject> trails;

    private GameNetworkManager manager;
    private Transform trailObjectsParent;
    private float _gapCooldownTimer;
    private float _gapDurationTimer;
    private bool borderTeleportActive = false;

    public void SetManager(GameNetworkManager gameNetworkManager)
    {
        manager = gameNetworkManager;
        trailObjectsParent = manager.TrailObjectsParent;
    }
    
    private bool movementEnabled = true;
    
    private Camera cachedCamera;
    private Rigidbody2D rb2d;
    private bool emittingTrail = false;
    private float horizontalInput;
    private bool ignoreNextCollisionOnTeleport = false;
    
    private void Start()
    {
        if (photonView.IsMine)
        {
            rb2d = GetComponent<Rigidbody2D>();
            playerCollisionCollider = GetComponent<Collider2D>();
            cachedCamera = Camera.main;
            _gapCooldownTimer = gapTimer;
            _gapDurationTimer = gapDuration;
            trails = new List<GameObject>();
            DisablePlayer();
        }
    }

    public void DisablePlayer()
    {
        movementEnabled = false;
    }

    public void EnablePlayer()
    {
        movementEnabled = true;
        SpawnTrail();
    }

    private void Update()
    {
        if (photonView.IsMine && movementEnabled)
        {
            horizontalInput = Input.GetAxis("Horizontal");
            transform.Rotate(Vector3.forward * (-horizontalInput * rotationSpeed * Time.fixedDeltaTime), Space.Self);
            if (_gapCooldownTimer > 0 && currentTrailPrefab != null)
            {
                currentTrailPrefab.UpdateTrail();
            
                _gapCooldownTimer -= Time.deltaTime;
                if (_gapCooldownTimer <= 0 && emittingTrail)
                {
                    StartCoroutine(DetachTrail(gapDuration));
                }
            }
        }
        //DEBUG
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            movementEnabled = !movementEnabled;
            playerCollisionCollider.enabled = !playerCollisionCollider.enabled;
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
    
    private IEnumerator DetachTrail(float duration)
    {
        currentTrailPrefab.StopUpdatingTrail();
        currentTrailPrefab = null;
        emittingTrail = false;
        _gapDurationTimer = duration;
        while (_gapDurationTimer > 0)
        {
            _gapDurationTimer -= Time.deltaTime;
            yield return null;
        }
        SpawnTrail();
        _gapCooldownTimer = gapTimer;
    }

    private void SpawnTrail()
    {
        currentTrailPrefab = PhotonNetwork.Instantiate(TrailObjectName, Vector3.zero, quaternion.identity, 0,
            new object []{ ColorUtility.ToHtmlStringRGB(playerColor)}).GetComponent<TrailCollider>();
        currentTrailPrefab.player = transform;
        trails.Add(currentTrailPrefab.gameObject);
        emittingTrail = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Collision");
        if (photonView.IsMine)
        {
            if (other.gameObject.layer == BorderLayer)
            {
                if (borderTeleportActive)
                {
                    if (ignoreNextCollisionOnTeleport)
                    {
                        ignoreNextCollisionOnTeleport = false;
                        return;
                    }
                    ignoreNextCollisionOnTeleport = true;
                    float newX = transform.position.x;
                    float newY = transform.position.y;
                    if (MathF.Abs(transform.position.x) > 9.5f)
                    {
                        newX *= -1;
                    }
                    if (MathF.Abs(transform.position.x) > 9.5f)
                    {
                        newY *= -1;
                    }
                    
                    transform.position = new Vector2(newX, newY);
                }
                else
                {
                    Die(photonView.Owner.ActorNumber);
                }
            }
            else if (other.gameObject.layer == TrailLayer)
            {
                Die(photonView.Owner.ActorNumber);
            }
            //photonView.RPC(DeathRPC, RpcTarget.All, photonView.Owner.ActorNumber);
        }
    }


    [PunRPC]
    public void Die(int actorNumber)
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
    public void GameOver()
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
                speed *= 1.5f;
                break;
            case PowerUp.PowerUpType.SlowDown:
                speed *= 0.6666667F;
                break;
            case PowerUp.PowerUpType.Invincibility:
                playerCollisionCollider.enabled = false;
                spriteRenderer.color = Color.black;
                break;
            case PowerUp.PowerUpType.NoTrail:
                if (emittingTrail)
                    StartCoroutine(DetachTrail(PowerUp.PowerUpDuration));
                else
                    _gapDurationTimer = PowerUp.PowerUpDuration;
                break;
            case PowerUp.PowerUpType.BorderTeleport:
                borderTeleportActive = true;
                break;
            default:
                break;
        }
        StartCoroutine(DisablePowerUpAfterDuration(type, PowerUp.PowerUpDuration));
        Debug.Log($"Player {photonView.Owner.ActorNumber} PowerUp applied: {type.ToString()}");
    }

    //TODO: Cause the Invincibility and BorderTeleport PowerUps to refresh duration on repeated pickups
    public IEnumerator DisablePowerUpAfterDuration(PowerUp.PowerUpType type, float duration)
    {
        yield return new WaitForSeconds(duration);
        switch (type)
        {
            case PowerUp.PowerUpType.SpeedUp:
                speed *= 0.6666667F;
                break;
            case PowerUp.PowerUpType.SlowDown:
                speed *= 1.5f;
                break;
            case PowerUp.PowerUpType.Invincibility:
                playerCollisionCollider.enabled = true;
                spriteRenderer.color = playerColor;
                break;
            case PowerUp.PowerUpType.NoTrail:
                break;
            case PowerUp.PowerUpType.BorderTeleport:
                borderTeleportActive = false;
                break;
            default:
                break;
        }
        Debug.Log($"Player {photonView.Owner.ActorNumber} PowerUp ended: {type.ToString()}");
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

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            //We own this player: send the others our data
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            //Network player, receive data
            correctPlayerPos = (Vector3)stream.ReceiveNext();
            correctPlayerRot = (Quaternion)stream.ReceiveNext();
        }
    }

    private Vector3 correctPlayerPos = Vector3.zero; //We lerp towards this
    private Quaternion correctPlayerRot = Quaternion.identity; //We lerp towards this

    public void SyncRemotePlayerPosition()
    {
        if (!photonView.IsMine)
        {
            transform.position = correctPlayerPos;
            transform.rotation = correctPlayerRot;
        }
    }
}