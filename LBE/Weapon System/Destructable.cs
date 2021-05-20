using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using System.Collections;
using System.Linq;

//*****************************************************************************************************************************
//*****************************************************************************************************************************
public class Destructable : Damageable {

    //*****************************************************************************************************************************
    [System.Serializable]
    public class DestructiblePiece {
        protected internal static Transform s_fragments;
        protected Transform m_startingParent;

        public string name;
        public int health = 1;
        public Collider[] colliders;

        public GameObject[] DeactivateOnDestroyed;
        public GameObject[] ActivateOnDestroyed;
        public Joint[] jointBreaks;
        public Rigidbody[] ejectedBodies;
        public float fragmentLifetime = 5;

        public bool useExplosion;
        public GameObject explosionOrigin;
        public ExplosiveEffect explosiveEffect;

        public UnityEvent OnDamaged;
        public UnityEvent OnDestroyed;

        //---------------------------------------------------------------------------------------------------------------------
        //---------------------------------------------------------------------------------------------------------------------
        public void SetDestroyed() {
            if (!s_fragments) {
                var fragments = new GameObject("Fragments");
                s_fragments = fragments.transform;
            }

            foreach (var activate in ActivateOnDestroyed) {
                if (activate && !activate.activeInHierarchy) {
                    activate.SetActive(true);
                }
            }

            foreach (var deactivate in DeactivateOnDestroyed) {
                if (deactivate && deactivate.activeInHierarchy) {
                    deactivate.SetActive(false);
                }
            }

            foreach (var joint in jointBreaks) {
                if (joint) {
                    Destroy(joint);
                }
            }

            foreach (var rbody in ejectedBodies) {
                if (!rbody)
                    continue;

                rbody.constraints = RigidbodyConstraints.None;

                m_startingParent = rbody.transform.parent;
                rbody.transform.SetParent(s_fragments);
                Destroy(rbody.gameObject, fragmentLifetime);
            }

            OnDestroyed.Invoke();

            if (useExplosion) {
                explosiveEffect.Initilize(explosionOrigin, explosiveEffect.m_profile);
                explosiveEffect.Detonate();
            }
        }

        //---------------------------------------------------------------------------------------------------------------------
        public void TakeDamage(int damage) {
            health -= damage;
            OnDamaged.Invoke();
        }
    }





    public DestructiblePiece[] pieces;
    private Transform Fragments;
    private RootMotion.FinalIK.RagdollUtility ragdoll;
    public Collider defaultCollider;

    //---------------------------------------------------------------------------------------------------------------------
    //---------------------------------------------------------------------------------------------------------------------
    [Server]
    public override void Hit(int damage, GameObject damageSource, Collision collision = null, bool useCollision = true, float hitPointx = 0, float hitPointy = 0, float hitPointz = 0) {
        ApplyDamage(damage, collision.collider);
        base.Hit(damage, damageSource, collision);
    }

    //---------------------------------------------------------------------------------------------------------------------
    [Server]
    public override void RaycastHit(int damage, GameObject damageSource, RaycastHit hit, Player damageOwner) {
        ApplyDamage(damage, hit.collider, damageOwner);
        base.RaycastHit(damage, damageSource, hit, damageOwner);
    }


    public override void Die() {
        ApplyDamage(int.MaxValue, defaultCollider);
        base.Die();
    }

    //---------------------------------------------------------------------------------------------------------------------
    [Server]
    public override void TakeDamage(int damage, Collider collider) {
        ApplyDamage(damage, collider);
        base.TakeDamage(damage, collider);
    }

    //---------------------------------------------------------------------------------------------------------------------
    [Server]
    protected void ApplyDamage(int damage, Collider hitCollider, Player damageOwner = null) {
        for (int i = 0; i < pieces.Length; i++) {
            //Debug.Log("[HIT] " + piece.name + " damage: " + damage);
            if (pieces[i].colliders.Contains(hitCollider)) {

                pieces[i].TakeDamage(damage);
                RpcDamagePiece(i, damage);

                if (pieces[i].health <= 0 ) {

                    if (pieces[i].explosiveEffect.m_profile.ragdollDuration > 0) { 
                        // for those right arm, left leg, right leg, head pieces
                        ApplyExplosion();
                        OnDie.Invoke();
                        if (damageOwner && isServer) {
                            Spaces.LBE.EventParam deathEventParams = new Spaces.LBE.EventParam();
                            Spaces.LBE.DebugLog.Log("gamestate", "ApplyDamage() registering kill from player id " + damageOwner.sPlrData.playerID);

                            deathEventParams.m_sParam = damageOwner.sPlrData.playerID;
                            Spaces.LBE.SpacesEventManager.TriggerEvent(Spaces.LBE.SpacesEventType.TerminatorKilled, deathEventParams);
                        }
                        
                        break;
                    } else {
                        //Debug.Log("[DESTROYED] " + piece.name);
                        pieces[i].SetDestroyed();
                        //UpdateRagdoll();
                        RpcDestroyPiece(i);
                    }
                }
            }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------
    [Server]
    protected void ApplyExplosion() {
        int damage = int.MaxValue;
        for (int i = 0; i < pieces.Length; i++) {

            // Randomly breaking pieces
            if (i < Random.Range(0, pieces.Length)) 
                continue;

            pieces[i].TakeDamage(damage);
            RpcDamagePiece(i, damage);

            if (pieces[i].health <= 0) {
                pieces[i].SetDestroyed();
                RpcDestroyPiece(i);
            }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------
    protected void UpdateRagdoll() {
        if (!ragdoll) {
            ragdoll = GetComponent<RootMotion.FinalIK.RagdollUtility>();
        }

        if (ragdoll) {
            ragdoll.UpdateRigidBones();
        }
    }

    //---------------------------------------------------------------------------------------------------------------------
    [ClientRpc]
    protected void RpcDamagePiece(int index, int damage) {
        if (!isServer && index < pieces.Length) {
            pieces[index].TakeDamage(damage);
        }
    }

    //---------------------------------------------------------------------------------------------------------------------
    [ClientRpc]
    protected void RpcDestroyPiece(int index) {
        if (!isServer && index < pieces.Length) {
            pieces[index].SetDestroyed();
            UpdateRagdoll();
        }
    }

    //---------------------------------------------------------------------------------------------------------------------
    private void OnDrawGizmosSelected() {
        foreach (var piece in pieces) {
            if (piece.useExplosion && piece.explosionOrigin) {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(piece.explosionOrigin.transform.position + piece.explosiveEffect.m_profile.effectPositionOffset, piece.explosiveEffect.m_profile.effectRadius);
            }
        }
    }
    //---------------------------------------------------------------------------------------------------------------------
}
