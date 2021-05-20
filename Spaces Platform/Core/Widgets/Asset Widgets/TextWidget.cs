using System;
using UnityEngine;
using UnityEngine.UI;

namespace Spaces.Core
{
    /// <summary>
    /// A widget for encapsulating and displaying a piece of text in a Space. 
    /// </summary>

    public class TextWidget : AssetHandlerWidget, IDisplay<string>
    {
        public string text;
        public TextComponentSettings settings;

        private TextComponentInterface m_textComponent;

        public override void Initialize()
        {
            base.Initialize();

            var canvas = GetComponentInChildren<Text>();
            var mesh = GetComponentInChildren<TextMesh>();

            if (mesh)
                m_textComponent = new TextComponentInterface(this, mesh);
            else if (canvas)
                m_textComponent = new TextComponentInterface(this, canvas);
            else
                m_textComponent = new TextComponentInterface(this, settings != null ? settings.textComponentType : TextComponentInterface.Type.Mesh);
        }

        public void Initialize(TextComponentInterface.Type textComponentType)
        {
            m_textComponent = new TextComponentInterface(this, textComponentType);
        }

        public void Initialize(Text textComponent)
        {
            m_textComponent = new TextComponentInterface(this, textComponent);
        }

        public void Initialize(TextMesh textComponent)
        {
            m_textComponent = new TextComponentInterface(this, textComponent);
        }

        public void SetContent(string text)
        {
            this.text = text;
            m_textComponent.text = text;
        }

        public override GameObject InstancePlaceholder()
        {
            return gameObject;
            SetContent("Spaces, Yo!");
        }
    }


}