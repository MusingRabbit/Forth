using Assets.Scripts.Util;
using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    /// Weapon spawn point
    /// </summary>
    public class WeaponSpawnPoint : MonoBehaviour
    {
        /// <summary>
        /// Stores the weapon type of this spawn point
        /// </summary>
        [SerializeField]
        private WeaponTypeSelection m_weaponType;

        /// <summary>
        /// Gets the weapon type for this spawn point. Will return random type if weapontype is set to 'Random'
        /// </summary>
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

        /// <summary>
        /// Gets the spawn positon of this weapon spawn.
        /// </summary>
        public Vector3 SpawnPosition { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public WeaponSpawnPoint()
        {
            m_weaponType = WeaponTypeSelection.AssaultRifle;
        }

        /// <summary>
        /// Called on load
        /// </summary>
        private void Awake()
        {
            var debugMeshObj = this.gameObject.FindChild("DebugMesh");
            var meshRenderer = debugMeshObj.GetComponent<MeshRenderer>();
            meshRenderer.enabled = false;

            this.SpawnPosition = debugMeshObj.transform.position;
        }

        /// <summary>
        /// Called before first frame
        /// </summary>
        private void Start()
        {

        }

        /// <summary>
        /// Called every frame
        /// </summary>
        private void Update()
        {
            
        }
    }
}
