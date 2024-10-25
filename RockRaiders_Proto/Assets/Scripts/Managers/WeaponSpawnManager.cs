using Assets.Scripts.Pickups.Weapons;
using Assets.Scripts.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace Assets.Scripts.Managers
{
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

    [Serializable]
    public struct WeaponTypeDefenition
    {
        [SerializeField]
        public WeaponType Type;

        [SerializeField]
        public GameObject Prefab;
    }

    public class WeaponSpawnManager : NetworkBehaviour
    {
        [SerializeField]
        private List<WeaponTypeDefenition> m_availableWeaponPrefabs;

        [SerializeField]
        private float m_respawnTimeout;

        [SerializeField]
        private List<WeaponSpawnPoint> m_spawnPoints;



        private TimeSpan m_respawnTimeSpan;


        private Dictionary<int, WeaponSpawnPointData> m_weaponSpawnData;

        public WeaponSpawnManager()
        {
            m_respawnTimeout = 60.0f;
            m_spawnPoints = new List<WeaponSpawnPoint>();
            m_weaponSpawnData = new Dictionary<int, WeaponSpawnPointData>();
            m_respawnTimeSpan = TimeSpan.FromSeconds(m_respawnTimeout);
        }

        private void Start()
        {
            if (this.IsServer)
            {
                this.InitialiseSpawnDataDictionary();
                this.InitialiseAndSpawnAllWeapons();
            }
            
        }


        private void Update()
        {
            if (this.IsServer)
            {
                this.CheckWeaponOwnershipState();
                this.CheckSpawnTimers();
            }
        }

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

        private GameObject GetPrefabByWeaponType(WeaponType weaponType)
        {
            foreach (var item in m_availableWeaponPrefabs)
            {
                if (item.Type == weaponType)
                {
                    return item.Prefab;
                }
            }

            throw new Exception("WeaponSpawnManager.GetPrefabByWeaponType | Couldn't find prefab for weapon type provided.");
        }

        private void InitialiseAndSpawnAllWeapons()
        {
            foreach (var spawn in m_spawnPoints)
            {
                var prefab = this.GetPrefabByWeaponType(spawn.WeaponType);

                var data = m_weaponSpawnData[spawn.GetInstanceID()];

                if (!data.IsAvailable)
                {
                    NotificationService.Instance.Warning("Attempting to spawn a weapon on a spawn point that is not available.");
                    continue;
                }

                var obj = GameObject.Instantiate(prefab);

                obj.transform.position = data.SpawnPoint.transform.position;
                obj.transform.rotation = data.SpawnPoint.transform.rotation;

                var network = obj.GetComponent<NetworkObject>();
                network.Spawn(true);

                data.IsAvailable = false;
                data.Weapon = obj.GetComponent<Weapon>();
            }
        }

        private WeaponSpawnPointData GetWeaponSpawnPointDataByInstanceId(int instanceId)
        {
            return m_weaponSpawnData[instanceId];
        }

        private WeaponSpawnPointData GetAvailableWeaponSpawnPoint()
        {
            foreach (var spawn in m_spawnPoints)
            {
                if (m_weaponSpawnData[spawn.gameObject.GetInstanceID()].IsAvailable)
                {
                    return m_weaponSpawnData[spawn.gameObject.GetInstanceID()];
                }
            }

            return null;
        }

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

        private void RespawnWeapon(WeaponSpawnPointData spawn)
        {
            if (spawn.Weapon != null)
            {
                spawn.Weapon.Reset();
                spawn.Weapon.transform.position = spawn.SpawnPoint.transform.position;
                spawn.Weapon.transform.rotation = spawn.SpawnPoint.transform.rotation;
            }
        }

        private void CheckWeaponOwnershipState()
        {
            foreach (var spawn in m_weaponSpawnData.Values)
            {
                if (spawn.Weapon == null)
                {
                    NotificationService.Instance.Warning("No weapon has been assigned to spawn point");
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
