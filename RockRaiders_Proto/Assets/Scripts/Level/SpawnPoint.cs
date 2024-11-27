using Assets.Scripts.Network;
using Assets.Scripts.Util;
using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    /// Spawn Point
    /// </summary>
    public class SpawnPoint : MonoBehaviour
    {
        /// <summary>
        /// Team this spawn point belongs to
        /// </summary>
        [SerializeField]
        private Team m_team;

        public Team Team
        {
            get
            {
                return m_team;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public SpawnPoint()
        {
            m_team = Team.None;
        }

        /// <summary>
        /// Called before first frame
        /// -> Hides debug mesh placeholder
        /// </summary>
        private void Start()
        {
            var debugMeshObj = this.gameObject.FindChild("DebugMesh");
            var meshRenderer = debugMeshObj.GetComponent<MeshRenderer>();
            meshRenderer.enabled = false;
        }

        /// <summary>
        /// Called every frame
        /// </summary>
        private void Update()
        {
            
        }
    }
}
