using UnityEngine;

namespace Assets.Scripts.Actor
{
    public class ActorCrosshair : RRMonoBehaviour
    {
        /// <summary>
        /// Layer mask - layers that the crosshair will interesct with
        /// </summary>
        [SerializeField]
        private LayerMask m_layerMask;

        /// <summary>
        /// The camera currently assigned to this actor entity.
        /// </summary>
        [SerializeField]
        private ActorCamera m_actorCamera;

        /// <summary>
        /// Object for rendering crosshair position.
        /// </summary>
        [SerializeField]
        private GameObject m_debugObj;

        /// <summary>
        /// The farthest distance from the player the crosshair will go.
        /// </summary>
        [SerializeField]
        private float m_maxDistance;

        /// <summary>
        /// Current position of the crosshair
        /// </summary>
        private Vector3 m_position;

        /// <summary>
        /// The actors' state component.
        /// </summary>
        private ActorState m_state;

        /// <summary>
        /// Gets or sets whether to update aim point from camera position.
        /// </summary>
        public bool UpdateAimpointFromCamera { get; set; }

        /// <summary>
        /// Gets or sets the current actors' camera
        /// </summary>
        public ActorCamera ActorCamera
        {
            get
            {
                return m_actorCamera;
            }
            set
            {
                m_actorCamera = value;
            }
        }

        /// <summary>
        /// Gets or sets the crosshair's aimpoint/position
        /// </summary>
        public Vector3 AimPoint
        {
            get
            {
                return m_position;
            }
            set
            {
                m_position = value;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ActorCrosshair()
        {
            m_maxDistance = 99999.9f;
        }

        /// <summary>
        /// Initialisation method
        /// </summary>
        public override void Initialise()
        {
            this.UpdateAimpointFromCamera = true;
            m_state = this.GetComponent<ActorState>();
        }

        /// <summary>
        /// Called prior to the first frame in scene.
        /// </summary>
        private void Start()
        {
            this.Initialise();
        }

        /// <summary>
        /// Called every frame.
        /// </summary>
        private void Update()
        {
            if (m_actorCamera == null || m_state.IsDead || this.UpdateAimpointFromCamera == false)
            {
                return;
            }

            var dir = m_actorCamera.transform.forward;
            var dPos = this.transform.position - m_actorCamera.transform.position;
            var startPos = m_actorCamera.transform.position + (m_actorCamera.transform.forward * (dPos.magnitude * 2));
            var hit = Physics.Raycast(startPos, dir, out var hitInfo, m_maxDistance, m_layerMask);

            if (hit)
            {
                m_position = hitInfo.point;
            }

            if (m_debugObj != null)
            {
                this.UpdateDebugObjTransform();
            }
        }

        /// <summary>
        /// Called whenever the object is to be 'reset'
        /// </summary>
        public override void Reset()
        {
            m_position = Vector3.zero;
            this.UpdateAimpointFromCamera = true;
            m_maxDistance = 99999.9f;
        }

        /// <summary>
        /// Updates the position of the debug game object
        /// </summary>
        private void UpdateDebugObjTransform()
        {
            m_debugObj.transform.position = m_position;

            var dir = m_position - m_actorCamera.transform.position;
            m_debugObj.gameObject.transform.forward = -m_actorCamera.transform.forward;
        }

        /// <summary>
        /// Called when debug gizmos set to be rendered.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawSphere(m_position, 0.5f);
        }


    }
}
