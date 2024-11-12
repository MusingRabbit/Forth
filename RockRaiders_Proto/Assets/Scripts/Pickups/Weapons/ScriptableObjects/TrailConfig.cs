using System;
using UnityEngine;

namespace Assets.Scripts.Pickups.Weapons.ScriptableObjects
{
    [Serializable]
    public class TrailConfig 
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

        public TrailConfig()
        {
            m_duration = 0.5f;
            m_minVertexDistance = 0.1f;
            m_missDistance = 100.0f;
            m_simulationSpeed = 200.0f;
        } 
    }
}