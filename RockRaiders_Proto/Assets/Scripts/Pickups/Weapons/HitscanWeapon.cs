using Assets.Scripts.Actor;
using Assets.Scripts.HealthSystem;
using Assets.Scripts.Pickups.Weapons.ScriptableObjects;
using Assets.Scripts.Services;
using Assets.Scripts.Util;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

namespace Assets.Scripts.Pickups.Weapons
{
    public class HitscanWeapon : Weapon
    {
        //[SerializeField]
        //[SerializeAs("MagazineSize")]
        //private int m_magazineSize;

        [SerializeField]
        [SerializeAs("TotalCapacity")]
        private int m_totalCapacity;

        [SerializeField]
        [SerializeAs("TrailConfig")]
        private ShootConfig m_shootConfig;

        [SerializeField]
        [SerializeAs("TrailConfig")]
        private TrailConfig m_trailConfig;

        private float m_lastShotTime;
        private GameObject m_muzzle;
        private ObjectPool<TrailRenderer> m_trailPool;
        private ParticleSystem m_particleSystem;

        // Start is called before the first frame update
        protected override void Start()
        {
            base.Start();
        }

        public override void Initialise()
        {
            m_muzzle = gameObject.FindChild("Projectile_Exit");
            m_trailPool = new ObjectPool<TrailRenderer>(CreateTrail);
            m_particleSystem = GetComponent<ParticleSystem>();

            base.Initialise();
        }

        // Update is called once per frame
        protected override void Update()
        {
            base.Update();
        }

        public override void Fire()
        {
            var deltaTime = Time.time - m_lastShotTime;
            var canFire = this.CanFire && (deltaTime > 1.0f / m_shootConfig.FireRate);

            if (canFire)
            {
                m_lastShotTime = Time.time;
                m_particleSystem.Play();
                base.Fire();

                var shootDir = m_muzzle.transform.forward +
                    new Vector3(
                        Random.Range(-m_shootConfig.Spread.x, m_shootConfig.Spread.x),
                        Random.Range(-m_shootConfig.Spread.y, m_shootConfig.Spread.y),
                        Random.Range(-m_shootConfig.Spread.z, m_shootConfig.Spread.z));

                var shootPos = m_muzzle.transform.position;
                var rayCastHit = Physics.Raycast(shootPos, shootDir, out RaycastHit hit, m_shootConfig.Range, m_shootConfig.HitMask);

                if (rayCastHit)
                {
                    StartCoroutine(this.PlayTrail(shootPos, hit.point, hit));

                    var victim = hit.collider.gameObject.QueryParents(x => x.tag == "Player");

                    if (victim != null)
                    {
                        var healthSys = victim.GetComponent<Health>();
                        var actorState = victim.GetComponent<ActorState>();
                        var damage = this.GetComponent<Damage>();

                        if (healthSys != null)
                        {
                            healthSys.RegisterDamage(damage);
                        }

                        if (actorState != null)
                        {
                            actorState.LastHitBy = this.gameObject;
                        }

                        NotificationService.Instance.NotifyPlayerAttacked(victim);
                    }
                }
                else
                {
                    StartCoroutine(PlayTrail(shootPos, shootPos + shootDir * m_shootConfig.Range, new RaycastHit()));
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
                instance.transform.position = Vector3.Lerp(startPoint, endPoint, Mathf.Clamp01(1 - remainingDistance / distance));
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
}