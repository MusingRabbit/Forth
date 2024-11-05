using Assets.Scripts.Util;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Actor
{
    public class ActorPainter : MonoBehaviour
    {
        [SerializeField]
        private GameObject m_body;

        private List<GameObject> m_paintabledObjs;
        private List<MeshRenderer> m_meshRenderers;

        private string[] m_exclude = new []{ "Capsule", "Visor" };

        private ActorState m_state;
        private Team m_oldTeam;
        private Color m_color;

        private bool m_painted;

        // Start is called before the first frame update
        void Start()
        {
            m_painted = false;
            m_state = this.GetComponent<ActorState>();
            m_paintabledObjs = m_body.GetAllChildren(m_exclude);
            m_paintabledObjs = m_paintabledObjs.Where(x => x.GetComponent<MeshRenderer>() != null).ToList();
            m_meshRenderers = m_paintabledObjs.Select(x => x.GetComponent<MeshRenderer>()).ToList();
            this.SetActorColour();
        }


        // Update is called once per frame
        void Update()
        {
            if (m_oldTeam != m_state.Team)
            {
                this.SetActorColour();
            }

            m_oldTeam = m_state.Team;
        }

        private void SetActorColour()
        {
            switch (m_state.Team)
            {
                case Team.None:
                    this.Paint(this.GetRandomColor());
                    break;
                case Team.Red:
                    this.Paint(Color.red);
                    break;
                case Team.Blue:
                    this.Paint(Color.blue);
                    break;
            }
        }

        public void Paint(Color color)
        {
            m_color = color;
            foreach (var mesh in m_meshRenderers)
            {
                mesh.material.color = m_color;
                //mesh.material.SetColor("_EmissionColor", color);
            }
        }

        public Color GetPaint()
        {
            return m_color;
        }

        private Color GetRandomColor()
        {
            return new Color(Random.value, Random.value, Random.value);
        }
    }
}