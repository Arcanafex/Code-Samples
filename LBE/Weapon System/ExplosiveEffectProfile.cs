using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Collections;


[System.Serializable]
public class ExplosiveEffectProfile
{
    public Vector3 effectPositionOffset;
    public float effectRadius;
    public float effectForce;

    public int baseDamage;
    public AnimationCurve falloff;
    public bool limitDamageablesToOneHit;
    public bool objectsShieldBlast;
    public float ragdollDuration;

    public LayerMask affectedLayers = -1;
}
