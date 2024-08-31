using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;
 
[RequireComponent(typeof(LineRenderer))]
public class TrailCollider : MonoBehaviourPunCallbacks, IPunInstantiateMagicCallback, IPunObservable
{
    [SerializeField] private float pointSpacing = 0.05f;
    public Transform player { get; set; }
    public Transform parent { get; set; }
    private LineRenderer myTrail;
    private EdgeCollider2D myCollider;
    private List<Vector2> points;

 
    void Start()
    {
        myTrail = GetComponent<LineRenderer>();
        myCollider = GetComponent<EdgeCollider2D>();
        points = new List<Vector2>();
        if (photonView.IsMine)
        {
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
        if (points.Count > 1)
        {
            myCollider.SetPoints(points.SkipLast(1).ToList());
        }
        
        points.Add(player.position);
        myTrail.positionCount = points.Count;
        myTrail.SetPosition(points.Count - 1, player.position);
    }


    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] instantiationData = info.photonView.InstantiationData;
        if (ColorUtility.TryParseHtmlString("#"+(string)instantiationData[0], out Color color))
        {
            myTrail.startColor = myTrail.endColor = color;
            //Debug.Log($"Trail color changed to {color}.");
        }
        else
        {
            //Debug.Log("Error with trail color data.");
        }

        transform.parent = GameNetworkManager.Instance.TrailObjectsParent;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (trailAbandoned) return;
        if (stream.IsWriting)
        {
            //We own this trail: send the others our data
            stream.SendNext(points);
        }
        else
        {
            points = (List<Vector2>)stream.ReceiveNext();
            if (points.Count > 1)
            {
                myCollider.SetPoints(points.SkipLast(1).ToList());
            }

            int prevPosCount = myTrail.positionCount;
            myTrail.positionCount = points.Count;
            for (int i = prevPosCount; i < myTrail.positionCount; i++)
            {
                myTrail.SetPosition(i, points[i]);
            }
        }
    }

    private bool trailAbandoned = false;
    public void StopUpdatingTrail()
    {
        trailAbandoned = true;
    }
}