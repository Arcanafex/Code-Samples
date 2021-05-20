using UnityEngine;
using System.Collections;
using System.Linq;


namespace Spaces.Core
{
    public class SpinWidget : Widget
    {
        public AnimationCurve X;
        public AnimationCurve Y;
        public AnimationCurve Z;

        public float cycleLength;
        public float speedMultiplier;
        private float elapsedTime;

        private Widget m_widget;
        public override Widget parentWidget
        {
            get { return m_widget; }
        }

        protected override void Start()
        {
            elapsedTime = 0;
            m_widget = GetComponentsInParent<Widget>().FirstOrDefault(w => w.GetComponent<AssetHandlerWidget>() != null);
        }

        void Update()
        {
            if (m_widget)
            {
                elapsedTime += Time.deltaTime;
                float cycleEvalPoint = elapsedTime / cycleLength;

                if (cycleEvalPoint > 1)
                {
                    cycleEvalPoint = 0;
                    elapsedTime = 0;
                }

                Vector3 spin = new Vector3(X.Evaluate(cycleEvalPoint), Y.Evaluate(cycleEvalPoint), Z.Evaluate(cycleEvalPoint));

                m_widget.transform.Rotate(spin * speedMultiplier);
            }
        }
    }
}
