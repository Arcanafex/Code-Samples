using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class GunSystem : ProjectileSystem
{
    public GunProfile profile;

    #region Profile Attributes
    [SyncVar]
    protected float m_fireForce;
    public float FireForce
    {
        get { return m_fireForce; }
        set { m_fireForce = value; }
    }

    [SyncVar]
    protected float m_fireRate;
    public float FireRate
    {
        get { return m_fireRate; }
        set { m_fireRate = value; }
    }

    [SyncVar]
    protected FiringMode m_fireMode = FiringMode.Single;
    public FiringMode FireMode
    {
        get { return m_fireMode; }
        set { m_fireMode = value; }
    }

    protected int m_burstSize;
    public int BurstSize
    {
        get { return m_burstSize; }
        set { m_burstSize = value; }
    }

    protected int m_magazineSize;
    public int MagazineSize
    {
        get { return m_magazineSize; }
        set { m_magazineSize = value; }
    }

    protected int m_reloadClipSize;
    public int ReloadClipSize
    {
        get { return m_reloadClipSize; }
        set { m_reloadClipSize = value; }
    }

    protected float m_warmUpTime;
    public float WarmUpTime
    {
        get { return m_warmUpTime; }
        set { m_warmUpTime = value; }
    }

    protected float m_overheatTime;
    public float OverheatTime
    {
        get { return m_overheatTime; }
        set { m_overheatTime = value; }
    }

    protected float m_cooldownTime;
    public float CooldownTime
    {
        get { return m_cooldownTime; }
        set { m_cooldownTime = value; }
    }

    #endregion

    [SyncVar]
    public int shotsRemaining;

    [SyncVar]
    protected int burstCount;
    public int FiredCount { get; private set; }

    protected float warmUpTimer;
    protected float overheatTimer;
    protected float cooldownTimer;

    protected bool lastStateTriggerPressed;
    protected bool waitingOnTriggerRelease;

    public Vector3 currentSpawnPoint;
    public Vector3 currentSpawnForward;
    public Quaternion currentSpawnRotation;

    public UnityEvent OnBulletFired;
    public UnityEvent OnMagazineEmpty;
    public UnityEvent OnReload;
    public UnityEvent OnBurstStart;
    public UnityEvent OnBurstDone;
    public UnityEvent OnFireStart;
    public UnityEvent OnFireEnd;

    public delegate bool OverrideAim(out Vector3 vTarget);
    public OverrideAim m_OverrideAim = null;

    protected override void Start()
    {
        base.Start();

        if (!profile)
            profile = new GunProfile();

        m_fireForce = profile.firingForce;
        m_fireRate = profile.firingRate;
        m_fireMode = profile.firingMode;
        m_burstSize = profile.burstSize;
        m_magazineSize = profile.magazineSize;
        m_reloadClipSize = profile.reloadClipSize;
        m_warmUpTime = profile.warmUpTime;
        m_overheatTime = profile.overheatTime;
        m_cooldownTime = profile.cooldownTime;

        shotsRemaining = m_magazineSize;
        burstCount = 0;
        FiredCount = 0;

        warmUpTimer = 0;
        overheatTimer = 0;
        cooldownTimer = 0;

        if (OnBulletFired == null)
            OnBulletFired = new UnityEvent();

        if (OnMagazineEmpty == null)
            OnMagazineEmpty = new UnityEvent();

        if (OnReload == null)
            OnReload = new UnityEvent();

        if (OnBurstStart == null)
            OnBurstStart = new UnityEvent();

        if (OnBurstDone == null)
            OnBurstDone = new UnityEvent();

        OnProjectileSpawned.AddListener(FireBullet);
        OnProjectileRaycast.AddListener(RaycastBullet);
    }

    private void LateUpdate() {
        currentSpawnPoint = ProjectileSpawnPoint.position;
        currentSpawnForward = ProjectileSpawnPoint.forward;
        currentSpawnRotation = ProjectileSpawnPoint.rotation;

        if (isServer) {
            if (cooldownTimer > 0)
                cooldownTimer -= Time.deltaTime;

            if (TriggerDown) {
                if (waitingOnTriggerRelease) {
                    // Check firing control
                } else if (cooldownTimer > 0) {
                    // Check firing rate
                } else {
                    // First frame trigger is pressed
                    if (!lastStateTriggerPressed) {
                        if (m_fireMode == FiringMode.Single)
                            waitingOnTriggerRelease = true;
                        RpcStartFire();
                    }

                    if (m_magazineSize > -1 && shotsRemaining <= 0) {
                        // Check remaining ammo
                        if (!isClient)
                            OnMagazineEmpty.Invoke();

                        RpcMagazineEmpty();
                    } else {
                        Fire();

                        // Update burst cycle
                        burstCount += 1;

                        if (burstCount >= m_burstSize) {
                            burstCount = 0;
                            waitingOnTriggerRelease = m_fireMode == FiringMode.Auto ? false : true;

                            if (!isClient)
                                OnBurstDone.Invoke();

                            RpcBurst(true);
                        }
                    }
                }
            } else {
                if (lastStateTriggerPressed) {
                    if (waitingOnTriggerRelease)
                        waitingOnTriggerRelease = false;
                    RpcStopFire();
                }
            }

            lastStateTriggerPressed = TriggerDown;
        }
    }

    protected void FireBullet(GameObject bullet)
    {
        if (isServer)
        {
            cooldownTimer = m_fireRate;
            FiredCount += 1;

            if (m_magazineSize > -1)
                shotsRemaining -= 1;
        }

        if (burstCount == 0)
            OnBurstStart.Invoke();

        bullet.transform.position = currentSpawnPoint;
        bullet.transform.rotation = currentSpawnRotation;

        var bulletBody = bullet.GetComponent<Rigidbody>();

        if (bulletBody)
        {
            Vector3 vAimAtTarget = Vector3.zero;
            if(m_OverrideAim != null &&  m_OverrideAim(out vAimAtTarget))
            {
                bulletBody.transform.LookAt(vAimAtTarget);
            }

            bulletBody.AddRelativeForce(Vector3.forward * m_fireForce, ForceMode.VelocityChange);
        }
            

        OnBulletFired.Invoke();
    }

    protected void RaycastBullet(Projectile bullet, RaycastHit hit)
    {
        if (isServer)
        {
            cooldownTimer = m_fireRate;
            FiredCount += 1;

            if (m_magazineSize > -1)
                shotsRemaining -= 1;

            if (burstCount == 0)
            {
                OnBurstStart.Invoke();
                RpcBurst(false);
            }
        }

        OnBulletFired.Invoke();

        // Simulate projectile hit
        if (hit.collider)
        {
            if (hit.rigidbody)
            {
                Vector3 bulletTrajectory = (hit.point - ProjectileSpawnPoint.position).normalized;
                Vector3 bulletVelocity = FireForce * bullet.projectileBody.mass * bulletTrajectory;
                //float bulletIncidence = Vector3.Dot(bulletVelocity, -hit.normal);

                hit.rigidbody.AddForceAtPosition(bulletVelocity, hit.point, ForceMode.Impulse);
                // hit.rigidbody.AddTorque(bulletVelocity * (1 - bulletIncidence), ForceMode.Impulse);
            }

            if (isServer)
            {
                var bulletCollider = bullet.gameObject.GetComponent<Collider>();
                var bulletEffectLayer = bullet.gameObject.GetComponent<DirectorEventCollision>();

                if (collisionParticleEffects.instance != null)
                {
                    collisionParticleEffects.instance.playParticles(this.GetInstanceID(), bulletCollider, hit.collider, hit.point, hit.normal);
                }

                if (sfxManager.instance != null)
                {
                    sfxManager.instance.playCollisionSfx(bulletCollider, hit.collider, hit.point);
                }

                if (Spaces.LBE.EventManager.instance != null && bulletEffectLayer)
                {
                    effectsLayers otherLayers = hit.collider.GetComponent<effectsLayers>();

                    if (otherLayers != null)
                    {
                        if (FXLayers.isMatch(bulletEffectLayer.m_AudioLayer.value, otherLayers.m_AudioLayer.value) ||
                                FXLayers.isMatch(bulletEffectLayer.m_ParticlesLayer.value, otherLayers.m_ParticlesLayer.value))
                        {
                            // FENG_TODO: Will replace the previous EventManager with SpacesEventManager. Now just handle the waterTowerHit first
                            if (bulletEffectLayer.m_CollisionEvent == Spaces.LBE.SpEvent.EventType.WaterTowerHit ) {
                                Spaces.LBE.SpacesEventManager.TriggerEvent(Spaces.LBE.SpacesEventType.Mister, null);
                            } else {
                                Spaces.LBE.EventManager.instance.SendEvent(new Spaces.LBE.SpEvent(bulletEffectLayer.m_CollisionEvent));
                            }

                        }
                    }
                }

                HapticCollisionTrigger hTrigger = hit.collider.GetComponent<HapticCollisionTrigger>();
                if (hTrigger) {
                    hTrigger.TriggerHaptic(hit.point);
                }
            }
        }

    }

    [Server]
    public void Reload()
    {
        shotsRemaining = Mathf.Clamp(shotsRemaining + m_reloadClipSize, 0, m_magazineSize);
        burstCount = 0;

        OnReload.Invoke();
        RpcReload();
    }

    [ClientRpc]
    protected void RpcReload()
    {
        if (!isServer)
            OnReload.Invoke();
    }

    [ClientRpc]
    protected void RpcMagazineEmpty()
    {
        if (!isServer)
            OnMagazineEmpty.Invoke();
    }

    [ClientRpc]
    protected void RpcBurst(bool burstDone)
    {
        if (!isServer)
        {
            if (!burstDone)
                OnBurstStart.Invoke();
            else
                OnBurstDone.Invoke();
        }
    }

    [ClientRpc]
    protected void RpcStartFire() {
        OnFireStart.Invoke();
    }

    [ClientRpc]
    protected void RpcStopFire() {
        OnFireEnd.Invoke();
    }
}
