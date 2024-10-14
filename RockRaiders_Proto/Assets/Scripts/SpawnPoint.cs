using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            var meshRenderer = this.GetComponent<MeshRenderer>();
            meshRenderer.enabled = false;
        }

        private void Update()
        {
            
        }
    }
}
