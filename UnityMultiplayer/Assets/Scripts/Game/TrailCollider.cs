using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;
 
[RequireComponent(typeof(LineRenderer))]
public class TrailCollider : MonoBehaviourPunCallbacks, IPunInstantiateMagicCallback, IPunObservable
{
    [SerializeField] private float pointSpacing = 0.05f;
    [SerializeField] LineRenderer myTrail;
    [SerializeField] EdgeCollider2D myCollider;
    private Transform player;
    private List<Vector2> points;

    private Queue<Vector2> unSyncedPointQueue;

    private bool inited = false;

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        if (inited) return;
        inited = true;
        points = new List<Vector2>();
        unSyncedPointQueue = new Queue<Vector2>();
        if (photonView.IsMine)
        {
            player = GameNetworkManager.Instance.currentPlayer.transform;
            SetPoint();
        }
    }

    public void UpdateTrail()
    {
        if (Vector3.Distance(points.Last(), player.position) > pointSpacing)
        {
            SetPoint();
        }
    }
 
    private void SetPoint()
    {
        UpdateCollider();
        
        points.Add(player.position);
        myTrail.positionCount = points.Count;
        myTrail.SetPosition(points.Count - 1, player.position);
        
        unSyncedPointQueue.Enqueue(player.position);
    }

    private void UpdateCollider()
    {
        if (points.Count > 1)
        {
            myCollider.SetPoints(points.SkipLast(1).ToList());
        }
    }


    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] instantiationData = info.photonView.InstantiationData;
        if (ColorUtility.TryParseHtmlString("#"+(string)instantiationData[0], out Color color))
        {
            myTrail.startColor = color;
            myTrail.endColor = color;
            //Debug.Log($"Trail color changed to {color}.");
        }
        else
        {
            Debug.LogWarning("Error with trail color data.");
        }

        Init();
        transform.parent = GameNetworkManager.Instance.TrailObjectsParent;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (trailAbandoned) return;
        if (stream.IsWriting)
        {
            //We own this trail: send the others our data
            if (unSyncedPointQueue.Count > 0)
            {
                Vector2 point = unSyncedPointQueue.Dequeue();
                stream.SendNext(point);
            }
        }
        else if (stream.IsReading)
        {
            Vector2 newPoint = (Vector2)stream.ReceiveNext();
                
            UpdateCollider();
                
            points.Add(newPoint);
            myTrail.positionCount = points.Count;
            myTrail.SetPosition(points.Count - 1, newPoint);
        }
    }

    private bool trailAbandoned;
    public void StopUpdatingTrail()
    {
        trailAbandoned = true;
    }
}