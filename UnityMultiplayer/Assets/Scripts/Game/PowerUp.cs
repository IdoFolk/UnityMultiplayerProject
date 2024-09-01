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

    public static float PowerUpDuration = 7f;
    
    public PowerUpType type { get; private set; }
    public int viewID { get; private set; }
    [SerializeField] SpriteRenderer _spriteRenderer;
    [SerializeField] private List<PowerUpTypeIcon> typeIcons;

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] instantiationData = info.photonView.InstantiationData;
        if (instantiationData[0] is int && (int)instantiationData[0] < Enum.GetValues(typeof(PowerUpType)).Length)
        {
            type = (PowerUpType)instantiationData[0];
            _spriteRenderer.sprite = typeIcons[(int)type].icon;
        }
        else
        {
            Debug.Log("Error with PowerUp type data.");
        }

        viewID = photonView.ViewID;
    }
}