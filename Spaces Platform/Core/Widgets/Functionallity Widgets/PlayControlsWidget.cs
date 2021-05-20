using UnityEngine;
using System.Linq;

namespace Spaces.Core
{
    public class PlayControlsWidget : Widget, IPlayable
    {
        private Widget m_controlTarget;
        public override Widget parentWidget
        {
            get
            {
                return m_controlTarget;
            }
        }

        void Start()
        {
            if (transform.parent)
                m_controlTarget = transform.parent.GetComponentsInParent<Widget>().FirstOrDefault(w => w.GetComponent<IPlayable>() != null);
        }

        public void Pause()
        {
            if (m_controlTarget)
                ((IPlayable)m_controlTarget).Pause();
        }

        public void Play()
        {
            if (m_controlTarget)
                ((IPlayable)m_controlTarget).Play();
        }

        public void Restart()
        {
            if (m_controlTarget)
                ((IPlayable)m_controlTarget).Restart();
        }

        public void Stop()
        {
            if (m_controlTarget)
                ((IPlayable)m_controlTarget).Stop();
        }
    }
}
