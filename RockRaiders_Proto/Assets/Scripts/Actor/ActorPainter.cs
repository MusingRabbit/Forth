using Assets.Scripts.Util;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Actor
{
    /// <summary>
    /// Actor behavioural component responsible for painting the actor.
    /// </summary>
    public class ActorPainter : MonoBehaviour
    {
        /// <summary>
        /// Actor body game object
        /// </summary>
        [SerializeField]
        private GameObject m_body;

        /// <summary>
        /// all paintable gameobjects
        /// </summary>
        private List<GameObject> m_paintabledObjs;

        /// <summary>
        /// Stores all mesh renderers
        /// </summary>
        private List<MeshRenderer> m_meshRenderers;

        /// <summary>
        /// Gameobject names to exclude from "GetAllChildren" query
        /// </summary>
        private string[] m_childrenToExcl = new []{ "Capsule", "Visor" };

        /// <summary>
        /// Store the actors state
        /// </summary>
        private ActorState m_state;

        /// <summary>
        /// Stores the old team (team set from last update to check for changes in player team)
        /// </summary>
        private Team m_oldTeam;

        /// <summary>
        /// Currently painted colour
        /// </summary>
        private Color m_colour;

        // Start is called before the first frame update
        void Start()
        {
            m_state = this.GetComponent<ActorState>();
            m_paintabledObjs = m_body.GetAllChildren(m_childrenToExcl);
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

        /// <summary>
        /// Sets the actor colour depending on the actors' current team.
        /// </summary>
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

        /// <summary>
        /// Paints the actor with the colour specified.
        /// </summary>
        /// <param name="color">Colour to paint the actor</param>
        public void Paint(Color color)
        {
            m_colour = color;
            foreach (var mesh in m_meshRenderers)
            {
                mesh.material.color = m_colour;
            }
        }

        /// <summary>
        /// Gets the current colour of the actor
        /// </summary>
        /// <returns>Colour <see cref="Color"/></returns>
        public Color GetColour()
        {
            return m_colour;
        }

        /// <summary>
        /// Gets a random colour
        /// </summary>
        /// <returns>Random Colour <see cref="Color"/></returns>
        private Color GetRandomColor()
        {
            return new Color(Random.value, Random.value, Random.value);
        }
    }
}