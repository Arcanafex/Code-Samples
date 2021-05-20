using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class BeamSystem : NetworkBehaviour
{
    public Transform beamSpawnPoint;

    public UnityEvent OnBeamCast;
    public BeamStrike OnBeamStrike;
    public UnityEvent OnBeamOverheat;
    public UnityEvent OnBeamOff;

    private bool beamOn;

    protected virtual void Awake()
    {
        if (OnBeamStrike == null)
            OnBeamStrike = new BeamStrike();
    }

    protected virtual void ProjectBeam()
    {
        var ray = new Ray(beamSpawnPoint.position, beamSpawnPoint.forward);

        var beamHits = Physics.RaycastAll(ray);

        if (OnBeamStrike != null)
            OnBeamStrike.Invoke(beamHits);
    }

}

public class BeamStrike : UnityEvent<RaycastHit[]>
{
}
