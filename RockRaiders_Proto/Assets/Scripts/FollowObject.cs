using System.Collections;
using System.Collections.Generic;
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
    private bool m_lookAtTarget;

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
    }
}
