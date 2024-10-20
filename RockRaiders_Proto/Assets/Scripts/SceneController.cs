using UnityEngine;

namespace Assets.Scripts
{
    public class SceneController : MonoBehaviour
    {
        [SerializeField]
        Camera m_mainCamera;

        PlayerState m_playerState;

        public SceneController()
        {
            m_playerState = PlayerState.Spectating;
        }

        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}