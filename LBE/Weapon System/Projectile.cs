using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : NetworkBehaviour, IPoolable
{
    public ProjectileProfile profile;

    public float ProjectileLifetime { get; set; }
    public int Damage { get; set; }
    public bool DestroyedOnImpact { get; set; }

    public bool isRocket { get { return profile.isRocket; } }
    public bool isExplosive { get { return profile.isExplosive; } }

    private Rigidbody _projectileBody;
    public Rigidbody projectileBody
    {
        get
        {
            if (!_projectileBody)
                _projectileBody = GetComponent<Rigidbody>();

            return _projectileBody;
        }
        set
        {
            _projectileBody = value;
        }
    }

    public bool isPooled { get { return poolSize > 0; } }
    public int poolSize = 20;
    protected bool isPrimed = true;

    public UnityEvent OnProjectileSpawned;
    public UnityEvent OnProjectileUpdate;
    public UnityEvent OnProjectileHit;
    public UnityEvent OnProjectileDestroyed;

    public RocketEffect rocket;
    public ExplosiveEffect explosive;

    protected virtual void Awake()
    {
        if (!profile)
            profile = ScriptableObject.CreateInstance<ProjectileProfile>();

        ProjectileLifetime = profile.projectileLifetime;
        Damage = profile.damage;
        DestroyedOnImpact = profile.destroyedOnImpact;
    }

    protected virtual void OnEnable()
    {
        isPrimed = true;
        StartCoroutine(DestroyProjectile(ProjectileLifetime));
    }

    protected virtual void OnDisable()
    {
        //Debug.Log(this.name + "." + this.GetType().ToString() + " [DISABLED]");
        StopAllCoroutines();
        projectileBody.velocity = Vector3.zero;
        projectileBody.angularVelocity = Vector3.zero;

    }

    protected virtual void Start()
    {
        if (OnProjectileSpawned == null)
            OnProjectileSpawned = new UnityEvent();

        if (OnProjectileUpdate == null)
            OnProjectileUpdate = new UnityEvent();

        if (OnProjectileHit == null)
            OnProjectileHit = new UnityEvent();

        if (OnProjectileDestroyed == null)
            OnProjectileDestroyed = new UnityEvent();

        OnProjectileDestroyed.AddListener(DestroyGameObject);
    }

    protected virtual void Update()
    {
        if (isPrimed)
        {
            isPrimed = false;
            InitializeProjectile();
        }

        OnProjectileUpdate.Invoke();
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("[" + this.GetType().ToString() + "] hit");

        if (isServer)
        {
            var hit = collision.gameObject;
            var damageable = hit.gameObject.GetComponentInParent<Damageable>();

            if (damageable)
            {
                if (!isClient)
                    OnProjectileHit.Invoke();

                RpcProjectileHit();

                damageable.Hit(profile.damage, gameObject, collision);
            }

            if (DestroyedOnImpact)
            {
                DestroyProjectile();
            }

            HapticCollisionTrigger trigger = collision.collider.gameObject.GetComponent(typeof(HapticCollisionTrigger)) as HapticCollisionTrigger;
            if (trigger && collision.contacts.Length > 0) {
                trigger.TriggerHaptic(collision.contacts[0].point);
            }
        }
        else
        {
            if (DestroyedOnImpact)
            {
                OnProjectileDestroyed.Invoke();
            }
        }
    }

    //public void PrimeProjectile()
    //{
    //    if (!gameObject.activeSelf)
    //        gameObject.SetActive(true);

    //    isPrimed = true;
    //    StartCoroutine(DestroyProjectile(ProjectileLifetime));
    //}

    protected void InitializeProjectile()
    {
        OnProjectileSpawned.Invoke();

        if (isRocket && profile.rocketProfile != null)
        {
            if (rocket == null)
                rocket = new RocketEffect();

            rocket.Initialize(this.gameObject, profile.rocketProfile);

            if (rocket.IgnitionDelay == 0)
                rocket.Ignite();

            StartCoroutine(rocket.UpdateRocket(projectileBody));
        }

        if (isExplosive && profile.explosiveProfile != null)
        {
            if (explosive == null)
                explosive = new ExplosiveEffect();

            explosive.Initilize(this.gameObject, profile.explosiveProfile);
            OnProjectileDestroyed.AddListener(explosive.Detonate);
        }

        if (profile.SoundGroupName.Length > 0 && isClient) {
            DarkTonic.MasterAudio.MasterAudio.PlaySound3DAtTransform(profile.SoundGroupName, this.transform);
        }
    }

    public void DestroyProjectile()
    {
        OnProjectileDestroyed.Invoke();

        if (isServer)
            RpcProjectileDestroyed();
    }

    public void DestroyGameObject()
    {
        if (isPooled)
        {
            NetworkObjectPool.Instance.ReturnInstance(gameObject);
        }
        else
        {
            if (isServer)
            {
                NetworkServer.Destroy(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    protected IEnumerator DestroyProjectile(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        OnProjectileDestroyed.Invoke();
    }

    [ClientRpc]
    protected virtual void RpcProjectileHit()
    {
        if (!isServer)
            OnProjectileHit.Invoke();
    }

    [ClientRpc]
    protected virtual void RpcProjectileDestroyed()
    {
        if (!isServer)
            OnProjectileDestroyed.Invoke();
    }

    //public void Reset()
    //{
    //    projectileBody.velocity = Vector3.zero;
    //    projectileBody.angularVelocity = Vector3.zero;

    //    foreach (var particleSystem in GetComponentsInChildren<ParticleSystem>())
    //    {
    //        particleSystem.Stop();
    //        particleSystem.Clear();
    //        particleSystem.Play();
    //    }

    //    foreach (var trail in GetComponentsInChildren<TrailRenderer>())
    //    {
    //        trail.Clear();
    //    }
    //}

    public int GetPoolSize()
    {
        return poolSize;
    }
}

