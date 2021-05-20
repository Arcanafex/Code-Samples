using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace Spaces.LBE
{
    [System.Serializable]
    public class GunPool
    {
        public enum GunType
        {
            Striker,
            PPGun_Red,
            PPGun_Orange,
            PPGun_MouseRifle,
            StrikerArena
        }

        [System.Serializable]
        public class GunInfo
        {
            public string name;
            public GunType m_GunType;
            public GameObject m_Prefab;
        }

        public List<GunInfo> m_Guns = new List<GunInfo>();
    }

    public class GunPoolManager : NetworkBehaviour
    {
        public GunPool m_GunPool;

        private static GunPoolManager instance;
        private static HashSet<GameObject> s_spawnedGuns;
        private bool m_Spawned = false;
        private List<NetworkConnection> m_alreadySpawned;

        private void Awake()
        {
            if (!instance)
            {
                instance = this;
                RegisterGunPrefabs();
                s_spawnedGuns = new HashSet<GameObject>();
                PersistentObjectManager.Add(gameObject);
            }
            else
            {
                this.enabled = false;
            }
        }

        private void OnDestroy()
        {
            foreach (var gun in s_spawnedGuns)
            {
                if (gun)
                    Destroy(gun);
            }

            if (s_spawnedGuns != null)
                s_spawnedGuns.Clear();
        }

        private void Start()
        {
            if (isServer)
            {
                SpawnGuns();
            }
        }

        private void Update()
        {
            if (isServer)
                SpawnGuns();
        }

        public GameObject FindPrefab(GunPool.GunType gunType)
        {
            if (m_GunPool.m_Guns == null)
            {
                Spaces.LBE.DebugLog.Log("weapons", "GunPoolManager.FindPrefab [No Guns Defined]");
            }

            foreach (GunPool.GunInfo gunInfo in m_GunPool.m_Guns)
            {
                if (gunType == gunInfo.m_GunType)
                {
                    return gunInfo.m_Prefab;
                }
            }

            Spaces.LBE.DebugLog.Log("weapons", "GunPoolManager.FindPrefab [no prefab for gun type: " + gunType.ToString() + "]");
            return null;
        }

        public void RegisterGunPrefabs()
        {
            if (m_GunPool != null)
            {
                if (m_GunPool.m_Guns != null)
                {
                    foreach (var gun in m_GunPool.m_Guns)
                    {
                        Spaces.LBE.DebugLog.Log("weapons", "[Prefab Registered] " + gun.name);
                        ClientScene.RegisterPrefab(gun.m_Prefab);
                    }
                }
                else
                {
                    Spaces.LBE.DebugLog.Log("weapons", "GunPoolManager.RegisterGunPrefabs [GunPool Guns List is Null]");
                }
            }
            else
            {
                Spaces.LBE.DebugLog.Log("weapons", "GunPoolManager.RegisterGunPrefabs [GunPool is Null]");
            }
        }

        [Server]
        public void SpawnGuns()
        {
            if (!m_Spawned)
            {
                for (int i = 0; i < Spaces.LBE.MachineConfigurationManager.instance.numGuns; ++i)
                {
                    var gunDef = Spaces.LBE.MachineConfigurationManager.instance.GetGunDef(i);
                    

                    var prefab = FindPrefab(gunDef.m_GunType);

                    if (!prefab)
                    {
                        // No gun prefab found for this type
                        continue;
                    }

                    GameObject spawnedGun = Instantiate(prefab, TrackingVolume.SceneOrigin);
                    spawnedGun.transform.position = Vector3.zero;
                    spawnedGun.transform.rotation = Quaternion.Euler(Vector3.zero);

                    var trackedObject = spawnedGun.GetComponent<TrackedObject>();

                    if (trackedObject)
                    {
                        SetupGun(ref spawnedGun, gunDef.name);
                    }

                    if (!s_spawnedGuns.Add(spawnedGun))
                    {
                        Spaces.LBE.DebugLog.Log("weapons", "GunPoolManager.SpawnGuns [GunPool.SpawnGuns: Spawning gun " + spawnedGun.name + "]");
                    }
                }

                m_Spawned = true;
            }
            //else
            //{
            //    Spaces.LBE.DebugLog.Log("weapons", "GunPoolManager.SpawnGuns [repeated spawn calls]");
            //}

            if (m_alreadySpawned == null)
                m_alreadySpawned = new List<NetworkConnection>();

            bool needsSpawnUpdate = false;

            foreach (var connection in NetworkServer.connections)
            {
                if (connection == null)
                {
                    m_alreadySpawned.Remove(connection);
                }
                else if (!m_alreadySpawned.Contains(connection) && connection.isReady)
                {
                    m_alreadySpawned.Add(connection);
                    needsSpawnUpdate = true;
                }
            }

            if (needsSpawnUpdate)
            {
                foreach (var spawnedGun in s_spawnedGuns)
                {
                    NetworkServer.Spawn(spawnedGun);
                    RpcAddGun(spawnedGun, spawnedGun.name);
                }
            }
        }

        [ClientRpc]
        private void RpcAddGun(GameObject spawnedGun, string gunDefName)
        {
            if (s_spawnedGuns.Add(spawnedGun))
            {
                spawnedGun.transform.position = Vector3.zero;
                spawnedGun.transform.rotation = Quaternion.Euler(Vector3.zero);

                SetupGun(ref spawnedGun, gunDefName);

                Spaces.LBE.DebugLog.Log("weapons", "GunPoolManager.RpcAddGun [Adding Spawned Gun] " + spawnedGun.name);
            }
        }

        private void SetupGun(ref GameObject spawnedGun, string gunDefName)
        {
            var gunDef = Spaces.LBE.MachineConfigurationManager.instance.GetGunDef(gunDefName);

            if (gunDef == null)
            {
                Debug.LogError(this.name + " [No Definition Found] " + gunDefName);
                return;
            }

            spawnedGun.name = gunDef.name;
            var trackedObject = spawnedGun.GetComponent<TrackedObject>();

            if (trackedObject)
            {
                trackedObject.Initialize();
                trackedObject.SetOptitrackID(gunDef.iRigidBodyID);

                trackedObject.PropInfo = new TrackedObject.TrackedObjectInfo()
                {
                    name = gunDef.name,
                    propID = gunDef.m_GunType.ToString(),
                    prefab = FindPrefab(gunDef.m_GunType)
                };
            }

            StrikerVRInterface strikerInterface = GetStrikerVRInterface(spawnedGun);

            if (strikerInterface != null)
            {
                strikerInterface.initialize(gunDef.strikerAddress);
            }
        }

        protected virtual StrikerVRInterface GetStrikerVRInterface(GameObject spawnedGun)
        {
            var queue = new Queue<Transform>();
            queue.Enqueue(spawnedGun.transform);

            while (queue.Count > 0)
            {
                var currentObject = queue.Dequeue();
                var strikerInterface = currentObject.GetComponent<StrikerVRInterface>();

                if (strikerInterface)
                {
                    return strikerInterface;
                }

                foreach (Transform child in currentObject)
                {
                    queue.Enqueue(child);
                }
            }

            return null;
        }
    }
}
