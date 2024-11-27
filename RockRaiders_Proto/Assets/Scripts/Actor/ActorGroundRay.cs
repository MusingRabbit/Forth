using Assets.Scripts;
using Assets.Scripts.Scan;
using Assets.Scripts.Util;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct ActorGroundRayHitInfo
{
    public bool IsHit;
    public Vector3 Position;
    public Quaternion Rotation;
}

/// <summary>
/// Actor ground ray 
/// Used for checking actors proximity to ground, as well as its orientation in relation to the player
/// </summary>
public class ActorGroundRay : RRMonoBehaviour
{
    /// <summary>
    /// Ground Rays' scanning behaviour
    /// </summary>
    [SerializeField]
    private Scan m_scan;

    /// <summary>
    /// Stores whether the ray is touching ground or not.
    /// </summary>
    private bool m_rayHit;

    /// <summary>
    /// Stores the average rotation of all raycasts.
    /// </summary>
    private Quaternion m_rot;

    /// <summary>
    /// Stores average position of all raycasts
    /// </summary>
    private Vector3 m_pos;

    /// <summary>
    /// Stores the average normal of all raycasts.
    /// </summary>
    private Vector3 m_norm;
    //private float m_height;

    /// <summary>
    /// Gets wheather ray is touching a surface.
    /// </summary>
    public bool Hit
    {
        get
        {
            return m_rayHit;
        }
    }

    /// <summary>
    /// Gets the average rotation of the ground ray
    /// </summary>
    public Quaternion Rotation
    {
        get
        {
            return m_rot;
        }
    }

    /// <summary>
    /// Gets the average position of all raycasts.
    /// </summary>
    public Vector3 Positon
    {
        get
        {
            return m_pos;
        }
    }

    /// <summary>
    /// Gets the average normals of all raycasts.
    /// </summary>
    public Vector3 Normal
    {
        get
        {
            return m_norm;
        }
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public ActorGroundRay()
    {

    }

    /// <summary>
    /// initialisation
    /// </summary>
    public override void Initialise()
    {
        m_pos = Vector3.zero;
        m_rot = Quaternion.identity;
    }

    // Start is called before the first frame update
    void Start()
    {
        this.Initialise();
    }

    /// <summary>
    /// Resets the ground ray
    /// </summary>
    public override void Reset()
    {
        m_pos = Vector3.zero;
        m_rot = Quaternion.identity;
        m_rayHit = false;
        m_norm = Vector3.up;
    }

    // Update is called once per frame
    void Update()
    {
        var points = m_scan.GetPoints();                    // Gets all points from the scan that make contact with a surface

        Quaternion avgRot;                                  
        List<Quaternion> rots = new List<Quaternion>();
        List<float> weights = new List<float>();
        Vector3 avgPos = Vector3.zero;
        Vector3 avgNorm = Vector3.zero;
        float avgHeight = 0;

        int nbPoint = 0;

        foreach (var item in points)                      // Aggregate the roation, weights, positions, normal, and height of all points
        {
            rots.Add(item.Rotation);
            weights.Add(item.Weight);
            avgPos += item.Position;
            avgNorm += item.Normal;
            avgHeight += item.Height;
            nbPoint++;
        }

        avgPos /= points.Count;                         // Calculage the average rotation, normal, height and position
        avgNorm /= points.Count;
        avgHeight /= points.Count;
        avgRot = QuaternionExtensions.QuatAvgApprox(rots.ToArray(), weights.ToArray());     // Calculate the weighted average rotation of all rotations.

        m_pos = avgPos;
        m_rot = avgRot;
        m_norm = avgNorm;
        //m_height = avgHeight;

        //Debug.Log("Height : " + m_height);

        m_rayHit = points.Any();
    }

    /// <summary>
    /// Returns ray hit information
    /// </summary>
    /// <returns><see cref="ActorGroundRayHitInfo"/>Ray hit information</returns>
    public ActorGroundRayHitInfo GetActorGroundRayHitInfo()
    {
        return new ActorGroundRayHitInfo
        {
            IsHit = m_rayHit,
            Position = m_pos,
            Rotation = m_rot
        };
    }
}
