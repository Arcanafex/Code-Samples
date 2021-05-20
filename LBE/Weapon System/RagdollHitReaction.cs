using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollHitReaction : MonoBehaviour
{
    public float recoveryDelay;
    private Rigidbody m_body;

    private void Start()
    {
        m_body = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!enabled)
            return;

        m_body.isKinematic = false;
        StartCoroutine(Recovery());
    }

    private IEnumerator Recovery()
    {
        yield return new WaitForSeconds(recoveryDelay);

        m_body.isKinematic = true;
    }

}