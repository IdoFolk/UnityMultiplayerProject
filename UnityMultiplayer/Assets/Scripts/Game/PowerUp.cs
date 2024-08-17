using System;
using Photon.Pun;
using UnityEngine;

public class PowerUp : MonoBehaviourPunCallbacks, IPunInstantiateMagicCallback
{
    public enum PowerUpType
    {
        SpeedUp,
        SlowDown,
        Invincibility,
        NoTrail
    }
    
    private PowerUpType type;
    [SerializeField] private MeshRenderer _meshRenderer;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && other.GetComponent<PhotonView>().IsMine && other.TryGetComponent<PlayerController>(out PlayerController playerController))
        {
            playerController.ApplyPowerUp(type);
            PhotonNetwork.Destroy(gameObject);
        }
    }


    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] instantiationData = info.photonView.InstantiationData;
        if (instantiationData[0] is int && (int)instantiationData[0] < Enum.GetValues(typeof(PowerUpType)).Length)
        {
            type = (PowerUpType)instantiationData[0];
            //_meshRenderer.material = type.GetMaterial();
            //Debug.Log($"Powerup color changed to {type}.");
        }
        else
        {
            Debug.Log("Error with PowerUp type data.");
        }
    }
}