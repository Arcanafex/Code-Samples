using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spaces.Core
{
    public class AnimatorWidget : Widget, IPlayable
    {
        public bool Loop;

        private Animator m_animator;
        private ModelWidget m_model;
        private float playSpeed;
        
        public void Initialize(ModelWidget model, Animator animator = null)
        {
            m_model = model;
            m_animator = animator ? animator : model.GetComponentInChildren<Animator>();
            playSpeed = m_animator.speed;
            Loop = m_animator.GetCurrentAnimatorStateInfo(0).loop;
        }

        void Start()
        {
            Play();
        }

        void LateUpdate()
        {
            if (m_animator)
            {
                var info = m_animator.GetCurrentAnimatorStateInfo(0);

                if (Loop && !info.loop)
                {
                    if (info.normalizedTime + (Time.deltaTime / info.length) >= 1)
                    {
                        Restart();
                    }
                }
                else if (!Loop && info.loop)
                {
                    if (info.normalizedTime + (Time.deltaTime / info.length) >= 1)
                    {
                        Pause();
                    }
                }
            }
        }

        public void Pause()
        {
            if (m_animator)
                m_animator.speed = 0;
        }

        public void Play()
        {
            if (!m_animator)
                return;

            if (m_animator.speed == 0)
                m_animator.speed = playSpeed;

            m_animator.Play("BaseLoopAnim");
        }

        public void Restart()
        {
            if (!m_animator)
                return;

            var stateInfo = m_animator.GetCurrentAnimatorStateInfo(0);
            m_animator.Play(stateInfo.fullPathHash, 0, 0);
        }

        public void Stop()
        {
            Pause();
            Restart();
        }
    }

}