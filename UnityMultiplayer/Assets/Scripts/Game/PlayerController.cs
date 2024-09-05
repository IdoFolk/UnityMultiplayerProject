using System;
using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using Unity.Mathematics;
using UnityEngine;
using Photon.Realtime;
using ExitGames.Client.Photon;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerController : MonoBehaviourPunCallbacks, IPunInstantiateMagicCallback/*, IPunObservable*/
{    
    private const string TrailObjectName = "Prefabs\\TrailPrefab";
    
    [SerializeField] private float speed = 10;
    [SerializeField] private float rotationSpeed = 90f;
    [field: SerializeField] private float gapTimer = 3f;
    [field: SerializeField] private float gapDuration = 0.75f;
    [field: SerializeField] private LayerMask TrailLayer;
    [field: SerializeField] private LayerMask BorderLayer;
    [SerializeField] private Color playerColor;
    [SerializeField] private Collider2D playerCollisionCollider;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private SpriteRenderer outlineSpriteRenderer;
    [SerializeField] private TrailCollider trailPrefab;
    
    private TrailCollider currentTrailPrefab;

    private GameNetworkManager manager;
    private float _gapCooldownTimer;
    private float _gapDurationTimer;
    private bool borderTeleportActive = false;
    private bool invincibilityActive = false;
    
    /// <summary>
    /// Disabled at the start of the round, or when the player is killed
    /// </summary>
    private bool playerAlive = true;
    
    private Camera cachedCamera;
    private Rigidbody2D rb2d;
    private bool emittingTrail = false;
    private float horizontalInput;
    private bool ignoreNextCollisionOnTeleport = false;

    private void Awake()
    {
        DisablePlayer();
    }

    private void Start()
    {
        if (photonView.IsMine)
        {
            manager = GameNetworkManager.Instance;
            manager.currentPlayer = this;
            rb2d = GetComponent<Rigidbody2D>();
            playerCollisionCollider = GetComponent<Collider2D>();
            cachedCamera = Camera.main;
            _gapCooldownTimer = gapTimer;
            _gapDurationTimer = gapDuration;
            playerColor = GameNetworkManager.CharacterColor;
            spriteRenderer.color = playerColor;
        }
        Hashtable roomProperties = PhotonNetwork.CurrentRoom.CustomProperties;
        speed = GetSpeedBasedOnDifficulty(roomProperties["difficulty"].ToString());
    }

    public void DisablePlayer()
    {
        playerAlive = false;
    }

    public void EnablePlayer()
    {
        playerAlive = true;
        SpawnTrail();
    }

    private void Update()
    {
        if (photonView.IsMine && playerAlive)
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
    }

    private void FixedUpdate()
    {
        if (photonView.IsMine)
        {
            if (playerAlive)
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

        if (playerAlive)
        {
            SpawnTrail();
        }
        _gapCooldownTimer = gapTimer;
    }

    private void SpawnTrail()
    {
        currentTrailPrefab = PhotonNetwork.Instantiate(TrailObjectName, Vector3.zero, quaternion.identity, 0,
            new object []{ ColorUtility.ToHtmlStringRGB(playerColor)}).GetComponent<TrailCollider>();
        emittingTrail = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (photonView.IsMine && playerAlive)
        {
            Debug.Log($"Trigger entered: {other.gameObject.name}");
            
            if ((BorderLayer & (1 << other.gameObject.layer)) != 0)
            {
                if (borderTeleportActive)
                {
                    /*if (ignoreNextCollisionOnTeleport)
                    {
                        ignoreNextCollisionOnTeleport = false;
                        return;
                    }
                    ignoreNextCollisionOnTeleport = true;*/
                    float newX = transform.position.x;
                    float newY = transform.position.y;
                    if (MathF.Abs(transform.position.x) > 9.7f)
                    {
                        newX *= -1;
                    }
                    if (MathF.Abs(transform.position.y) > 9.7f)
                    {
                        newY *= -1;
                    }

                    if (emittingTrail)
                    {
                        StartCoroutine(DetachTrail(0.1f));
                    }

                    transform.position = new Vector2(newX, newY);
                    Debug.Log("Teleport");
                }
                else
                {
                    Die();
                }
            }
            
            else if ((TrailLayer & (1 << other.gameObject.layer)) != 0)
            {
                //There isn't a check for the Invincibility power-up, because it simply disables the collider
                Die();
            }
            
            else if (other.gameObject.CompareTag("PowerUp") && other.TryGetComponent(out PowerUp powerUp))
            {
                ApplyPowerUp(powerUp.type);
                manager.SendDestroyPowerUpRPC(powerUp.viewID);
            }
        }
    }
    
    private void Die()
    {
        if (playerAlive)
        {
            playerAlive = false;
            if (borderTeleportActive) manager.ToggleBorder(true);
            manager.SendPlayerDeathRPC();
        }
    }
    
    public void ApplyPowerUp(PowerUp.PowerUpType type)
    {
        if (!playerAlive) return;
        switch (type)
        {
            case PowerUp.PowerUpType.SpeedUp:
                speed *= 1.5f;
                break;
            case PowerUp.PowerUpType.SlowDown:
                speed *= 0.6666667F;
                break;
            case PowerUp.PowerUpType.Invincibility:
                invincibilityActive = true;
                playerCollisionCollider.enabled = false;
                spriteRenderer.color = new Color(.15f, .15f, .15f, 1f);
                break;
            case PowerUp.PowerUpType.NoTrail:
                if (emittingTrail)
                    StartCoroutine(DetachTrail(PowerUp.PowerUpDuration));
                else
                    _gapDurationTimer = PowerUp.PowerUpDuration;
                break;
            case PowerUp.PowerUpType.BorderTeleport:
                borderTeleportActive = true;
                outlineSpriteRenderer.color = Color.clear;
                manager.ToggleBorder(false);
                break;
            default:
                break;
        }
        StartCoroutine(DisablePowerUpAfterDuration(type, PowerUp.PowerUpDuration));
        Debug.Log($"Player {photonView.Owner.ActorNumber} PowerUp applied: {type.ToString()}");
    }

    //TODO: Cause the Invincibility and BorderTeleport PowerUps to refresh duration on repeated pickups.
    //Also TODO: Display the remaining duration around the player (BotW stamina-meter style)
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
                outlineSpriteRenderer.color = Color.white;
                manager.ToggleBorder(true);
                break;
            default:
                break;
        }
        Debug.Log($"Player {photonView.Owner.ActorNumber} PowerUp ended: {type.ToString()}");
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] instantiationData = info.photonView.InstantiationData;
        if (ColorUtility.TryParseHtmlString("#"+(string)instantiationData[0], out Color color))
        {
            playerColor = color;
            spriteRenderer.color = color;
        }
        else
        {
            Debug.LogWarning($"Error with processing player color: {PhotonNetwork.NickName}");
        }
    }
    
    //TODO: FIX THIS IN ORDER TO SYNCHRONIZE POWERUP UPTIME WITH OTHER PLAYERS. AFTER FIX, UN-COMMENT THE IPunObservable interface
    
    /*/// <summary>
    /// Serializes the player's invincibility and border teleport status 
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="info"></param>
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (!playerAlive) return;
        if (stream.IsWriting)
        {
            stream.SendNext(invincibilityActive);
            stream.SendNext(borderTeleportActive);
        }
        else if (stream.IsReading)
        {
            //Network player, receive data
            bool prevInvincibilityActive = invincibilityActive;
            invincibilityActive = (bool)stream.ReceiveNext();
            if (prevInvincibilityActive != invincibilityActive)
            {
                spriteRenderer.color = invincibilityActive ? new Color(.15f, .15f, .15f, 1f) : playerColor;
            }
            bool prevBorderTeleportActive = borderTeleportActive;
            borderTeleportActive = (bool)stream.ReceiveNext();
            if (prevBorderTeleportActive != borderTeleportActive)
            {
                outlineSpriteRenderer.color = borderTeleportActive ? Color.clear : Color.white;
            }
            
            //SyncRemotePlayerPosition();
        }
    }*/

    private int GetSpeedBasedOnDifficulty(string difficulty)
    {
        switch (difficulty)
        {
            case "1":
                return 70;
            case "2":
                return 100;
            case "3":
                return 130;
            default:
                return 60;
        }
    }
}