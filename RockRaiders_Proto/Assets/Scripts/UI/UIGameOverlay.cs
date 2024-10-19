using Assets.Scripts.Actor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGameOverlay : MonoBehaviour
{
    [SerializeField]
    private ActorController m_actor;

    [SerializeField]
    private Text m_txtGravBootsEnabled;

    private ActorState m_actorState;

    public ActorController Actor
    {
        get
        {
            return m_actor;
        }
        set
        {
            m_actor = value;
            m_actorState = m_actor.GetComponent<ActorState>();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.UpdateGravBootsStatus();
    }

    private void UpdateGravBootsStatus()
    {
        string txtVal = "Unknown";
        txtVal = m_actorState?.GravBootsEnabled ?? false ? "True" : "False";
        var txtTint = m_actorState?.GravBootsEnabled ?? false ? Color.green : Color.red;
        m_txtGravBootsEnabled.text = txtVal;
        m_txtGravBootsEnabled.color = txtTint;
    }
}
