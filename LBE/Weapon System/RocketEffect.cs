using UnityEngine;
using UnityEngine.Events;
using System.Collections;

[System.Serializable]
public class RocketEffectProfile
{
    public float ignitionDelay;
    public float thrust;
    public float burnTime;
}

[System.Serializable]
public class RocketEffect
{
    public RocketEffectProfile m_profile;

    public float Thrust { get; set; }
    public float IgnitionDelay { get; set; }
    public float BurnTime { get; set; }
    public bool Ignited { get; set; }

    public UnityEvent OnIgnition;

    public RocketEffect()
    {
        OnIgnition = new UnityEvent();
    }

    public void Initialize(GameObject rocket, RocketEffectProfile profile)
    {
        m_profile = profile;
        Thrust = m_profile.thrust;
        IgnitionDelay = m_profile.ignitionDelay;
        BurnTime = m_profile.burnTime;
    }

    public virtual void Ignite()
    {
        Ignited = true;
        OnIgnition.Invoke();
    }

    public virtual IEnumerator UpdateRocket(Rigidbody rocketBody)
    {
        while (BurnTime > 0)
        {
            if (!Ignited)
            {
                while (IgnitionDelay > Time.deltaTime)
                {
                    IgnitionDelay -= Time.deltaTime;
                    yield return null;
                }

                Ignite();
            }
            else
            {
                BurnTime -= Time.deltaTime;
                rocketBody.AddRelativeForce(Vector3.forward * Thrust, ForceMode.Acceleration);
                yield return null;
            }
        }
    }
}
