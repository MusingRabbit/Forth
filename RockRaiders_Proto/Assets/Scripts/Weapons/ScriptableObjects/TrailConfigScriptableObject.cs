using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Trail Config", menuName = "Guns/Gun Trail Config", order = 4)]
public class TrailConfigScriptableObject : ScriptableObject
{
    [SerializeField]
    private Material m_material;

    [SerializeField]
    private AnimationCurve m_widthCurve;

    [SerializeField]
    private float m_duration = 0.5f;

    [SerializeField]
    private float m_minVertexDistance = 0.1f;

    [SerializeField]
    private Gradient m_colour;

    [SerializeField]
    private float m_missDistance = 100.0f;

    [SerializeField]
    private float m_simulationSpeed = 100.0f;

    public Gradient Colour
    {
        get
        {
            return m_colour;
        }
    }

    public Material Material
    {
        get
        {
            return m_material;
        }
    }

    public AnimationCurve WidthCurve
    {
        get
        {
            return m_widthCurve;
        }
    }

    public float Duration
    {
        get
        {
            return m_duration;
        }
    }

    public float MinVertexDistance
    {
        get
        {
            return m_minVertexDistance;
        }
    }

    public float MissDistance
    {
        get
        {
            return m_missDistance;
        }
    }

    public float SimulationSpeed
    {
        get
        {
            return m_simulationSpeed;
        }
    }
}
