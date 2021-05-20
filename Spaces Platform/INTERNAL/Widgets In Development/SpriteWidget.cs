using System;
using UnityEngine;
using UnityEngine.UI;

namespace Spaces.Core
{
    public class SpriteWidget : AssetHandlerWidget, IPopupWidget, IDisplay<Sprite>
    {
        public Sprite m_sprite;
        private Image m_imageComponent;
        private SpriteRenderer m_spriteRenderer;

        void Start()
        {
            if (!m_imageComponent)
                m_imageComponent = GetComponentInChildren<Image>();

            if (!m_spriteRenderer)
                m_spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            SetContent(m_sprite);
        }

        public void SetContent(Sprite sprite)
        {
            m_sprite = sprite;

            if (m_imageComponent)
                m_imageComponent.sprite = sprite;

            if (m_spriteRenderer)
                m_spriteRenderer.sprite = sprite;
        }

        public void Show()
        {
            foreach (Transform child in transform)
                child.gameObject.SetActive(true);
        }

        public void Hide()
        {
            foreach (Transform child in transform)
                child.gameObject.SetActive(false);
        }

        public void FaceCamera()
        {
            transform.LookAt(Camera.main.transform);
        }

        public override GameObject InstancePlaceholder()
        {
            throw new NotImplementedException();
        }
    }
}