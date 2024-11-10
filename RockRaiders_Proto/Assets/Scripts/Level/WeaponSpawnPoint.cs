using Assets.Scripts.Util;
using UnityEngine;

namespace Assets.Scripts
{
    public class WeaponSpawnPoint : MonoBehaviour
    {
        [SerializeField]
        private WeaponType m_weaponType;

        public WeaponType WeaponType
        {
            get
            {
                return m_weaponType;
            }
        }

        public WeaponSpawnPoint()
        {
            m_weaponType = WeaponType.AssaultRifle;
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
