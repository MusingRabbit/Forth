using Assets.Scripts.Network;
using Assets.Scripts.Util;
using UnityEngine;

namespace Assets.Scripts
{
    public class SpawnPoint : MonoBehaviour
    {
        [SerializeField]
        private Team m_team;

        public Team Team
        {
            get
            {
                return m_team;
            }
        }

        public SpawnPoint()
        {
            m_team = Team.None;
        }

        private void Start()
        {
            var debugMeshObj = this.gameObject.FindChild("DebugMesh");
            var meshRenderer = debugMeshObj.GetComponent<MeshRenderer>();
            meshRenderer.enabled = false;
        }

        private void Update()
        {
            
        }
    }
}
