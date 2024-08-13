using System;
using Photon.Pun;
using UnityEngine;

public class PlayerTrail : MonoBehaviourPunCallbacks, IPunInstantiateMagicCallback
{
    [SerializeField] private MeshRenderer _meshRenderer;

    // Update is called once per frame
    void Update()
    {
        
    }


    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] instantiationData = info.photonView.InstantiationData;
        if (ColorUtility.TryParseHtmlString("#"+(string)instantiationData[0], out Color color))
        {
            _meshRenderer.material.color = color;
            Debug.Log($"Trail color changed to {color}.");
        }
        else
        {
            Debug.Log("Error with trail color data.");
        }
    }
}
