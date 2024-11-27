using Assets.Scripts.Pickups;
using Assets.Scripts.Pickups.Weapons;
using Assets.Scripts.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Network
{
    /// <summary>
    /// Weapon spawn point data
    /// </summary>
    internal class WeaponSpawnPointData
    {
        public int SpawnInstanceId { get; set; }
        public WeaponSpawnPoint SpawnPoint { get; set; }
        public bool IsAvailable { get; set; }
        public bool BeenUsed { get; set; }
        public bool BeenDropped { get; set; }
        public Timer RespawnTimer { get; set; }
        public Weapon Weapon { get; set; }
    }

    /// <summary>
    /// Weapon type defenition
    /// </summary>
    [Serializable]
    public struct WeaponTypeDefenition
    {
        [SerializeField]
        public WeaponType Type;

        [SerializeField]
        public GameObject Prefab;
    }

    /// <summary>
    /// Weapn spawn manager
    /// </summary>
    public class WeaponSpawnManager : NetworkBehaviour
    {
        /// <summary>
        /// A list of all available weapon prefabs
        /// </summary>
        [SerializeField]
        private List<WeaponTypeDefenition> m_availableWeaponPrefabs;

        /// <summary>
        /// Respawn timeout
        /// </summary>
        [SerializeField]
        private float m_respawnTimeout;

        /// <summary>
        /// A list of all spawn points
        /// </summary>
        private List<WeaponSpawnPoint> m_spawnPoints;

        /// <summary>
        /// Respawn timespan
        /// </summary>
        private TimeSpan m_respawnTimeSpan;

        /// <summary>
        /// Weapon dictionary
        /// </summary>
        private Dictionary<int, Weapon> m_weaponDictionary;

        /// <summary>
        /// Weapon spawn data
        /// </summary>
        private Dictionary<int, WeaponSpawnPointData> m_weaponSpawnData;

        /// <summary>
        /// Constructor
        /// </summary>
        public WeaponSpawnManager()
        {
            m_respawnTimeout = 60.0f;
            m_spawnPoints = new List<WeaponSpawnPoint>();
            m_weaponDictionary = new Dictionary<int, Weapon>();
            m_weaponSpawnData = new Dictionary<int, WeaponSpawnPointData>();
            m_respawnTimeSpan = TimeSpan.FromSeconds(m_respawnTimeout);
        }

        /// <summary>
        /// Called before first frame
        /// If is server
        ///     -> Refreshes all spawn points for current scene
        ///     -> Initialises member data sturctures
        ///     -> Spawns in all weapons
        /// </summary>
        private void Start()
        {
            if (this.IsServer)
            {
                m_spawnPoints.Clear();
                m_spawnPoints = this.GetAllWeaponSpawnPointsInScene(SceneManager.GetActiveScene());

                this.InitialiseSpawnDataDictionary();
                this.InitialiseAndSpawnAllWeapons();
            }
        }

        /// <summary>
        /// Called every frame
        /// -> If server
        ///     -> Checks all weapon ownership states
        ///     -> Check sall spawn timers
        /// </summary>
        private void Update()
        {
            if (this.IsServer)
            {
                this.CheckWeaponOwnershipState();
                this.CheckSpawnTimers();
            }
        }

        /// <summary>
        /// Spawns weapon
        /// </summary>
        /// <param name="weaponType">Type of weapon to be spawned</param>
        /// <param name="position">Position to spawn the weapon at</param>
        /// <returns>Spawned weapon</returns>
        public Weapon SpawnWeapon(WeaponType weaponType, Vector3 position)
        {
            if (this.IsServer)
            {
                var prefab = this.GetPrefabByWeaponType(weaponType);
                var obj = GameObject.Instantiate(prefab);
                var netObj = obj.GetComponent<NetworkObject>();
                netObj.Spawn(true);

                var rb = obj.GetComponent<Rigidbody>();
                rb.constraints = RigidbodyConstraints.FreezeAll;

                var wpn = obj.GetComponent<Weapon>();
                wpn.Initialise();
                this.RegisterWeapon(wpn);
                return wpn;
            }

            return null;
        }

        /// <summary>
        /// Registers weapon with this manager
        /// </summary>
        /// <param name="wpn">Weapon to be registered</param>
        public void RegisterWeapon(Weapon wpn)
        {
            if (this.IsServer)
            {
                var key = wpn.GetInstanceID();

                if (!m_weaponDictionary.ContainsKey(key))
                {
                    m_weaponDictionary.Add(key, wpn);
                }
            }
        }

        /// <summary>
        /// Despawns all registered weapons
        /// </summary>
        public void DespawnAllRegisteredWeapons()
        {
            if (this.IsServer)
            {
                var allItems = m_weaponDictionary.Values.ToList();

                foreach (var item in allItems)
                {
                    var netObj = item.GetComponent<NetworkObject>();

                    if (netObj.IsSpawned)
                    {
                        netObj.Despawn(true);
                    }
                }
            }
        }

        /// <summary>
        /// Gets all spawn points within the scene specified
        /// </summary>
        /// <param name="scene">Scene to query</param>
        /// <returns>Spawn points from within 'scene'</returns>
        private List<WeaponSpawnPoint> GetAllWeaponSpawnPointsInScene(Scene scene)
        {
            var result = new List<WeaponSpawnPoint>();
            var rootObjs = scene.GetRootGameObjects();

            foreach (var obj in rootObjs)
            {
                if (obj == this)
                {
                    continue;
                }

                var spawnPoints = obj.GetComponentsInChildren<WeaponSpawnPoint>();
                result.AddRange(spawnPoints);
            }

            return result;
        }

        /// <summary>
        /// Initialises the spawn data dictionary
        /// -> For every spawn point
        ///     -> Create a new spawn point data instance, and populate it with information from the spawn point
        /// </summary>
        private void InitialiseSpawnDataDictionary()
        {
            foreach (var spawn in m_spawnPoints)
            {
                var instanceId = spawn.GetInstanceID();

                var data = new WeaponSpawnPointData();
                m_weaponSpawnData[instanceId] = data;
                data.IsAvailable = true;
                data.SpawnInstanceId = instanceId;
                data.SpawnPoint = spawn;
                data.RespawnTimer = new Timer(m_respawnTimeSpan);
                data.RespawnTimer.AutoReset = false;
            }
        }

        /// <summary>
        /// Fetches weapon prefab by specified weapon type.
        /// </summary>
        /// <param name="weaponType">The weapon type</param>
        /// <returns>Weapon prefab</returns>
        private GameObject GetPrefabByWeaponType(WeaponType weaponType)
        {
            foreach (var item in m_availableWeaponPrefabs)
            {
                if (item.Type == weaponType)
                {
                    return item.Prefab;
                }
            }

            return null;
        }

        /// <summary>
        /// Initialises and spawns all weapons for all spawn points
        /// </summary>
        private void InitialiseAndSpawnAllWeapons()
        {
            foreach (var spawn in m_spawnPoints)
            {
                var prefab = this.GetPrefabByWeaponType(spawn.WeaponType);

                if (prefab == null)
                {
                    NotificationService.Instance.Warning($"Could not find weapon prefab for specified weapon type : {spawn.WeaponType}");
                    continue;
                }

                var data = m_weaponSpawnData[spawn.GetInstanceID()];

                if (!data.IsAvailable)
                {
                    NotificationService.Instance.Warning("Attempting to spawn a weapon on a spawn point that is not available.");
                    continue;
                }

                var obj = GameObject.Instantiate(prefab);

                obj.transform.position = data.SpawnPoint.SpawnPosition;
                obj.transform.rotation = data.SpawnPoint.transform.rotation;

                var network = obj.GetComponent<NetworkObject>();
                network.Spawn(true);

                data.IsAvailable = false;
                data.Weapon = obj.GetComponent<Weapon>();

                if (data.Weapon != null)
                {
                    var rb = obj.GetComponent<Rigidbody>();
                    rb.constraints = RigidbodyConstraints.FreezeAll;
                }
            }
        }

        /// <summary>
        /// Gets spawn point data by object instance Id
        /// </summary>
        /// <param name="instanceId"></param>
        /// <returns></returns>
        private WeaponSpawnPointData GetWeaponSpawnPointDataByInstanceId(int instanceId)
        {
            return m_weaponSpawnData[instanceId];
        }

        /// <summary>
        /// Checks spawn timers
        /// If any timers have been lapsed, reset the associated weapon and reset the timer.
        /// </summary>
        private void CheckSpawnTimers()
        {
            foreach (var spawn in m_weaponSpawnData.Values)
            {
                spawn.RespawnTimer.Tick();

                if (spawn.RespawnTimer.Started && spawn.RespawnTimer.Elapsed)
                {
                    this.RespawnWeapon(spawn);
                    spawn.RespawnTimer.ResetTimer();
                }
            }
        }

        /// <summary>
        /// Respawns weapon for weapon spawn point data
        /// </summary>
        /// <param name="spawn">Weapon spawn point data</param>
        private void RespawnWeapon(WeaponSpawnPointData spawn)
        {
            if (spawn.Weapon != null)
            {
                spawn.Weapon.Reset();
                spawn.Weapon.ResetRigidBody();

                spawn.Weapon.transform.position = spawn.SpawnPoint.SpawnPosition;
                spawn.Weapon.transform.rotation = spawn.SpawnPoint.transform.rotation;
            }
        }

        /// <summary>
        /// Checks which weapons are currently in use by anyone
        /// </summary>
        private void CheckWeaponOwnershipState()
        {
            foreach (var spawn in m_weaponSpawnData.Values)
            {
                if (spawn.Weapon == null)
                {
                    //NotificationService.Instance.Warning("No weapon has been assigned to spawn point");
                    continue;
                }

                if (spawn.Weapon.Owner != null)
                {
                    spawn.BeenUsed = true;
                    spawn.RespawnTimer.ResetTimer();
                }
                else if (spawn.Weapon.Owner == null && spawn.BeenUsed)
                {
                    spawn.RespawnTimer.Start();
                }
            }
        }

        /// <summary>
        /// Randomly gets an available spawn point
        /// </summary>
        /// <returns></returns>
        private WeaponSpawnPointData GetAvailableSpawnPoint()
        {
            var availableSpawns = m_weaponSpawnData.Values.Where(x => x.IsAvailable).ToList();

            if (!availableSpawns.Any())
            {
                var rndIdx = UnityEngine.Random.Range(0, availableSpawns.Count() - 1);
                return availableSpawns[rndIdx];
            }

            NotificationService.Instance.Warning("No available spawn points left.");

            return null;
            
        }

    }
}
