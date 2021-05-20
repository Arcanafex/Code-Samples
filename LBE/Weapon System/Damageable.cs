using UnityEngine;
using System.Linq;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Events;

//*****************************************************************************************************************************
//*****************************************************************************************************************************
public class Damageable : NetworkBehaviour {

    public DamageableProfile profile;
    public DamageableProfile.DamageEffectState currentDamageState;

    [SyncVar]
    protected int health;

    [SyncVar]
    public int totalDamageTaken;

    [SyncVar]
    public float recentDamageTaken;

    public int RemainingHealth {
        get { return health; }
    }

    // Recording the attacker number for AI targetting choice and other usage
    [SyncVar]
    protected int m_attackerNum;
    public int AttackerNum {
        get { return m_attackerNum; }
        set { m_attackerNum = value; }
    }

    public bool Alive {
        get {
            return (health > profile.death);
        }
    }

    public DamageableHit OnHit;
    public DamageableRaycastHit OnRaycastHit;
    public UnityEvent OnTakeDamage;
    public UnityEvent OnDie;
    public UnityEvent OnDestroyed;

    // Damage types
    //  Enum must match string list. String list must match tags assigned in Unity prefabs
    public static readonly string DAMAGE_TAG_NAME_BULLET = "Bullet";              // Must match the Tag of bullets used by player GunSystems
    public static readonly string DAMAGE_TAG_NAME_ENEMYBULLET = "EnemyBullet";    // Must match the Tag of bullets used by enemy GunSystems
    [SerializeField]
    public static readonly string[] damageTags = {
        DAMAGE_TAG_NAME_BULLET,
        DAMAGE_TAG_NAME_ENEMYBULLET };
    public enum DamageType {
        Bullet,
        EnemyBullet,
        COUNT
    }

    [SerializeField]
    private DamageType[] damageTypes = { DamageType.Bullet, DamageType.EnemyBullet };   //The damage types are we vulnerable to.

    BoomFX boomComponent;

    //-------------------------------------------------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------------------------------------------------
    protected virtual void Awake() {
        if (OnHit == null)
            OnHit = new DamageableHit();

        if (OnRaycastHit == null)
            OnRaycastHit = new DamageableRaycastHit();

        if (OnTakeDamage == null)
            OnTakeDamage = new UnityEvent();

        if (OnDie == null)
            OnDie = new UnityEvent();

        if (OnDestroyed == null)
            OnDestroyed = new UnityEvent();

        if (!profile)
            profile = ScriptableObject.CreateInstance<DamageableProfile>();

        if ((int)DamageType.COUNT != damageTags.Length)
            Debug.LogError("Damage Types not setup properly. Enum must match taglist 'damageTags'. " + (int)DamageType.COUNT + " DamageTypes, " + damageTags.Length + " damage tags.");

    }

    //-------------------------------------------------------------------------------------------------------------------------
    protected virtual void Start() {
    health = profile.health;
        m_attackerNum = 0;
        boomComponent = gameObject.GetComponent<BoomFX>();
    }

    //-------------------------------------------------------------------------------------------------------------------------
    protected virtual void Update() {
        if (recentDamageTaken > 0) {
            recentDamageTaken = profile.recoveryRate < recentDamageTaken ? recentDamageTaken - profile.recoveryRate : 0;
        }
    }

    //-------------------------------------------------------------------------------------------------------------------------
    [Server]
    public virtual void TakeDamage(int damage) {
        if (Alive) {
            health -= damage;
            totalDamageTaken += damage;
            OnTakeDamage.Invoke();
            RpcTakeDamage(damage);

            if (!Alive) {
                Die();
            } else {
                EvaluateDamage(damage);
            }
        }
    }

    //-------------------------------------------------------------------------------------------------------------------------
    [Server]
    public virtual void TakeDamage(int damage, Collider collider) {
        TakeDamage(damage);
    }

    //-------------------------------------------------------------------------------------------------------------------------
	/// <summary>
	/// Hit function. Uses a Collision class by default. Optionally uses a explicit hit point.
	/// If using an explicit hit point, set 'useCollision' to false.
	/// </summary>
	/// <param name="damage"></param>
	/// <param name="damageSource"></param>
	/// <param name="collision"></param>
	/// <param name="useCollision"></param>
	/// <param name="hitPointx"></param>
	/// <param name="hitPointy"></param>
	/// <param name="hitPointz"></param>
	[Server]
	public virtual void Hit(int damage, GameObject damageSource, Collision collision = null, bool useCollision = true, float hitPointx = 0, float hitPointy = 0, float hitPointz = 0)
    {
        // Check for bullet type
        if (CompareTag(damageSource, damageTypes)) {
            TakeDamage(damage);
            OnHit.Invoke(damageSource, collision, useCollision, new Vector3(hitPointx, hitPointy, hitPointz));
        }
    }

    //-------------------------------------------------------------------------------------------------------------------------
    [Server]
    public virtual void RaycastHit(int damage, GameObject damageSource, RaycastHit hit, Player damageOwner) {
        // Check for bullet type
        if (CompareTag(damageSource, damageTypes)) {
            TakeDamage(damage);
            OnRaycastHit.Invoke(damageSource, hit);
        }
    }

