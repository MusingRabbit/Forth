using Assets.Scripts.Util;
using UnityEngine;

namespace Assets.Scripts
{
    public class WeaponSpawnPoint : MonoBehaviour
    {
        [SerializeField]
        private WeaponTypeSelection m_weaponType;

        public WeaponType WeaponType
        {
            get
            {
                if (m_weaponType == WeaponTypeSelection.Random)
                {
                    return (WeaponType)Random.Range((int)WeaponTypeSelection.Pistol, (int)WeaponTypeSelection.Random - 1);
                }

                return (WeaponType)m_weaponType;
            }
        }

        public WeaponSpawnPoint()
        {
            m_weaponType = WeaponTypeSelection.AssaultRifle;
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
