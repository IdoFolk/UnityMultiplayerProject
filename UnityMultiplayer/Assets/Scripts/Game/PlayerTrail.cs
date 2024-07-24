using System;
using Photon.Pun;
using UnityEngine;

public class PlayerTrail : MonoBehaviourPunCallbacks, IPunInstantiateMagicCallback
{
    private MeshRenderer _meshRenderer;

    private void OnValidate()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] instantiationData = info.photonView.InstantiationData;
        if (ColorUtility.TryParseHtmlString((string)instantiationData[0], out Color color))
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
