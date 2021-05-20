using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ProjectilePool : MonoBehaviour
{
    public class Pool
    {
        public GameObject poolPrefab;
        public int poolMax;
        private List<GameObject> activePool;
        private Queue<GameObject> availablePool;

        public Pool(GameObject poolObject)
        {
            poolObject = poolPrefab;
            activePool = new List<GameObject>();
            availablePool = new Queue<GameObject>();
        }

        public GameObject GetObject()
        {
            GameObject poolObj = null;

            if (availablePool.Count > 0)
            {
                // Object available in inactive pool
                poolObj = availablePool.Dequeue();
                poolObj.SetActive(true);
            }
            else if (activePool.Count <= poolMax)
            {
                // No idle objects available, max active not yet reached
                poolObj = Instantiate(poolPrefab);
                poolObj.SetActive(true);
                activePool.Add(poolObj);
            }
            else
            {
                // None idle, and max active reached, so retrieve oldest active object
                ReturnObject(activePool.First());
                poolObj = availablePool.Dequeue();
            }

            return poolObj;
        }

        public void ReturnObject(GameObject poolObject)
        {
            if (activePool.Contains(poolObject) && activePool.Remove(poolObject))
            {
                poolObject.SetActive(false);
                //TODO: Re-initialize object? Free whatever resources?
                if (!availablePool.Contains(poolObject))
                    availablePool.Enqueue(poolObject);
            }
            else
            {
                //Either that's not a pool object, or there's something keeping it from being returned.
            }
        }

        public bool Contains(GameObject poolObject)
        {
            return activePool.Contains(poolObject) || availablePool.Contains(poolObject);
        }
    }


    protected static ProjectilePool instance;

    public static ProjectilePool Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<ProjectilePool>();

            return instance;
        }
    }



    protected List<Pool> pools;

    private void Awake()
    {
        if (pools == null)
            pools = new List<Pool>();
    }

    public GameObject GetPoolObject(GameObject projectile)
    {
        var pool = pools.FirstOrDefault(p => p.poolPrefab == projectile);

        if (pool == null)
        {
            pool = new Pool(projectile);
            pools.Add(pool);
        }

        return pool.GetObject();
    }

    public void ReturnPoolObject(GameObject projectile)
    {
        var pool = pools.FirstOrDefault(p => p.Contains(projectile));
        pool.ReturnObject(projectile);
    }
}
