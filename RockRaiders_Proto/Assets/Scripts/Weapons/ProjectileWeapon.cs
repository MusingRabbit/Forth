using Assets.Scripts.Util;
using Assets.Scripts.Weapons;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

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
    private ProjectileNetwork m_pwNet;
    

    // Start is called before the first frame update
    public override void Start()
    {
        m_muzzle = this.gameObject.FindChild("Projectile_Exit");
        m_pwNet = this.gameObject.GetComponent<ProjectileNetwork>();
        base.Start();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
    }

    public override void Fire()
    {
        if (this.OwnerRigidBody != null)
        {
            var deltaTime = Time.time - m_lastShotTime;
            var canFire = deltaTime > (1.0f / m_fireRate);

            var spread = Random.Range(-m_spread, m_spread);
            var velOffset = this.OwnerRigidBody.velocity + new Vector3(0.0f, spread, 0.0f);

            if (canFire)
            {
                m_lastShotTime = Time.time;
                m_pwNet.SpawnProjectile(
                    m_muzzle.transform.position,
                    m_muzzle.transform.rotation,
                    velOffset,
                    m_muzzleVelocity);
            }
        }
    }
}
