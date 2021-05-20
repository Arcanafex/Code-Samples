using UnityEngine;
using UnityEngine.Events;
using System.Collections;

[CreateAssetMenu(menuName = "Damageable Profile")]
public class DamageableProfile : ConfigurableProfile
{
    public int health = 1;
    public float recoveryRate = 0;
    public int death = 0;

    public DamageEffectState[] damageStates;

    public DamageEffect[] damageEffects;

    [System.Serializable]
    public class DamageEffect
    {
        public int damageThreshold;
        public UnityEvent OnEffectTriggered;
    }

    [System.Serializable]
    public class DamageEffectState
    {
        public int damageThreshold;
        public float effectDuration;
        public float effectUpdateFrequency;

        public UnityEvent OnEffectStart;
        public UnityEvent OnEffectUpdate;
        public UnityEvent OnEffectDurationDone;

        public float elapsedTime { get; protected set; }
        public bool effectInterrupted { get; protected set; }

        public float progress
        {
            get { return elapsedTime / effectDuration; }
        }

        public bool done
        {
            get { return elapsedTime < effectDuration; }
        }

        public void InitEffect()
        {
            elapsedTime = 0;
            effectInterrupted = false;
            OnEffectStart.Invoke();
        }

        public void TriggerEffect(Damageable target)
        {
            InitEffect();
            target.StartCoroutine(StartDamageEffectState());
        }

        public void EndEffect()
        {
            effectInterrupted = true;
        }

        protected IEnumerator StartDamageEffectState()
        {
            while (elapsedTime < effectDuration && !effectInterrupted)
            {
                elapsedTime += effectUpdateFrequency;
                yield return new WaitForSeconds(effectUpdateFrequency);

                OnEffectUpdate.Invoke();
            }

            OnEffectDurationDone.Invoke();
        }

    }
}
