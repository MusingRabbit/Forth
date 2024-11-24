using UnityEngine;

namespace Assets.Scripts
{
    public class MovingPlatform : MonoBehaviour
    {
        private Transform m_transform;

        [SerializeField]
        private Vector3 m_startPosition;

        [SerializeField]
        private Vector3 m_endPosition;

        [SerializeField]
        private float m_interpolationTime;

        private float m_startTime;
        private float m_currTime;
        private bool m_reverse;

        // Start is called before the first frame update
        void Start()
        {
            m_reverse = false;
            m_transform = gameObject.transform;
            this.ResetStartTime();
        }

        // Update is called once per frame
        void Update()
        {
            m_currTime = Time.time;
            var deltaTime = m_currTime - m_startTime;
            var val = Mathf.Sin(Mathf.PI * (deltaTime / m_interpolationTime));

            if (m_reverse)
            {
                val = 1.0f - val;
            }

            m_transform.position = Vector3.Lerp(m_startPosition, m_endPosition, val);

            if (deltaTime >= m_interpolationTime)
            {
                //m_reverse = !m_reverse;
                this.ResetStartTime();
            }
        }

        private void ResetStartTime()
        {
            m_startTime = Time.time;
        }
    }
}