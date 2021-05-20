using UnityEngine;
using System.IO;

[CreateAssetMenu(menuName = "Gun Profile")]
public class GunProfile : ConfigurableProfile
{
    public float firingForce = 10;
    public float firingRate = 0.02f;

    public ProjectileSystem.FiringMode firingMode = ProjectileSystem.FiringMode.Single;
    public ProjectileSystem.ProjectileSpawnPointProgression progression;

    public int burstSize = 3;
    public int magazineSize = -1;
    public int reloadClipSize = 0;

    public float warmUpTime;
    public float overheatTime;
    public float cooldownTime;

    //private void OnEnable()
    //{
    //    Load();
    //}

}
