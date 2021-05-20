using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;

public class Explosive : MonoBehaviour
{
    public ExplosiveEffect explosiveEffect;

    private void Start()
    {
        explosiveEffect.Initilize(gameObject, explosiveEffect.m_profile);
    }

    public void Explode()
    {
        explosiveEffect.Detonate();
    }
}
