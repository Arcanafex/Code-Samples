using UnityEngine;

public class Bullet : MonoBehaviour
{
	public NetworkSpawnPool bulletPool;

	void Start()
	{
		bulletPool = NetworkSpawnPool.GetPoolByName("DynamicPool");
	}

	void OnCollisionEnter(Collision collision)
	{
		//var hit = collision.gameObject;
		//var hitCombat = hit.GetComponent<Combat>();
		//if (hitCombat != null)
		//{
		//	hitCombat.TakeDamage(10);

		//	bulletPool.ServerReturnToPool(gameObject);
		//	//Destroy(gameObject);
		//}
	}
}
