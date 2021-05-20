using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class GrabTesting : NetworkBehaviour
{
    public PlayerHand hand;

    public GameObject prefab;
    public GameObject grabObject;
    public GunSystem gun;

    [SyncVar]
    public bool triggerDown;

    private void Update()
    {
        if (isClient)
        {
            if (Input.GetKeyDown(KeyCode.G))
            {
                Debug.Log("Grab Command Called");
                CmdGrab();
            }

            if (Input.GetKeyDown(KeyCode.U))
            {
                Debug.Log("Ungrab Command Called");
                CmdUngrab();
            }

            if (gun)
            {
                gun.TriggerDown = Input.GetKey(KeyCode.Space);
                CmdTrigger(gun.TriggerDown);
            }
        }

        //if (isServer)
        //{
        //    if (Input.GetKeyDown(KeyCode.S))
        //    {
        //        Debug.Log("Spawn Gun Client");
        //        grabObject = Instantiate(prefab, Vector3.up, Quaternion.identity);
        //        NetworkServer.Spawn(grabObject);
        //    }
        //}
    }

    [Command]
    private void CmdTrigger(bool down)
    {
        if (gun)
        {
            gun.TriggerDown = down;
            //Debug.Log("Gun Trigger: " + gun.TriggerDown);
        }

    }

    //[Command]
    //private void CmdGiveAuthority()
    //{
    //    Debug.Log("Authority requested for gun");
    //    if (gun)
    //    {
    //        Debug.Log("Authority requested for gun");

    //    }
    //}

    //[Command]
    //private void CmdRemoveAuthority()
    //{
    //    if (gun)
    //    {
    //        gun.GetComponent<NetworkIdentity>().RemoveClientAuthority(connectionToServer);
    //    }
    //}


    [Command]
    private void CmdGrab()
    {
        if (!hand)
            hand = FindObjectOfType<PlayerHand>();

        if (hand)
        {
            Debug.Log("CMD Grab: " + hand.hand.ToString());

            var guns = FindObjectsOfType<GunSystem>();

            Debug.Log("Guns: " + guns.Length);
            gun = guns.Length > 0 ? guns[Random.Range(0, guns.Length)] : null;
            GameObject gunObject = gun ? gun.gameObject : null;

            //var netIdent = hand.grabbedBody.GetComponent<NetworkIdentity>();
            //netIdent.AssignClientAuthority(connectionToClient);
            //netIdent.localPlayerAuthority = true;

            var grabbable = gunObject.GetComponent<GrabbableObject>();
            grabbable.SnapTo(hand, false);

            hand.Grab(gunObject);
            RpcGrab(gunObject);
        }
    }

    [ClientRpc]
    private void RpcGrab(GameObject grabbedThing)
    {
        if (!hand)
            hand = FindObjectOfType<PlayerHand>();

        if (hand)
        {
            grabObject = grabbedThing;

            //var netIdent = grabObject.GetComponent<NetworkIdentity>();
            //netIdent.localPlayerAuthority = true;

            var grabbable = grabObject.GetComponent<GrabbableObject>();
            grabbable.SnapTo(hand, true);

            Debug.Log("RPC Grab: " + hand.hand.ToString());
            hand.Grab(grabObject);

            if (grabObject && (gun = grabObject.GetComponentInChildren<GunSystem>()))
                Debug.Log("Grabbed a gun!");
        }
    }

    [Command]
    private void CmdUngrab()
    {
        if (!hand)
            hand = FindObjectOfType<PlayerHand>();

        if (hand)
        {
            Debug.Log("CMD Ungrab: " + hand.hand.ToString());

            //var netIdent = hand.grabbedBody.GetComponent<NetworkIdentity>();
            //netIdent.RemoveClientAuthority(connectionToClient);
            //netIdent.localPlayerAuthority = false;
            if (hand.grabbedBody)
                hand.grabbedBody.WakeUp();

            hand.Ungrab();
            RpcUngrab();
        }
    }

    [ClientRpc]
    private void RpcUngrab()
    {
        if (!hand)
            hand = FindObjectOfType<PlayerHand>();

        if (hand)
        {
            //var netIdent = grabObject.GetComponent<NetworkIdentity>();
            //netIdent.localPlayerAuthority = false;
            Debug.Log("RPC Ungrab: " + hand.hand.ToString());
            hand.Ungrab();
        }
    }
}
