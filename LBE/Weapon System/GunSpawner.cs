using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GunSpawner : NetworkBehaviour
{
    public GameObject gunPrefab;

    // Update is called once per frame
    void Update()
    {
        if (isServer)
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                Debug.Log("Spawn Gun Client");
                var gun = Instantiate(gunPrefab, Vector3.up, Quaternion.identity);
                NetworkServer.Spawn(gun);
            }
        }
    }
}
