using Assets.Scripts.Scan;
using Assets.Scripts.Util;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Assets.Scripts.Util.PhysicsExtensions;

public struct ActorGroundRayHitInfo
{
    public bool IsHit;
    public RaycastHit Info;
    public Vector3 Position;
    public Quaternion Rotation;
}

public class ActorGroundRay : MonoBehaviour
{
    [SerializeField]
    private Scan m_scan;

    private bool m_rayHit;
    private RaycastHit m_hitInfo;
    private Quaternion m_rot;
    private Vector3 m_pos;
    private Vector3 m_norm;
    private float m_height;

    public bool Hit
    {
        get
        {
            return m_rayHit;
        }
    }

    public float Height
    {
        get
        {
            return m_height;
        }
    }

    //public RaycastHit HitInfo
    //{
    //    get
    //    {
    //        return m_hitInfo;
    //    }
    //}

    public Quaternion Rotation
    {
        get
        {
            return m_rot;
        }
    }

    public Vector3 Positon
    {
        get
        {
            return m_pos;
        }
    }

    public Vector3 Normal
    {
        get
        {
            return m_norm;
        }
    }


    public ActorGroundRay()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        m_pos = Vector3.zero;
        m_rot = Quaternion.identity;
    }

    // Update is called once per frame
    void Update()
    {
        var points = m_scan.GetPoints();

        Quaternion avgRot;
        List<Quaternion> rots = new List<Quaternion>();
        List<float> weights = new List<float>();
        Vector3 avgPos = Vector3.zero;
        Vector3 avgNorm = Vector3.zero;
        float avgHeight = 0;

        int nbPoint = 0;

        foreach (var item in points)
        {
            rots.Add(item.Rotation);
            weights.Add(item.Weight);
            avgPos += item.Position;
            avgNorm += item.Normal;
            avgHeight += item.Height;
            nbPoint++;
        }

        avgPos /= points.Count;
        avgNorm /= points.Count;
        avgHeight /= points.Count;
        avgRot = QuaternionExtensions.QuatAvgApprox(rots.ToArray(), weights.ToArray());

        m_pos = avgPos;
        m_rot = avgRot;
        m_norm = avgNorm;
        m_height = avgHeight;

        Debug.Log("Height : " + m_height);

        m_rayHit = points.Any();
    }


    public ActorGroundRayHitInfo GetActorGroundRayHitInfo()
    {
        return new ActorGroundRayHitInfo
        {
            Info = m_hitInfo,
            IsHit = m_rayHit,
            Position = m_pos,
            Rotation = m_rot
        };
    }
}
