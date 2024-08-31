using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PowerUp : MonoBehaviourPunCallbacks, IPunInstantiateMagicCallback
{
    [Serializable]
    public enum PowerUpType
    {
        SpeedUp,
        SlowDown,
        Invincibility,
        NoTrail,
        BorderTeleport
    }

    [Serializable]
    public struct PowerUpTypeIcon
    {
        public PowerUpType type;
        public Sprite icon;
    }

    public static float PowerUpDuration = 5f;
    
    private PowerUpType type;
    [SerializeField] SpriteRenderer _spriteRenderer;
    [SerializeField] private List<PowerUpTypeIcon> typeIcons;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && other.GetComponent<PhotonView>().IsMine && other.TryGetComponent(out PlayerController playerController))
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
            //_spriteRenderer.sprite = 
            //Debug.Log($"Powerup color changed to {type}.");
        }
        else
        {
            Debug.Log("Error with PowerUp type data.");
        }
    }
}