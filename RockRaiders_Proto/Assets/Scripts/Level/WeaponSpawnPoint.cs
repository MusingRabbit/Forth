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

        public Vector3 SpawnPosition { get; set; }

        public WeaponSpawnPoint()
        {
            m_weaponType = WeaponTypeSelection.AssaultRifle;
        }

        private void Awake()
        {
            var debugMeshObj = this.gameObject.FindChild("DebugMesh");
            var meshRenderer = debugMeshObj.GetComponent<MeshRenderer>();
            meshRenderer.enabled = false;

            this.SpawnPosition = debugMeshObj.transform.position;
        }

        private void Start()
        {

        }

        private void Update()
        {
            
        }
    }
}
