using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Collections;

//*****************************************************************************************************************************
//*****************************************************************************************************************************
[System.Serializable]
public class ExplosiveEffect {

    public ExplosiveEffectProfile m_profile;
    private Transform m_effectOrigin { get; set; }

    public Vector3 EffectPositionOffset { get; set; }
    public Vector3 EffectRotation { get; set; }
    public float EffectRadius { get; set; }
    public float EffectForce { get; set; }
    public int BaseDamage { get; set; }
    public AnimationCurve Falloff { get; set; }
    public LayerMask AffectedLayers { get; set; }
    public bool LimitDamageablesToOneHit { get; set; }
    public bool ObjectsShieldBlast { get; set; }

    public GameObject shrapnelPrefab;
    public float shrapnelDuration;
    public UnityEvent OnDetonate;

    //-------------------------------------------------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------------------------------------------------
    public ExplosiveEffect() {
        OnDetonate = new UnityEvent();
    }

    //-------------------------------------------------------------------------------------------------------------------------
    public void Initilize(GameObject explosive, ExplosiveEffectProfile profile) {
        m_profile = profile;
        m_effectOrigin = explosive.transform;

        EffectPositionOffset = m_profile.effectPositionOffset;
        EffectRadius = m_profile.effectRadius;
        EffectForce = m_profile.effectForce;
        BaseDamage = m_profile.baseDamage;
        Falloff = m_profile.falloff;
        AffectedLayers = m_profile.affectedLayers;
        LimitDamageablesToOneHit = m_profile.limitDamageablesToOneHit;
        ObjectsShieldBlast = m_profile.objectsShieldBlast;
    }

    //-------------------------------------------------------------------------------------------------------------------------
    // TODO: integrate damage filters into explosive effect attributes
    public virtual void Detonate() {
        OnDetonate.Invoke();

        if (shrapnelPrefab) {
            foreach (var collider in m_effectOrigin.GetComponentsInChildren<Collider>()) {
                collider.isTrigger = true;
            }

            var shrapnel = GameObject.Instantiate(shrapnelPrefab, m_effectOrigin.position, m_effectOrigin.rotation);
            shrapnel.SetActive(true);
            GameObject.Destroy(shrapnel, shrapnelDuration);
        }

        //TODO: implement shielding of blast effects
        Collider[] hitColliders = Physics.OverlapSphere(m_effectOrigin.position + EffectPositionOffset, EffectRadius, AffectedLayers, QueryTriggerInteraction.UseGlobal);
        var hitDamageables = new List<Damageable>();
        //var bodies = new List<Rigidbody>();

        //Debug.Log("[BOOM HIT]");
        RootMotion.FinalIK.RagdollUtility ragdoll = null;

        foreach (var hitCollider in hitColliders) {
            //Debug.Log("[BOOM HIT] " + hit.name);

            var damageable = hitCollider.GetComponentInParent<Damageable>();

            if (damageable) {
                if (damageable.isServer) {
                    bool firstHit = !hitDamageables.Contains(damageable);

                    if (firstHit) {
                        hitDamageables.Add(damageable);
                    }

                    if (!LimitDamageablesToOneHit || firstHit) {
                        // TODO: more intelligently select the root collider to apply damage effect to
                        var effectRange = Vector3.Distance(hitCollider.transform.position, m_effectOrigin.position) / EffectRadius;
                        int damage = Mathf.RoundToInt(BaseDamage * Falloff.Evaluate(effectRange));

                        if (damage > 0)
                            damageable.TakeDamage(damage, hitCollider);
                    }
                }

                // FENG_TODO: not using ragdoll for now based on design; will see if we will enable it in the future
                bool using_Ragdoll = false;
                if (using_Ragdoll) {
                    if (!ragdoll) {
                        ragdoll = damageable.GetComponent<RootMotion.FinalIK.RagdollUtility>();
                    }

                    if (ragdoll && !ragdoll.isRagdoll) {
                        if (m_profile.ragdollDuration > 0) {
                            damageable.StartCoroutine(TempRagdoll(damageable, ragdoll, m_profile.ragdollDuration));
                        } else if (m_profile.ragdollDuration < 0) {
                            ragdoll.EnableRagdoll();
                        }
                    }
                }
            }

            // TODO: figure out best way optionally limit force to apply just to root body.
            Rigidbody rb = hitCollider.GetComponentInParent<Rigidbody>();
            if (rb != null) {

                rb.AddExplosionForce(EffectForce, m_effectOrigin.position + EffectPositionOffset + Random.onUnitSphere, 2*EffectRadius, 3.0f);
                //rb.AddForce(EffectForce,, m_effectOrigin.position + EffectPositionOffset, 2 * EffectRadius, 1.0f);
            }
        }
    }

    //-------------------------------------------------------------------------------------------------------------------------
    private IEnumerator TempRagdoll(Damageable damageable, RootMotion.FinalIK.RagdollUtility ragdoll, float duration) {
        ragdoll.EnableRagdoll();

        yield return new WaitForSeconds(duration);

        if (damageable.Alive) {
            ragdoll.DisableRagdoll();

            //FENG_TODO: will remove this if we would like to add crawling animations. Let's kill it for the time-being 
            damageable.OnDie.Invoke();
        }
    }
    //-------------------------------------------------------------------------------------------------------------------------
}
