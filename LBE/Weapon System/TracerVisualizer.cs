using UnityEngine;
using System.Collections;

public class TracerVisualizer : MonoBehaviour
{
    public GameObject prefab;
    //public LineRenderer lineRenderer;
    public float duration = 1;
    public float defaultLength = 20;
    public GunSystem gun;

    private float elapsedTime;
    private float showingTracer;

    private void Start()
    {
        if (!gun)
            gun = GetComponentInParent<GunSystem>();

        gun.OnProjectileRaycast.AddListener(ShowTracer);
    }

    public LineRenderer GetNewLine()
    {
        var newLine = Instantiate(prefab);
        var lineRenderer = newLine.GetComponent<LineRenderer>();

        lineRenderer.useWorldSpace = true;
        lineRenderer.positionCount = 2;

        return lineRenderer;
    }

    public void ShowTracer(Projectile bullet, RaycastHit hit)
    {
        var lineRenderer = GetNewLine();
        lineRenderer.SetPosition(0, gun.ProjectileSpawnPoint.position);

        if (hit.collider)
        {
            lineRenderer.SetPosition(1, hit.point);
        }
        else
        {
            lineRenderer.SetPosition(1, gun.ProjectileSpawnPoint.position + (gun.ProjectileSpawnPoint.forward * defaultLength));
        }

        StartCoroutine(Disappear(lineRenderer));
    }

    private IEnumerator Disappear(LineRenderer lineRenderer)
    {
        Vector3 start = lineRenderer.GetPosition(0);
        Vector3 end = lineRenderer.GetPosition(1);
        float elapsed = 0;

        while(elapsed < duration)
        {
            Vector3 tailPoint = Vector3.Lerp(start, end, elapsed / duration);

            lineRenderer.SetPosition(1, tailPoint);

            elapsed += Time.deltaTime;

            yield return null;
        }

        Destroy(lineRenderer.gameObject);
    }
}
