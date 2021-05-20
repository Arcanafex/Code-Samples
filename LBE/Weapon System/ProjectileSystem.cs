using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public abstract class ProjectileSystem : NetworkBehaviour
{
    public enum ProjectileDeliveryMethod
    {
        Rigidbody,
        Raycast
    }

    public enum FiringMode
    {
        Single,
        Burst,
        Auto
    }

    public enum ProjectileSpawnPointProgression
    {
        Progressive,
        Random,
        Concurrant
    }

    public GameObject[] projectilePrefabs;
    public ProjectileDeliveryMethod delivery;
    protected int projectilePrefabIndex = 0;

    public GameObject projectilePrefab
    {
        get
        {
            if (projectilePrefabIndex > -1 && projectilePrefabIndex < projectilePrefabs.Length)
                return projectilePrefabs[projectilePrefabIndex];
            else
                return null;
        }
    }

    public Transform[] projectileSpawnPoints;
    public ProjectileSpawnPointProgression progression;
    protected int projectileSpawnIndex = 0;

    public Transform ProjectileSpawnPoint
    {
        get
        {
            if (projectileSpawnPoints.Length == 1) 
            {
                return projectileSpawnPoints[0];
            }
            else if (projectileSpawnIndex < projectileSpawnPoints.Length)
            {
                return projectileSpawnPoints[projectileSpawnIndex];
            }
            else
            {
                return transform;
            }
        }
    }

    public Transform NextProjectileSpawnPoint
    {
        get
        {
            Transform currentPoint = transform;

            if (projectileSpawnPoints.Length == 1)
            {
                currentPoint = projectileSpawnPoints[0];
            }
            else if (projectileSpawnPoints.Length > 1)
            {
                if (progression == ProjectileSpawnPointProgression.Progressive)
                {
                    projectileSpawnIndex = (projectileSpawnIndex + 1) % projectileSpawnPoints.Length;
                    currentPoint = projectileSpawnPoints[projectileSpawnIndex];
                }
                else
                {
                    int randomIndex = Random.Range(0, projectileSpawnPoints.Length);

                    while (projectileSpawnIndex == randomIndex)
                    {
                        randomIndex = Random.Range(0, projectileSpawnPoints.Length);
                    }

                    projectileSpawnIndex = randomIndex;
                    currentPoint = projectileSpawnPoints[projectileSpawnIndex];
                }
            }

            if (currentPoint)
                return currentPoint;
            else
                return transform;
        }
    }

    public ProjectileSpawned OnProjectileSpawned;
    public ProjectileRaycast OnProjectileRaycast;

    public Player m_Owner = null;

    [SyncVar]
    public float triggerDown;
    public bool TriggerDown
    {
        get { return triggerDown > 0; }
        set { triggerDown = value ? 1 : 0; }
    }

    protected virtual void Awake()
    {
        foreach (var prefab in projectilePrefabs)
        {
            ClientScene.RegisterPrefab(prefab);
        }

        OnProjectileSpawned = new ProjectileSpawned();
        OnProjectileRaycast = new ProjectileRaycast();
        TriggerDown = false;
    }

    protected virtual void OnEnable()
    {
        //UnityEngine.SceneManagement.SceneManager.sceneLoaded += UnstickTrigger;
    }

    protected virtual void OnDisable()
    {
        TriggerDown = false;
        //UnityEngine.SceneManagement.SceneManager.sceneLoaded -= UnstickTrigger;
    }

    protected virtual void Start()
    {
    }

    protected virtual void Update()
    {
    }

    public virtual void ToggleTrigger()
    {
        TriggerDown = !TriggerDown;
    }

    public virtual void ToggleTrigger(bool down)
    {
        TriggerDown = down;
    }

    public virtual void NextProjectileIndex()
    {
        projectilePrefabIndex = (projectilePrefabIndex + 1) % projectilePrefabs.Length;
    }

    public virtual void PreviousProjectileIndex()
    {
        projectilePrefabIndex = projectilePrefabIndex > 0 ? projectilePrefabIndex - 1 : projectilePrefabs.Length - 1;
    }

    public virtual void SetProjectileIndex(int index)
    {
        projectilePrefabIndex = Mathf.Clamp(index, 0, projectilePrefabs.Length);
    }

    [Server]
    public virtual void Fire()
    {
        Fire(projectilePrefabIndex);
    }

    [Server]
    public virtual void Fire(int projectileIndex)
    {
        SetProjectileIndex(projectilePrefabIndex);

        if (progression == ProjectileSpawnPointProgression.Concurrant)
        {
            foreach (Transform currentSpawnPoint in projectileSpawnPoints)
            {
                Fire(currentSpawnPoint);
            }
        }
        else
        {
            var currentSpawnPoint = NextProjectileSpawnPoint;

            Fire(currentSpawnPoint);
        }
    }

    protected virtual void Fire(Transform origin)
    {
        GameObject prefab = projectilePrefab;
        var projectile = prefab.GetComponent<Projectile>();
        int faceIndex = -1;
        string playerID = "";

        if (delivery == ProjectileDeliveryMethod.Rigidbody)
        {
            var projectileInstance = projectile.isPooled ? NetworkObjectPool.Instance.GetInstance(prefab) : Instantiate(prefab);
            projectileInstance.transform.position = origin.position;
            projectileInstance.transform.rotation = origin.rotation;

            if (!projectile.isPooled)
            {
                projectileInstance.gameObject.SetActive(true);
                NetworkServer.Spawn(projectileInstance);
            }

            if (OnProjectileSpawned != null)
                OnProjectileSpawned.Invoke(projectileInstance);

            RpcFire(projectileInstance);
        }
        else if (delivery == ProjectileDeliveryMethod.Raycast)
        {
            var hit = new RaycastHit();

            if (Physics.Raycast(origin.position, origin.forward, out hit, 100.0f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                Debug.DrawRay(origin.position, origin.forward, Color.green);
                var damageable = hit.transform.GetComponentInParent<Damageable>();

                if (damageable && projectile)
                {
                    damageable.RaycastHit(projectile.profile.damage, prefab, hit, m_Owner);
                    if (!damageable.Alive)
                    {
                        //damageable.Hit(projectile.profile.damage, projectile.gameObject, null, false, hit.point.x, hit.point.y, hit.point.z);
                        DestructableObject dest = hit.transform.GetComponent<DestructableObject>();
                        if (dest != null)
                        {
                            Vector3 vel = origin.forward * projectile.profile.speed;
                            float mass = projectile.GetComponent<Rigidbody>().mass;
                            dest.RaycastDestruct(mass, vel);
                            dest.RpcRaycastDestruct(mass, vel);
                        }
                    }
                }
                // Face damage
                BulletTriggerForFace btff = (BulletTriggerForFace)hit.transform.GetComponent(typeof(BulletTriggerForFace));
                if (btff != null) {
                    faceIndex = btff.FindHitIndex(hit.point);
                    playerID = btff.playerRef.sPlrData.playerID;
                }
            }

            OnProjectileRaycast.Invoke(projectile, hit);
            RpcRaycast(origin.position, origin.forward, faceIndex, playerID);
        }
    }

    [ClientRpc]
    protected virtual void RpcFire(GameObject projectile)
    {
        if (!isServer)
        {
            projectile.Activate();
            projectile.transform.position = ProjectileSpawnPoint.position;
            projectile.transform.rotation = ProjectileSpawnPoint.rotation;

            if (OnProjectileSpawned != null)
                OnProjectileSpawned.Invoke(projectile);
        }
    }

    [ClientRpc]
    protected virtual void RpcRaycast(Vector3 origin, Vector3 direction, int faceHitIndex, string playerID)
    {
        GameObject prefab = projectilePrefab;
        var projectile = prefab.GetComponent<Projectile>();

        var hit = new RaycastHit();
        //var projectile = prefab.GetComponent<Projectile>();

        bool hitsomething = Physics.Raycast(origin, direction, out hit, 100.0f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        if (OnProjectileRaycast != null)
        {
            OnProjectileRaycast.Invoke(projectile, hit);
        }       

        if (faceHitIndex != -1) {
            PlayerController_Optitrack playerHit = (PlayerController_Optitrack)Spaces.LBE.SpacesGameManager.Instance.idToPlayerMap[playerID];
            playerHit.faceTrigger.SetFaceIndexHit(faceHitIndex);
        }
    }

    protected virtual void UnstickTrigger(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        if (TriggerDown)
        {
            ToggleTrigger(false);
        }
    }
}

public class ProjectileSpawned : UnityEvent<GameObject> { }
public class ProjectileRaycast : UnityEvent<Projectile, RaycastHit> { }
