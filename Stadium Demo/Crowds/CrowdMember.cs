using UnityEngine;

namespace TMPC
{
    [CreateAssetMenu(fileName = "CrowdMember", menuName = "Crowd Member")]
    public class CrowdMember : ScriptableObject
    {
        public GameObject ModelPrefab;

        [Header("Position Variation")]
        public FloatRange XPosVariation;
        public FloatRange YPosVariation;
        public FloatRange ZPosVariation;

        [Header("Rotiation Variation")]
        public FloatRange XRotVariation;
        public FloatRange YRotVariation;
        public FloatRange ZRotVariation;

        public FloatRange ScaleVariationRange = FloatRange.one;
        public FloatRange AnimationSpeedMultiplierRange = FloatRange.one; 
        [Range(0f, 1f)]
        public float AnimationCycleOffsetRange;

        public Material[] MaterialOptions;

        public GameObject Spawn(Transform parent, Vector3 position)
        {
            var model = Instantiate(ModelPrefab, parent);
            model.transform.position = position;
            model.transform.localPosition += PosVariation();
            model.transform.localRotation *= RotVariation();
            model.transform.localScale *= Random.Range(ScaleVariationRange.Min, ScaleVariationRange.Max);

            var animator = model.GetComponent<Animator>();
            animator.SetFloat("Offset", Random.Range(0, AnimationCycleOffsetRange));
            animator.SetFloat("Speed", Random.Range(AnimationSpeedMultiplierRange.Min, AnimationSpeedMultiplierRange.Max));

            // Non-functional attempt to introduce offset on exit to transition
            //var animatorController = animator.runtimeAnimatorController as AnimatorController;
            //var idleState = animatorController.layers[0].stateMachine.states.FirstOrDefault(s => s.state.name == "Sitting Idle");
            //foreach (var transition in idleState.state.transitions)
            //{
            //    transition.hasExitTime = true;
            //    transition.exitTime = offset;
            //}

            if (MaterialOptions.Length > 0)
            {
                foreach (var renderer in model.GetComponentsInChildren<Renderer>())
                {
                    renderer.sharedMaterial = MaterialOptions[Random.Range(0, MaterialOptions.Length)];
                }
            }

            return model;
        }

        public Vector3 PosVariation()
        {
            return new Vector3(
                Random.Range(XPosVariation.Min, XPosVariation.Max),
                Random.Range(YPosVariation.Min, YPosVariation.Max),
                Random.Range(ZPosVariation.Min, ZPosVariation.Max)
                );
        }

        public Quaternion RotVariation()
        {
            return Quaternion.Euler(
                Random.Range(XRotVariation.Min, XRotVariation.Max),
                Random.Range(YRotVariation.Min, YRotVariation.Max),
                Random.Range(ZRotVariation.Min, ZRotVariation.Max)
                );
        }
    }

    [System.Serializable]
    public struct FloatRange
    {
        public FloatRange(float min, float max)
        {
            Min = min;
            Max = max;
        }

        public static FloatRange zero => new FloatRange(0, 0);
        public static FloatRange one => new FloatRange(1, 1);

        public float Min;
        public float Max;
    }
}
