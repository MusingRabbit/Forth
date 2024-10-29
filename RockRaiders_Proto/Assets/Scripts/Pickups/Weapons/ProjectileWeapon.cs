using Assets.Scripts.Util;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.Scripts.Pickups.Weapons
{
    public class ProjectileWeapon : Weapon
    {
        [SerializeField]
        [SerializeAs("MagazineSize")]
        private int m_magazineSize;

        [SerializeField]
        [SerializeAs("TotalCapacity")]
        private int m_totalCapacity;

        [SerializeField]
        [SerializeAs("MuzzleVecocity")]
        private float m_muzzleVelocity;

        [SerializeField]
        [SerializeAs("FireRate")]
        private float m_fireRate;

        [SerializeField]
        private float m_spread = 0.1f;

        private float m_lastShotTime;
        private GameObject m_muzzle;
        private ProjectileSpawnManager m_projectileSpawner;


        // Start is called before the first frame update
        protected override void Start()
        {
            m_muzzle = gameObject.FindChild("Projectile_Exit");
            m_projectileSpawner = gameObject.GetComponent<ProjectileSpawnManager>();
            base.Start();
        }

        // Update is called once per frame
        protected override void Update()
        {
            base.Update();
        }

        public override void Fire()
        {
            if (OwnerRigidBody != null)
            {
                var deltaTime = Time.time - m_lastShotTime;
                var canFire = this.CanFire && deltaTime > 1.0f / m_fireRate;

                var spreadX = Random.Range(-m_spread, m_spread);
                var spreadY = Random.Range(-m_spread, m_spread);
                var spread = new Vector3(spreadX, spreadY, 0.0f);
                var velOffset = OwnerRigidBody.velocity + spread;

                var deltaV = Crosshair.AimPoint - m_muzzle.transform.position;
                var rotation = Quaternion.LookRotation(deltaV, transform.up);

                if (canFire)
                {
                    m_lastShotTime = Time.time;
                    if (m_projectileSpawner.SpawnProjectile(this.gameObject,
                        m_muzzle.transform.position,
                        rotation,
                        velOffset,
                        m_muzzleVelocity))
                    {
                        Invoke_OnShotFired((velOffset + m_muzzle.transform.forward).normalized * m_muzzleVelocity, m_projectileSpawner.Projectile.Mass);
                    }
                }
            }
        }
    }
}