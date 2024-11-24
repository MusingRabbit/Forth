using Assets.Scripts;
using Assets.Scripts.Pickups;
using Assets.Scripts.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flag : PickupItem
{
    [SerializeField]
    private Team m_team;

    [SerializeField]
    private Light m_light;

    private GameObject m_flagL;
    private GameObject m_flagR;
    private MeshRenderer m_fRendL;
    private MeshRenderer m_fRendR;
    private bool m_captured;
    private bool m_returned;

    public Team Team
    {
        get
        {
            return m_team;
        }
        set
        {
            m_team = value;
        }
    }

    public bool Captured
    {
        get
        {
            return m_captured;
        }
        set
        {
            m_captured = value;
        }
    }

    public bool Retreived
    {
        get
        {
            return m_returned;
        }
        set
        {
            m_returned = value;
        }
    }

    public Flag()
    {
        m_packType = PackType.Flag;
    }

    public override void Initialise()
    {
        m_flagL = base.gameObject.FindChild("FlagL");
        m_flagR = base.gameObject.FindChild("FlagR");

        m_fRendL = m_flagL.GetComponent<MeshRenderer>();
        m_fRendR = m_flagR.GetComponent<MeshRenderer>();

        var colour = m_team == Team.Blue ? Color.blue : m_team == Team.Red ? Color.red : Color.white;
        this.Paint(colour);

        m_captured = false;

        base.Initialise();
    }

    protected override void Start()
    {
        this.Initialise();
        base.Start();
    }

    protected override void Update()
    {
        var colour = m_team == Team.Blue ? Color.blue : m_team == Team.Red ? Color.red : Color.white;
        this.Paint(colour);

        base.Update();
    }

    private void Paint(Color color)
    {
        m_fRendL.material.color = color;
        m_fRendR.material.color = color;
        m_light.color = color;
    }
}
