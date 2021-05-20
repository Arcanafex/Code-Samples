using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

[RequireComponent(typeof(Rigidbody))]
public class Rocket : MonoBehaviour
{
    public RocketEffect rocketEffect;

    private void Start()
    {
        rocketEffect.Initialize(gameObject, rocketEffect.m_profile);
    }

    public void Ignite()
    {
        rocketEffect.Ignite();
    }
}