    //-------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Returns true if a game object's Tag is of any of the tags that corresponding to the types in a list
    /// </summary>
    /// <param name="gameObject"></param>
    /// <param name="types">A list of DamageTypes</param>
    /// <returns></returns>
    public static bool CompareTag(GameObject gameObject, DamageType[] types) {
        for (int i = 0; i < types.Length; i++) {
            if (gameObject.CompareTag(damageTags[(int)types[i]]))
                return true;
        }
        return false;
    }

    //-------------------------------------------------------------------------------------------------------------------------
    [Server]
    public virtual void Die() {
        health = 0;
        m_attackerNum = 0;
        if (!isClient) {
            OnDie.Invoke();
        }
        RpcDie();
    }

    //-------------------------------------------------------------------------------------------------------------------------
    public virtual void Destroy(float seconds = 0) {
        if (seconds > 0) {
            StartCoroutine(DelayedDestroy(seconds));
        } else {
            Destroy();
        }

    }

    //-------------------------------------------------------------------------------------------------------------------------
    protected virtual IEnumerator DelayedDestroy(float seconds) {
        yield return new WaitForSeconds(seconds);

        Destroy();
    }

    //-------------------------------------------------------------------------------------------------------------------------
    public virtual void Destroy() {
        //Debug.Log("Invoking Destroy: " + this.name);
        OnDestroyed.Invoke();

        if (isServer) {
            RpcDestroy();
            //NetworkServer.Destroy(gameObject);
        }

        Destroy(gameObject);
    }

    //-------------------------------------------------------------------------------------------------------------------------
    [ClientRpc]
    public virtual void RpcTakeDamage(int damage) {
        if (!isServer)
            OnTakeDamage.Invoke();
    }

    //-------------------------------------------------------------------------------------------------------------------------
    [Server]
    protected virtual void EvaluateDamage(int damage) {
        recentDamageTaken += damage;

        if (profile.damageEffects != null && profile.damageEffects.Length > 0) {
            foreach (var effect in profile.damageEffects.OrderBy(e => e.damageThreshold)) {
                if (damage >= effect.damageThreshold) {
                    effect.OnEffectTriggered.Invoke();
                }
            }
        }

        if (profile.damageStates != null && profile.damageStates.Length > 0) {
            foreach (var state in profile.damageStates.OrderByDescending(s => s.damageThreshold)) {
                // Stops after finding the highest damage threshold state that exceeds recent damage
                if (state.damageThreshold <= recentDamageTaken) {
                    if (currentDamageState != state) {
                        if (currentDamageState.damageThreshold < state.damageThreshold) {
                            StopAllCoroutines();
                            state.TriggerEffect(this);
                            currentDamageState = state;
                        }
                    } else {
                        // Refresh state's elapsed
                        currentDamageState.InitEffect();
                    }

                    break;
                }
            }
        }
    }

    //-------------------------------------------------------------------------------------------------------------------------
    [ClientRpc]
    public void RpcTriggerDamageState(int effectIndex) {

    }

    //-------------------------------------------------------------------------------------------------------------------------
    public void EndCurrentState() {
        if (isServer) {
            if (!isClient)
                EndCurrentServerState();

            RpcEndCurrentState();
        } else if (isClient) {
            //EndCurrentClientState();
            CmdEndCurrentState();
        }
    }

    //-------------------------------------------------------------------------------------------------------------------------
    [Client]
    public void EndCurrentClientState() {
        currentDamageState.EndEffect();
    }

    //-------------------------------------------------------------------------------------------------------------------------
    [Server]
    public void EndCurrentServerState() {
        currentDamageState.EndEffect();
    }

    //-------------------------------------------------------------------------------------------------------------------------
    [Command]
    public void CmdEndCurrentState() {
        EndCurrentServerState();
    }

    //-------------------------------------------------------------------------------------------------------------------------
    [ClientRpc]
    public void RpcEndCurrentState() {
        EndCurrentClientState();
    }

    //-------------------------------------------------------------------------------------------------------------------------
    [ClientRpc]
    public virtual void RpcDie() {
        OnDie.Invoke();
        if (boomComponent) {
            boomComponent.GoBoom();
        }
    }

    //-------------------------------------------------------------------------------------------------------------------------
    [ClientRpc]
    public virtual void RpcDestroy() {
        if (!isServer) {
            OnDestroyed.Invoke();
            Destroy(gameObject);
        }
    }

    //-------------------------------------------------------------------------------------------------------------------------
    public interface IEnemyTargetable {
        Transform GetHeadTransform();
    }

    //-------------------------------------------------------------------------------------------------------------------------
    public interface IPlayerTargetable { }

    //-------------------------------------------------------------------------------------------------------------------------
    //public class DamageableHit : UnityEvent<GameObject, Collision> { }

    //-------------------------------------------------------------------------------------------------------------------------
    public class DamageableRaycastHit : UnityEvent<GameObject, RaycastHit> { }

    //-------------------------------------------------------------------------------------------------------------------------
    public class DamageableHit : UnityEvent<GameObject, Collision, bool, Vector3> { }
}
