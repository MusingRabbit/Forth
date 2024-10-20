using UnityEngine;

namespace Assets.Scripts.Actor
{
    public class ActorCrosshair : RRMonoBehaviour
    {
        [SerializeField]
        private LayerMask m_layerMask;

        [SerializeField]
        private ActorCamera m_actorCamera;

        [SerializeField]
        private GameObject m_debugObj;

        [SerializeField]
        private float m_maxDistance;

        private Vector3 m_point;

        private ActorState m_state;

        public bool UpdateAimpointFromCamera { get; set; }

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

        public Vector3 AimPoint
        {
            get
            {
                return m_point;
            }
            set
            {
                m_point = value;
            }
        }

        public ActorCrosshair()
        {
            m_maxDistance = 999.9f;
        }

        public override void Initialise()
        {
            this.UpdateAimpointFromCamera = true;
            m_state = this.GetComponent<ActorState>();
        }

        private void Start()
        {
            this.Initialise();
        }

        private void UpdateDebugObjTransform()
        {
            m_debugObj.transform.position = m_point;

            var dir = m_point - m_actorCamera.transform.position;
            m_debugObj.gameObject.transform.forward = -m_actorCamera.transform.forward;
        }

        private void Update()
        {
            if (m_actorCamera == null || m_state.IsDead || this.UpdateAimpointFromCamera == false)
            {
                return;
            }

            var dir = m_actorCamera.transform.forward;
            var hit = Physics.Raycast(m_actorCamera.transform.position, dir, out var hitInfo, m_maxDistance, m_layerMask);

            if (hit)
            {
                m_point = hitInfo.point;
            }

            if (m_debugObj != null)
            {
                this.UpdateDebugObjTransform();
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawSphere(m_point, 0.5f);
        }
    }
}
