using UnityEngine;
using System.Collections;

public class LaserScope : MonoBehaviour
{
    public bool on = true;
    public Color color = Color.red;
    public int range = 100;
    private ProjectileSystem projectileSystem;


    private void OnDrawGizmos()
    {
        if (on)
        {
            if (!projectileSystem)
            {
                projectileSystem = GetComponent<ProjectileSystem>();
            }

            if (projectileSystem)
            {
                Gizmos.color = color;

                Gizmos.DrawLine(projectileSystem.ProjectileSpawnPoint.position, projectileSystem.ProjectileSpawnPoint.forward * range);
            }
        }
    }
}
