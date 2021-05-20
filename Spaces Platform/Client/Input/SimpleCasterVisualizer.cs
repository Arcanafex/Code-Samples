using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Spaces.UnityClient
{
    [CreateAssetMenu(menuName ="Space Casters/Simple Visualizer")]
    public class SimpleCasterVisualizer : CasterVisualizer
    {
        [System.Serializable]
        public class BeamStyle
        {
            public float minBeamLength = 0;
            public float maxBeamLength = float.MaxValue;
            public Vector3 pointSize;
            public Vector2 beamThickness;
            public Color beamColor;
            public Color pointColor;
        }

        public GameObject beamPointPrefab;
        public GameObject beamBodyPrefab;

        public Material baseBeamMaterial;
        public Material baseCanvasMaterial;

        private bool isTargetClicked;
         
        [Header("Beam Characteristics")]
        public BeamStyle normal;
        public BeamStyle onTarget;
        public BeamStyle onTargetClickable;
        public BeamStyle onTargetClicked;

        private GameObject beamPoint;
        private GameObject beamBody;

        private Material beamPointMaterial;
        private Material beamBodyMaterial;

        private Material beamPointCanvasMaterial;
        private Material beamBodyCanvasMaterial;

        private List<Renderer> beamPointRenderers;
        private List<Renderer> beamBodyRenderers;

        private BeamStyle currentStyle
        {
            get
            {
                if (State == BeamState.OnTarget)
                    return onTarget;
                else
                    return normal;
            }
        }

        private Vector3 lastParentScale;
        private Vector3 globalScaleFactor;

        public override void GenerateBeam(SpaceCaster caster)
        {
            // Beam Body setup

            if (!beamBody)
            {
                if (beamBodyPrefab)
                {
                    beamBody = Instantiate(beamBodyPrefab, caster.transform) as GameObject;
                }
                else
                {
                    beamBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    beamBody.transform.SetParent(caster.transform, false);
                    beamBody.layer = LayerMask.NameToLayer("Ignore Raycast");
                }

                beamBody.name = "BeamBody";
                beamBody.layer = 2;
            }

            beamBody.transform.localPosition = Vector3.zero;
            beamBody.transform.localRotation = Quaternion.Euler(Vector3.zero);
            beamBody.transform.localScale = GlobalizeScale(caster.transform, new Vector3(currentStyle.beamThickness.x, currentStyle.beamThickness.y, currentStyle.minBeamLength));

            // Beam Point setup

            if (!beamPoint)
            {
                if (beamPointPrefab != null)
                {
                    beamPoint = Instantiate(beamPointPrefab, caster.transform) as GameObject;
                }
                else
                {
                    beamPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    beamPoint.transform.SetParent(caster.transform, false);
                    beamPoint.layer = LayerMask.NameToLayer("Ignore Raycast");
                }

                beamPoint.name = "BeamPoint";
                beamPoint.layer = 2;
            }
            
            beamPoint.transform.localPosition = Vector3.zero;
            beamPoint.transform.localRotation = Quaternion.Euler(Vector3.zero);
            beamPoint.transform.localScale = GlobalizeScale(caster.transform, currentStyle.pointSize);

            // Renderering settings

            if (!baseBeamMaterial)
                beamBodyMaterial = new Material(Shader.Find("Unlit/Color"));
            else
                beamBodyMaterial = new Material(baseBeamMaterial);

            if (!baseCanvasMaterial)
                beamBodyCanvasMaterial = beamBodyMaterial;
            else
                beamBodyCanvasMaterial = new Material(baseCanvasMaterial);

            beamBodyMaterial.color = currentStyle.beamColor;
            beamBodyCanvasMaterial.color = currentStyle.beamColor;

            if (!baseBeamMaterial)
                beamPointMaterial = new Material(Shader.Find("Unlit/Color"));
            else
                beamPointMaterial = new Material(baseBeamMaterial);

            if (!baseCanvasMaterial)
                beamPointCanvasMaterial = beamPointMaterial;
            else
                beamPointCanvasMaterial = new Material(baseCanvasMaterial);
             
            beamPointMaterial.color = currentStyle.pointColor;
            beamPointCanvasMaterial.color = currentStyle.pointColor;

            beamPointRenderers = beamPoint.GetComponentsInChildren<Renderer>().ToList();
            beamBodyRenderers = beamBody.GetComponentsInChildren<Renderer>().ToList();

            beamPointRenderers.ForEach(r => r.material = beamPointMaterial);
            beamBodyRenderers.ForEach(r => r.material = beamBodyMaterial);

            Default(caster);
        }

        public override IEnumerator UpdateBeam(SpaceCaster caster, List<UnityEngine.EventSystems.RaycastResult> beamHits)
        {
            while (State != BeamState.Off)
            {
                if (beamHits.Where(hit => hit.gameObject).ToList().Count > 0)
                {
                    var closestHit = beamHits.OrderBy(h => h.distance).OrderByDescending(h => h.depth).First();

                    if (closestHit.gameObject.GetComponent<UnityEngine.UI.Graphic>())
                    {
                        beamPointRenderers.ForEach(r => { r.material = beamPointCanvasMaterial; r.sortingOrder = closestHit.sortingOrder + 1; });
                        beamBodyRenderers.ForEach(r => r.material = beamBodyCanvasMaterial);
                    }
                    else
                    {
                        beamPointRenderers.ForEach(r => r.material = beamPointMaterial);
                        beamBodyRenderers.ForEach(r => r.material = beamBodyMaterial);
                    }

                    OnTarget(caster);

                    float reach = closestHit.distance > currentStyle.maxBeamLength ? currentStyle.maxBeamLength : closestHit.distance;

                    // TODO: change reposition into a lerp
                    beamPoint.transform.localPosition = Vector3.forward * reach;
                    beamBody.transform.localScale = GlobalizeScale(caster.transform, new Vector3(currentStyle.beamThickness.x, currentStyle.beamThickness.y, reach));
                    beamBody.transform.localPosition = Vector3.forward * reach * 0.5f;
                }
                else
                {
                    Default(caster);
                    beamPoint.transform.localPosition = Vector3.forward * currentStyle.minBeamLength;
                    beamBody.transform.localScale = GlobalizeScale(caster.transform, new Vector3(currentStyle.beamThickness.x, currentStyle.beamThickness.y, currentStyle.minBeamLength));
                    beamBody.transform.localPosition = Vector3.forward * currentStyle.minBeamLength * 0.5f;
                }

                yield return null;
            }

            Default(caster);

            beamPoint.transform.localPosition = Vector3.forward * currentStyle.minBeamLength;
            beamBody.transform.localScale = GlobalizeScale(caster.transform, new Vector3(currentStyle.beamThickness.x, currentStyle.beamThickness.y, currentStyle.minBeamLength));
            beamBody.transform.localPosition = Vector3.forward * currentStyle.minBeamLength * 0.5f;
        }

        public override void Default(SpaceCaster caster)
        {
            base.Default(caster);

            beamPointRenderers.ForEach(r => r.material = beamPointMaterial);
            beamBodyRenderers.ForEach(r => r.material = beamBodyMaterial);

            UpdateMaterialColors();
        }

        public override void OnTarget(SpaceCaster caster)
        {
            base.OnTarget(caster);

            UpdateMaterialColors();
        }

        private void UpdateMaterialColors()
        {
            beamPointMaterial.color = currentStyle.pointColor;
            beamPointCanvasMaterial.color = currentStyle.pointColor;

            beamBodyMaterial.color = currentStyle.beamColor;
            beamBodyCanvasMaterial.color = currentStyle.beamColor;
        }

        /// <summary>
        /// Adjusts scale of vector to be absolute global scale relative to this.transform.
        /// </summary>
        /// <param name="localScale">The vector to be adjusted to absolute value.</param>
        /// <returns></returns>
        private Vector3 GlobalizeScale(Transform transform, Vector3 localScale)
        {
            if (lastParentScale != transform.lossyScale)
            {
                lastParentScale = transform.lossyScale;
                globalScaleFactor = new Vector3(1f / lastParentScale.x, 1f / lastParentScale.y, 1f / lastParentScale.z);
            }

            return Vector3.Scale(localScale, globalScaleFactor);
        }

        public override GameObject[] GetVisualizerGameObjects()
        {
            return new GameObject[] { beamBody, beamPoint };
        }
    }
}