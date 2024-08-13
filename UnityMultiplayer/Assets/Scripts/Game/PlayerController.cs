using System;
using System.Collections;
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
    

    private float collisionTimer = 0;
    
    private bool movementEnabled = true;
    
    private Camera cachedCamera;
    private Rigidbody _rigidbody;
    private Vector3 raycastPos;
    private Vector3 movementVector = new Vector3();
    
    private void Start()
    {
        if (photonView.IsMine)
        {
            _rigidbody = GetComponent<Rigidbody>();
            cachedCamera = Camera.main;
            StartCoroutine(LeaveTrail());
        }
    }

    private void FixedUpdate()
    {
        collisionTimer += Time.fixedDeltaTime;
        //constantly moves the player forward
        if (photonView.IsMine && movementEnabled)
        {
            _rigidbody.linearVelocity = transform.forward * (speed); 
            //moves the transform in the direction of the forward vector in local world space
            //transform.Translate(transform.forward * (speed * Time.fixedDeltaTime), Space.World);
            
            //rotates the player left or right when he presses the arrow keys
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                transform.Rotate(Vector3.up, -rotationSpeed * Time.fixedDeltaTime, Space.World);
                Debug.Log("left");
            }
        
            if (Input.GetKey(KeyCode.RightArrow))
            {
                transform.Rotate(Vector3.up, rotationSpeed * Time.fixedDeltaTime, Space.World);
                Debug.Log("right");
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
            Debug.Log("Object Instantiated");
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
        if (collisionTimer < 3)
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
            PhotonNetwork.Destroy(gameObject);
            Debug.Log($"Player {photonView.Owner.ActorNumber} died");
        }
    }
    
    [PunRPC] private void GameOver()
    {
        movementEnabled = false;
    }

    IEnumerator DestroyDelay(float delay, GameObject otherObject)
    {
        yield return new WaitForSeconds(delay);
        PhotonNetwork.Destroy(otherObject);
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
            Debug.Log($"Player color changed to {color}.");
        }
        else
        {
            Debug.Log("Error with player color data.");
        }
    }
}