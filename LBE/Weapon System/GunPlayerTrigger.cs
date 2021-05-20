using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunPlayerTrigger : MonoBehaviour
{
    const string RIGHT_HAND_TAG = "rhand";
    const string LEFT_HAND_TAG = "lhand";
    public GunSystem m_attachedGun;
    protected Collider lastCollider;

    protected float updateFreq = 0.5f;
    protected float elapsedTime;

    private void OnTriggerStay(Collider other)
    {
        elapsedTime += Time.deltaTime;

        if (elapsedTime > updateFreq)
        {
            // Only perform check if GunSystem isn't null and other collider is new
            if (m_attachedGun && other != lastCollider)
            {
                if (other.tag == LEFT_HAND_TAG || other.tag == RIGHT_HAND_TAG)
                {
                    // register player to gun
                    TrackedPlayerHand playerHandGO = other.gameObject.GetComponent<TrackedPlayerHand>();

                    if (playerHandGO
                        && playerHandGO.IsTracking
                        && playerHandGO.m_Owner
                        && playerHandGO.m_Owner.isLocalPlayer
                        && playerHandGO.m_Owner != m_attachedGun.m_Owner
                        )
                    {
                        lastCollider = other;
                        playerHandGO.m_Owner.LocallyRegisterGun(m_attachedGun);
                        Spaces.LBE.DebugLog.Log("networking", "Player " + playerHandGO.m_Owner.name + " registered to gun " + m_attachedGun.name);
                    }
                }
            }
        }
    }
}
