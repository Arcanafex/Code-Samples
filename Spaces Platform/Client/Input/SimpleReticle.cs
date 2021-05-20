using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Spaces.UnityClient
{
    [CreateAssetMenu(menuName = "Space Casters/Simple Reticle")]
    public class SimpleReticle : CasterVisualizer
    {
        public float minReticleDistance = 0;
        public float maxReticleDistance = 5;

        public Vector2 reticleSize = Vector2.one;

        public Color DefaultColor;
        public Color OnTargetColor;
        public Color OnInteractiveTargetColor;

        public SpriteRenderer reticlePrefab;

        private GameObject reticle;
        private SpriteRenderer reticleSpriteRenderer;
        private Vector2 reticleBaseScale;

        public override void GenerateBeam(SpaceCaster caster)
        {
            // Beam Body setup

            if (reticlePrefab)
            {
                reticle = Instantiate(reticlePrefab.gameObject, caster.transform) as GameObject;
                reticleSpriteRenderer = reticle.GetComponent<SpriteRenderer>();
            }
            else
            {
                reticle = new GameObject("Reticle");
                reticleSpriteRenderer = reticle.AddComponent<SpriteRenderer>();
            }

            reticle.layer = 2;
            reticleBaseScale = reticle.transform.localScale;
        }

        public override IEnumerator UpdateBeam(SpaceCaster caster, List<UnityEngine.EventSystems.RaycastResult> raycastHits)
        {
            while (caster.casterActive)
            {
                if (raycastHits.Count > 0)
                {
                    var closestHit = raycastHits.OrderBy(h => h.distance).OrderByDescending(h => h.depth).First();

                    OnTarget(caster);

                    float reach = closestHit.distance > maxReticleDistance ? maxReticleDistance : closestHit.distance;

                    // TODO: change reposition into a lerp
                    reticle.transform.localPosition = Vector3.forward * reach;
                    reticle.transform.localScale = Vector3.Scale(reticleBaseScale, reticleSize) * (reach / maxReticleDistance);
                }
                else
                {
                    Default(caster);
                    reticle.transform.localPosition = Vector3.forward * minReticleDistance;
                    reticle.transform.localScale = Vector3.Scale(reticleBaseScale, reticleSize);
                }

                yield return null;
            }

            Default(caster);
            reticle.transform.localPosition = Vector3.forward * minReticleDistance;
            reticle.transform.localScale = Vector3.Scale(reticleBaseScale, reticleSize);
        }

        public override void Default(SpaceCaster caster)
        {
            base.Default(caster);

            reticleSpriteRenderer.color = DefaultColor;
        }

        public override void OnTarget(SpaceCaster caster)
        {
            base.OnTarget(caster);

            reticleSpriteRenderer.color = OnTargetColor;
        }


        public override GameObject[] GetVisualizerGameObjects()
        {
            return new GameObject[] { reticle };
        }

    }
}