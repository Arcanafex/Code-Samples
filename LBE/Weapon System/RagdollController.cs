using UnityEngine;
using System.Collections.Generic;

public class RagdollController : MonoBehaviour
{
    public Animator m_animator;
    public bool animOn;
    public bool useGravity;
    public bool isKinematic;
    public bool freezePosition;
    public bool freezeRotation;

    public float jointBreakForce;
    public bool infiniteBreakForce;

    public float jointBreakTorque;
    public bool infiniteBreakTorque;


    private List<Rigidbody> m_bodies;
    private List<Joint> m_Joints;

    private void Start()
    {
        m_bodies = new List<Rigidbody>(GetComponentsInChildren<Rigidbody>());
        m_Joints = new List<Joint>(GetComponentsInChildren<Joint>());
    }

    private void Update()
    {
        m_animator.enabled = animOn;

        m_bodies.ForEach(b => b.useGravity = useGravity);
        m_bodies.ForEach(b => b.isKinematic = isKinematic);
        m_bodies.ForEach(b => b.freezeRotation = freezeRotation);
        m_bodies.ForEach(b => b.constraints = freezeRotation ? RigidbodyConstraints.FreezeAll : RigidbodyConstraints.None);

        m_Joints.ForEach(j => j.breakForce = infiniteBreakForce ? Mathf.Infinity : jointBreakForce);
        m_Joints.ForEach(j => j.breakTorque = infiniteBreakTorque ? Mathf.Infinity : jointBreakTorque);
    }
}
