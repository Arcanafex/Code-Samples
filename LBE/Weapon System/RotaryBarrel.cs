using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotaryBarrel : MonoBehaviour
{
    public enum SpinMode
    {
        Stopped,
        SpinUp,
        AtSpeed,
        SpinDown
    }

    public Transform rotaryPivot;
    public float angleDeltaPerShot;
    public float rotationSpeedMultiplier = 1;
    public float spinUpTime;
    public float spinDownTime;

    public SpinMode state;

    protected GunSystem gunSystem;
    protected bool lastFireState;
    protected float rotationIncrement;

    protected float startingRotationRate;
    protected float currentRotationRate;
    protected float elapsedTime;


    private void Start()
    {
        gunSystem = GetComponent<GunSystem>();
        lastFireState = gunSystem.TriggerDown;
        rotationIncrement = angleDeltaPerShot / gunSystem.FireRate;

        //var AITargeting = GetComponent<AITargetingBehaviour>();

        //if (AITargeting)
        //{
        //    AITargeting.OnFiring.AddListener(Firing);
        //}
        //else
        //{

        //}

        currentRotationRate = state == SpinMode.AtSpeed ? rotationSpeedMultiplier : 0;
    }

    private void Update()
    {
        if (gunSystem.TriggerDown != lastFireState)
        {
            if (gunSystem.TriggerDown)
            {
                SpinUp();
            }
            else
            {
                SpinDown();
            }

            lastFireState = gunSystem.TriggerDown;
        }

        if (state == SpinMode.SpinUp)
        {
            if (elapsedTime < spinUpTime)
            {
                elapsedTime += Time.deltaTime;
                currentRotationRate = Mathf.Lerp(startingRotationRate, rotationSpeedMultiplier, elapsedTime / spinUpTime);
            }
            else
            {
                currentRotationRate = rotationSpeedMultiplier;
                state = SpinMode.AtSpeed;
            }
        }
        else if (state == SpinMode.SpinDown)
        {
            if (elapsedTime < spinDownTime)
            {
                elapsedTime += Time.deltaTime;
                currentRotationRate = Mathf.Lerp(startingRotationRate, 0, elapsedTime / spinDownTime);
            }
            else
            {
                currentRotationRate = 0;
                state = SpinMode.Stopped;
            }
        }

        rotaryPivot.Rotate(Vector3.forward, rotationIncrement * Time.deltaTime * currentRotationRate, Space.Self);

        //if (Input.GetKeyDown(KeyCode.Y))
        //    SpinUp();

        //if (Input.GetKeyDown(KeyCode.U))
        //    SpinDown();
    }

    public void SpinUp()
    {
        elapsedTime = currentRotationRate > 0 ? (currentRotationRate / rotationSpeedMultiplier) * spinUpTime : 0;
        startingRotationRate = currentRotationRate;
        state = SpinMode.SpinUp;
    }

    public void SpinDown()
    {
        elapsedTime = currentRotationRate < rotationSpeedMultiplier ? (currentRotationRate / rotationSpeedMultiplier) * spinDownTime : 0;
        startingRotationRate = currentRotationRate;
        state = SpinMode.SpinDown;
    }

    //public void Firing(int shots)
    //{
    //    if (shots > 0)
    //    {
    //        SpinUp();
    //    }
    //    else
    //    {
    //        SpinDown();
    //    }
    //}
}