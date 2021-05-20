using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;

public class NetworkObjectPool : NetworkBehaviour
{
    protected static NetworkObjectPool instance;
    protected GameObject poolItemContainer;
    protected List<NetworkConnection> m_updatedConnections;

    public static NetworkObjectPool Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<NetworkObjectPool>();

                if (instance == null)
                {
                    var pool = new GameObject("Object Pool");
                    instance = pool.AddComponent<NetworkObjectPool>();
                }
            }
            return instance;
        }
    }

    public List<Pool> pools;
    public const int DEFAULTPOOLSIZE = 10;

    protected void Awake()
    {
        m_updatedConnections = new List<NetworkConnection>();

        if (pools == null)
            pools = new List<Pool>();

        foreach (var pool in pools)
        {
            ClientScene.RegisterPrefab(pool.prefab);
            Spaces.LBE.DebugLog.Log("weapons", "[Prefab Registered] " + pool.prefab.name);
        }

        poolItemContainer = new GameObject("Network Pool Items");
    }

    //private void PlayerAdded(UnityEngine.Networking.NetworkMessage netMsg)
    //{
    //    Debug.Log("[SERVER] This happened: " + netMsg.conn.hostId);
    //}

    public override void OnStartServer()
    {
        base.OnStartServer();
        //NetworkServer.RegisterHandler(MsgType.AddPlayer, PlayerAdded);
        PopulatePools();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
    }

    private void Update()
    {
        if (isServer)
        {
            bool needsPoolUpdate = false;

            foreach (var connection in NetworkServer.connections)
            {                
                if (connection == null)
                {
                    m_updatedConnections.Remove(connection);
                }
                else if (!m_updatedConnections.Contains(connection) && connection.isReady)
                {
                    m_updatedConnections.Add(connection);
                    needsPoolUpdate = true;
                    Spaces.LBE.DebugLog.Log("networking", "[Object Pool Update] connection: " + connection.address);
                }
            }

            if (needsPoolUpdate)
            {
                for (int p = 0; p < pools.Count; p++)
                {
                    foreach (var item in pools[p].items)
                    {
                        NetworkServer.Spawn(item);
                        RpcUpdateInstance(p, item);
                    }
                }

                needsPoolUpdate = false;
            }
        }
    }

    [Server]
    protected void ClearUpdatedConnections()
    {
        if (m_updatedConnections == null)
            m_updatedConnections = new List<NetworkConnection>();
        else
            m_updatedConnections.Clear();
    }

    [Server]
    protected Pool CreatePool(GameObject prefab, int poolSize = DEFAULTPOOLSIZE)
    {
        Spaces.LBE.DebugLog.Log("networking", "[SERVER] Pool Created " + prefab.name);

        if (pools == null)
            pools = new List<Pool>();

        var pool = pools.FirstOrDefault(p => p.prefab == prefab);
        int index = pool != null ? pools.IndexOf(pool) : pools.Count;

        if (pool == null)
        {
            pool = new Pool(prefab, poolSize);
            pools.Add(pool);
        }

        return pool;
    }

    [Server]
    protected void PopulatePools()
    {
        pools.ForEach(pool => pool.PopulatePool());
    }

    [Server]
    public GameObject GenerateItem(GameObject prefab)
    {
        var pool = pools.FirstOrDefault(p => p.prefab.Equals(prefab));
        return GenerateItem(pool, prefab);
    }

    [Server]
    public GameObject GenerateItem(Pool pool, GameObject prefab)
    {
        if (pool == null)
        {
            pool = CreatePool(prefab);
        }

        var poolItem = Instantiate(prefab, poolItemContainer.transform);
        poolItem.SetActive(true);
        poolItem.Suspend();
        //NetworkServer.Spawn(poolItem);
        Spaces.LBE.DebugLog.Log("networking", "[SERVER] Generated pool item: " + poolItem.name + "(" + poolItem.GetNetworkID() + ")");

        return poolItem;
    }

    [Server]
    public void AddInstance(Pool pool, GameObject poolItem)
    {
        if (pool == null)
        {
            pool = CreatePool(poolItem);
        }

        pool.AddItem(poolItem);
        ClearUpdatedConnections();
    }

    [Server]
    public GameObject GetInstance(GameObject prefab)
    {
        var pool = pools.FirstOrDefault(p => p.prefab == prefab);

        if (pool == null)
        {
            var poolable = prefab.GetComponent<IPoolable>();
            int poolSize = poolable != null ? poolable.GetPoolSize() : DEFAULTPOOLSIZE;

            pool = CreatePool(prefab, poolSize);
        }

        var poolObject = pool.GetItem();

        return poolObject;
    }

    public void ReturnInstance(GameObject poolItem)
    {
        var pool = pools.FirstOrDefault(p => p.Contains(poolItem));

        if (pool != null)
        {
            pool.ReturnItem(poolItem);
        }
        else
        {
            poolItem.Suspend();
        }

        if (poolItem && poolItemContainer && poolItem.transform.parent != poolItemContainer.transform)
        {
            poolItem.transform.SetParent(poolItemContainer.transform, true);
        }
    }

    [Client]
    protected void UpdateInstance(Pool pool, GameObject poolItem)
    {
        if (poolItem)
        {
            if (pool != null)
            {
                pool.AddItem(poolItem);
            }

            if (poolItem && poolItemContainer && poolItem.transform.parent != poolItemContainer.transform)
            {
                poolItem.transform.SetParent(poolItemContainer.transform, true);
            }
        }
    }

    [ClientRpc]
    public void RpcUpdateInstance(int poolIndex, GameObject poolItem)
    {
        Spaces.LBE.DebugLog.Log("networking", "[CLIENT] Update received from Server for pool item: " + poolItem.name + "(" + poolItem.GetNetworkID() + ")");

        Pool pool = null;

        if (poolIndex < pools.Count)
        {
            pool = pools[poolIndex];
        }
        else
        {
            pool = new Pool(poolItem);
            pools.Insert(poolIndex, pool);
        }

        UpdateInstance(pool, poolItem);
    }

    [ClientRpc]
    protected void RpcReturnInstance(GameObject poolItem)
    {
        ReturnInstance(poolItem);
    }

    [System.Serializable]
    public class Pool
    {
        public string name;
        public GameObject prefab;
        public int size = DEFAULTPOOLSIZE;
        public List<GameObject> items;
        private int lastActivePulled = 0;

        public int ActiveCount
        {
            get
            {
                return items.Count(i => i.activeInHierarchy);
            }
        }

        public int AvailableCount
        {
            get
            {
                return items.Count(i => !i.activeInHierarchy);
            }
        }

        public Pool(GameObject poolItem, int size = DEFAULTPOOLSIZE)
        {
            name = poolItem.name + " [pool]";
            prefab = poolItem;
            this.size = size;
            items = new List<GameObject>();
        }

        protected GameObject GetFirstAvailableItem()
        {
            return items.FirstOrDefault(i => !i.activeInHierarchy);
        }

        protected GameObject GetNextActiveItem()
        {
            var item = items.First();

            if (lastActivePulled < items.Count)
            {
                item = items[lastActivePulled++];
            }
            else
            {
                lastActivePulled = 0;
            }

            return item;
        }

        public GameObject GetItem()
        {
            GameObject poolItem = null;

            // Object available in inactive pool
            poolItem = GetFirstAvailableItem();

            if (!poolItem)
            {
                // No idle objects available

                if (items.Count < size)
                {
                    //max active not yet reached
                    poolItem = GenerateItem();
                }
                else
                {
                    // None idle, and max active reached, so retrieve oldest active object
                    poolItem = GetNextActiveItem();
                    ReturnItem(poolItem);
                }
            }

            poolItem.Activate();
            return poolItem;
        }

        protected GameObject GenerateItem()
        {
            var poolItem = NetworkObjectPool.Instance.GenerateItem(this, prefab);

            return poolItem;
        }

        public void PopulatePool()
        {
            if (!prefab)
            {
                Debug.LogError(name + " [Pool prefab is null]");
                return;
            }

            if (items == null)
                items = new List<GameObject>();

            for (int i = 0; i < size; i++)
            {
                var item = NetworkObjectPool.Instance.GenerateItem(this, prefab);
                NetworkObjectPool.Instance.AddInstance(this, item);
            }
        }

        public void AddItem(GameObject poolItem)
        {
            if (!items.Contains(poolItem))
            {
                items.Add(poolItem);
            }
        }

        public void ReturnItem(GameObject poolItem)
        {
            poolItem.Suspend();
            AddItem(poolItem);
        }

        public bool Contains(GameObject poolItem)
        {
            return items.Contains(poolItem);
        }
    }
}

