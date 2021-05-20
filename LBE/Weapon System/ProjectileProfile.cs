using UnityEngine;
using System.IO;

[CreateAssetMenu(menuName = "Projectile Profile")]
public class ProjectileProfile : ConfigurableProfile
{
    public bool isRocket;
    public bool isExplosive;

    public float projectileLifetime = 3;
    public int damage;
    public bool destroyedOnImpact;
    public float speed = 100.0f;

    public RocketEffectProfile rocketProfile;
    public ExplosiveEffectProfile explosiveProfile;

    public string SoundGroupName = "";
}