using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollJoint : MonoBehaviour
{
    public GameObject[] ActivateOnBreak;
    public GameObject[] DeactivateOnBreak;

    public bool useKinematics;

    private Joint m_joint;
    private Transform m_startingParent;

    private bool breakRequested = false;


    private void Start()
    {
        m_joint = GetComponent<Joint>();
        m_startingParent = transform.parent;

        breakRequested = false;
        Swap(false);
    }

    public void EnableRagdoll()
    {
        //if (transform.parent != transform.root)
        //{
        //    transform.SetParent(transform.root);
        //}

        //Swap(true);
        BreakJoint();

        //foreach (var body in GetComponentsInChildren<Rigidbody>())
        //{
        //    body.isKinematic = false;
        //}
    }

    private void OnJointBreak(float breakForce)
    {        
        Debug.Log(this.name + " broke with foece " + breakForce);

        BreakJoint();
    }

    public void Swap(bool broken)
    {
        foreach (var obj in DeactivateOnBreak)
        {
            obj.SetActive(!broken);
        }

        foreach (var obj in ActivateOnBreak)
        {
            obj.SetActive(broken);
        }
    }

    public void BreakJoint()
    {
        Swap(true);
        Destroy(m_joint);

        transform.SetParent(null);

        //if (transform.parent != transform.root)
        //{
        //    transform.SetParent(transform.root);
        //}

        foreach (var body in GetComponentsInChildren<Rigidbody>())
        {
            if (useKinematics)
            {
                body.isKinematic = false;
            }
            else
            {
                body.constraints = RigidbodyConstraints.None;
            }

        }

    }

}