public interface IPoolable
{
    int GetPoolSize();
    //void Reset();
}

public static class GameObjectExtension
{
    public static void Activate(this GameObject instance)
    {
        if (!instance)
        {
            Debug.LogError("[Activate Failed] GameObject is null");
            return;
        }

        // Make sure network object is in active state so it will be correctly spawned across network.
        instance.SetActive(true);

        instance.GetComponentsInChildren<Behaviour>().ToList().ForEach(i => i.enabled = true);
        instance.GetComponentsInChildren<Collider>().ToList().ForEach(i => i.enabled = true);

        foreach (Transform child in instance.transform)
        {
            child.gameObject.SetActive(true);            
        }

        foreach (var particleSystem in instance.GetComponentsInChildren<ParticleSystem>())
        {
            particleSystem.Clear();
            particleSystem.Play();
        }

        foreach (var trail in instance.GetComponentsInChildren<TrailRenderer>())
        {
            trail.Clear();
        }
    }

    public static void Suspend(this GameObject instance)
    {
        // Make sure network object is in active state so it will be correctly spawned across network.
        instance.SetActive(true);

        instance.GetComponentsInChildren<Behaviour>().ToList().ForEach(i => i.enabled = false);
        instance.GetComponentsInChildren<Collider>().ToList().ForEach(i => i.enabled = false);

        foreach (Transform child in instance.transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    public static int GetNetworkID(this GameObject instance)
    {
        var netID = instance.GetComponent<NetworkIdentity>();

        return netID ? (int)netID.netId.Value : -1;
    }
}