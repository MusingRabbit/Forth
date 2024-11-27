using Assets.Scripts;
using Assets.Scripts.Pickups;
using Assets.Scripts.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flag : PickupItem
{

    /// <summary>
    /// Stores the flags team
    /// </summary>
    [SerializeField]
    private Team m_team;

    /// <summary>
    /// Stores the flags light component
    /// </summary>
    [SerializeField]
    private Light m_light;

    /// <summary>
    /// Stores the left quad of the flag
    /// </summary>
    private GameObject m_flagL;

    /// <summary>
    /// Stores the right quad of the flag
    /// </summary>
    private GameObject m_flagR;

    /// <summary>
    /// Stores the mesh renderer for the left quad
    /// </summary>
    private MeshRenderer m_fRendL;

    /// <summary>
    /// Stores the mesh renderer for the right quad
    /// </summary>
    private MeshRenderer m_fRendR;

    /// <summary>
    /// Stores whether the flag has been captured
    /// </summary>
    private bool m_captured;

    /// <summary>
    /// Stores whether the flag has been returned
    /// </summary>
    private bool m_returned;

    /// <summary>
    /// Get or set the flags' team
    /// </summary>
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

    /// <summary>
    /// Gets or sets whether flag has been captured
    /// </summary>
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

    /// <summary>
    /// Gets or sets whether flag has been retreived
    /// </summary>
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

    /// <summary>
    /// Constructor
    /// </summary>
    public Flag()
    {
        m_packType = PackType.Flag;
    }

    /// <summary>
    /// Initialisation
    /// </summary>
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

    /// <summary>
    /// Called before first frame in scene
    /// </summary>
    protected override void Start()
    {
        this.Initialise();
        base.Start();
    }

    /// <summary>
    /// Called every frame
    /// -> Depending on which team the flag belongs, paint the flag the respective colour.
    /// </summary>
    protected override void Update()
    {
        var colour = m_team == Team.Blue ? Color.blue : m_team == Team.Red ? Color.red : Color.white;
        this.Paint(colour);

        base.Update();
    }

    /// <summary>
    /// Paints the flag a specified colour.
    /// </summary>
    /// <param name="color">Colour to paint the flag</param>
    private void Paint(Color color)
    {
        m_fRendL.material.color = color;
        m_fRendR.material.color = color;
        m_light.color = color;
    }
}
