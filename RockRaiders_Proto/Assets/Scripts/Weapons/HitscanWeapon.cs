using Assets.Scripts.ScriptableObjects;
using Assets.Scripts.Util;
using Assets.Scripts.Weapons;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

public class HitscanWeapon : Weapon
{
    [SerializeField]
    [SerializeAs("MagazineSize")]
    private int m_magazineSize;

    [SerializeField]
    [SerializeAs("TotalCapacity")]
    private int m_totalCapacity;

    [SerializeField]
    [SerializeAs("FireRate")]
    private float m_fireRate;

    [SerializeField]
    [SerializeAs("TrailConfig")]
    private TrailConfigScriptableObject m_trailConfig;

    [SerializeField]
    [SerializeAs("TrailConfig")]
    private ShootConfigurationScriptableObject m_shootConfig;

    private float m_lastShotTime;
    private GameObject m_muzzle;
    private ObjectPool<TrailRenderer> m_trailPool;
    private ParticleSystem m_particleSystem;

    // Start is called before the first frame update
    public override void Start()
    {
        m_muzzle = this.gameObject.FindChild("Projectile_Exit");
        m_trailPool = new ObjectPool<TrailRenderer>(this.CreateTrail);
        m_particleSystem = this.GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
    }

    public override void Fire()
    {
        var deltaTime = Time.time - m_lastShotTime;
        var canFire = deltaTime > (1.0f / m_fireRate);

        if (canFire)
        {
            m_lastShotTime = Time.time;
            m_particleSystem.Play();

            var shootDir = m_muzzle.transform.forward +
                new Vector3(
                    Random.Range(-m_shootConfig.Spread.x, m_shootConfig.Spread.x),
                    Random.Range(-m_shootConfig.Spread.y, m_shootConfig.Spread.y),
                    Random.Range(-m_shootConfig.Spread.z, m_shootConfig.Spread.z));

            var shootPos = m_muzzle.transform.position;
            var rayCastHit = Physics.Raycast(shootPos, shootDir, out RaycastHit hit, m_shootConfig.Range, m_shootConfig.HitMask);

            if (rayCastHit)
            {
                this.StartCoroutine(this.PlayTrail(shootPos, hit.point, hit));
            }
            else
            {
                this.StartCoroutine(this.PlayTrail(shootPos, shootPos + (shootDir * m_shootConfig.Range), new RaycastHit()));
            }
        }
    }

    private IEnumerator PlayTrail(Vector3 startPoint, Vector3 endPoint, RaycastHit hit)
    {
        TrailRenderer instance = m_trailPool.Get();
        instance.gameObject.SetActive(true);
        instance.transform.position = startPoint;
        yield return null;
        instance.emitting = true;
        float distance = Vector3.Distance(startPoint, endPoint);
        float remainingDistance = distance;

        while (remainingDistance > 0)
        {
            instance.transform.position = Vector3.Lerp(startPoint, endPoint, Mathf.Clamp01(1 - (remainingDistance / distance)));
            remainingDistance -= m_trailConfig.SimulationSpeed * Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(m_trailConfig.Duration);
        yield return null;
        instance.emitting = false;
        instance.gameObject.SetActive(false);
        m_trailPool.Release(instance);
    }

    private TrailRenderer CreateTrail()
    {
        GameObject instance = new GameObject("Trail");
        TrailRenderer trail = instance.AddComponent<TrailRenderer>();
        trail.colorGradient = m_trailConfig.Colour;
        trail.material = m_trailConfig.Material;
        trail.widthCurve = m_trailConfig.WidthCurve;
        trail.time = m_trailConfig.Duration;
        trail.minVertexDistance = m_trailConfig.MinVertexDistance;

        trail.emitting = false;
        trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        return trail;
    }
}
