using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Spaces.UnityClient
{
    public abstract class CasterVisualizer : ScriptableObject
    {
        public enum BeamState
        {
            Off,
            Default,
            NearTarget,
            OnTarget,
            OnInteractableTarget
        }

        public BeamState State { get; private set; }
        public abstract GameObject[] GetVisualizerGameObjects();

        public virtual void GenerateBeam(SpaceCaster caster) { }
        public virtual IEnumerator UpdateBeam(SpaceCaster caster, List<UnityEngine.EventSystems.RaycastResult> beamHits)
        {
            while (caster.casterActive)
                yield return null;
        }

        public virtual void Off(SpaceCaster caster)
        {
            State = BeamState.Off;
        }

        public virtual void Default(SpaceCaster caster)
        {
            State = BeamState.Default;
        }

        public virtual void NearTarget(SpaceCaster caster)
        {
            State = BeamState.NearTarget;
        }

        public virtual void OnTarget(SpaceCaster caster)
        {
            State = BeamState.OnTarget;
        }

        public virtual void OnInteractableTarget(SpaceCaster caster)
        {
            State = BeamState.OnInteractableTarget;
        }

    }
}