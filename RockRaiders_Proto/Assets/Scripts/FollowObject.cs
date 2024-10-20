using Unity.VisualScripting;
using UnityEngine;

public class FollowObject : MonoBehaviour
{
    public GameObject Target
    {
        get
        {
            return m_targetObj;
        }
        set
        {
            m_targetObj = value;
        }
    }

    public Vector3 Offset
    {
        get
        {
            return m_offset;
        }
        set
        {
            m_offset = value;
        }
    }

    public bool IsLookingAtTarget
    {
        get
        {
            return m_lookAtTarget;
        }
        set
        {
            m_lookAtTarget = value;
        }
    }


    [SerializeField]
    [SerializeAs("TargetObject")]
    private GameObject m_targetObj;

    [SerializeField]
    [SerializeAs("Offset")]
    private Vector3 m_offset;

    [SerializeField]
    [SerializeAs("LookAtTarget")]
    [Tooltip("Check if you want this object to look at the target. Checking this will ignore match rotation.")]
    private bool m_lookAtTarget;

    [SerializeField]
    private bool m_matchRotation;

    [SerializeField]
    private float m_roationSlerp;

    public FollowObject()
    {
        m_offset = Vector3.zero;
        m_lookAtTarget = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (m_targetObj == null)
        {
            return; 
        }

        this.gameObject.transform.position = m_targetObj.gameObject.transform.position + m_offset;

        if (m_lookAtTarget)
        {
            this.gameObject.transform.LookAt(m_targetObj.gameObject.transform.position);
        }
        else if (m_matchRotation)
        {
            var tgtRotaion = m_targetObj.gameObject.transform.rotation;
            this.gameObject.transform.rotation = Quaternion.Slerp(this.gameObject.transform.rotation, tgtRotaion, m_roationSlerp * Time.deltaTime);
        }
    }
}
