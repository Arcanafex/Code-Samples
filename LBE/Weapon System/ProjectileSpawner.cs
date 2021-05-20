using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileSpawner : MonoBehaviour {

    public float firingForce = 10;
    public float maxFiringRate = 0.02f;
    public GameObject projectileObject;

    private float waitTime;

    private void Update()
    {
        if (waitTime > 0)
        {
            waitTime -= Time.deltaTime;
        }
    }

    public void SpawnProjectile()
    {
        if (waitTime > 0)
        {
            return;
        }
        else
        {
            var projectile = Instantiate(projectileObject, transform.position, transform.rotation);
            projectile.gameObject.SetActive(true);

            var bulletBody = projectile.GetComponent<Rigidbody>();

            if (bulletBody)
            {
                bulletBody.AddRelativeForce(Vector3.forward * firingForce, ForceMode.VelocityChange);
            }

            waitTime = maxFiringRate;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.2f);
    }

    private void OnDrawGizmosSelected()
    {
        var ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        Gizmos.color = Color.red;

        if (Physics.Raycast(ray, out hit, 100))
        {
            Gizmos.DrawLine(transform.position, hit.point);
            Gizmos.DrawSphere(hit.point, 0.2f);
        }
        else
        {
            Gizmos.DrawRay(transform.position, transform.forward);
        }
    }
}
