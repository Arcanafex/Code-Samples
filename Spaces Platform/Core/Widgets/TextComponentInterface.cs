using UnityEngine.UI;
using UnityEngine;
using System.Collections;

namespace Spaces.Core
{
    [System.Serializable]
    public class TextComponentSettings
    {
        //public string text;
        public TextComponentInterface.Type textComponentType;
        public TextAnchor anchor;
        public float size;
        public Color color;
        public Font font;
        public int fontSize;
        public FontStyle fontStyle;
        public float lineSpacing;
        public float offsetZ;
        public bool supportRichText;
        public float tabSize;
    }

    public class TextComponentInterface
    {
        public enum Type
        {
            Mesh,
            Canvas
        }

        public readonly Type componentType;
        public Component textComponent
        {
            get
            {
                if (componentType == Type.Mesh)
                    return m_mesh;
                else if (componentType == Type.Canvas)
                    return m_canvas;
                else
                    return null;
            }
        }

        private TextWidget m_widget;
        private TextMesh m_mesh;
        private Text m_canvas;

        public TextComponentInterface(TextWidget textWidget, Text textComponent)
        {
            m_widget = textWidget;
            componentType = Type.Canvas;
            m_canvas = textComponent;
            ApplySettings(m_widget);
        }

        public TextComponentInterface(TextWidget textWidget, TextMesh textComponent)
        {
            m_widget = textWidget;
            componentType = Type.Mesh;
            m_mesh = textComponent;
            ApplySettings(m_widget);
        }

        public TextComponentInterface(TextWidget textWidget, Type type)
        {
            m_widget = textWidget;
            componentType = type;

            if (componentType == Type.Canvas)
            {
                var textComponent = UnityClient.UserSession.Instance.GetInstance("Text Canvas", m_widget.transform);
                m_canvas = textComponent.GetComponentInChildren<Text>();
            }
            else if (componentType == Type.Mesh)
            {
                var textComponent = textWidget.gameObject.AddComponent<TextMesh>();
                m_mesh = textComponent;
            }

            ApplySettings(m_widget);
        }

        private void ApplySettings(TextWidget widget)
        {
            text = widget.text;
            anchor = widget.settings.anchor;
            size = widget.settings.size;
            color = widget.settings.color;
            font = widget.settings.font;
            fontSize = widget.settings.fontSize;
            fontStyle = widget.settings.fontStyle;
            lineSpacing = widget.settings.lineSpacing;
            offsetZ = widget.settings.offsetZ;
            supportRichText = widget.settings.supportRichText;
            tabSize = widget.settings.tabSize;
        }

        //anchor	Which point of the text shares the position of the Transform.
        public TextAnchor anchor
            {
                get
                {
                    if (componentType == Type.Mesh)
                        return m_mesh.anchor;
                    else if (componentType == Type.Canvas)
                        return m_canvas.alignment;
                    else
                        return TextAnchor.MiddleCenter;
                }

                set
                {
                    m_widget.settings.anchor = value;

                    if (componentType == Type.Mesh)
                        m_mesh.anchor = value;
                    else if (componentType == Type.Canvas)
                        m_canvas.alignment = value;
                }
            }

        //characterSize	The size of each character (This scales the whole text).
        public float size
        {
            get
            {
                if (componentType == Type.Mesh)
                    return m_mesh.characterSize;
                else if (componentType == Type.Canvas)
                    return m_canvas.rectTransform.lossyScale.y;
                else
                    return -1;
            }

            set
            {
                m_widget.settings.size = value;

                if (componentType == Type.Mesh)
                    m_mesh.characterSize = value;
                else if (componentType == Type.Canvas)
                    m_canvas.rectTransform.localScale = Vector3.one * value;
            }
        }

        //color	The color used to render the text.
        public Color color
        {
            get
            {
                if (componentType == Type.Mesh)
                    return m_mesh.color;
                else if (componentType == Type.Canvas)
                    return m_canvas.color;
                else
                    return Color.white;
            }

            set
            {
                m_widget.settings.color = value;

                if (componentType == Type.Mesh)
                    m_mesh.color = value;
                else if (componentType == Type.Canvas)
                    m_canvas.color = value;
            }
        }

        //font	The Font used.
        public Font font
        {
            get
            {
                if (componentType == Type.Mesh)
                    return m_mesh.font;
                else if (componentType == Type.Canvas)
                    return m_canvas.font;
                else
                    return null;
            }

            set
            {
                m_widget.settings.font = value;

                if (componentType == Type.Mesh)
                    m_mesh.font = value;
                else if (componentType == Type.Canvas)
                    m_canvas.font = value;
            }
        }

        //fontSize	The font size to use (for dynamic fonts).
        public int fontSize
        {
            get
            {
                if (componentType == Type.Mesh)
                    return m_mesh.fontSize;
                else if (componentType == Type.Canvas)
                    return m_canvas.fontSize;
                else
                    return -1;
            }

            set
            {
                m_widget.settings.fontSize = value;

                if (componentType == Type.Mesh)
                    m_mesh.fontSize = value;
                else if (componentType == Type.Canvas)
                    m_canvas.fontSize = value;
            }
        }

        //fontStyle	The font style to use (for dynamic fonts).
        public FontStyle fontStyle
        {
            get
            {
                if (componentType == Type.Mesh)
                    return m_mesh.fontStyle;
                else if (componentType == Type.Canvas)
                    return m_canvas.fontStyle;
                else
                    return FontStyle.Normal;
            }

            set
            {
                m_widget.settings.fontStyle = value;

                if (componentType == Type.Mesh)
                    m_mesh.fontStyle = value;
                else if (componentType == Type.Canvas)
                    m_canvas.fontStyle = value;
            }
        }

        //lineSpacing	How much space will be in-between lines of text.
        public float lineSpacing
        {
            get
            {
                if (componentType == Type.Mesh)
                    return m_mesh.lineSpacing;
                else if (componentType == Type.Canvas)
                    return m_canvas.lineSpacing;
                else
                    return -1;
            }

            set
            {
                m_widget.settings.lineSpacing = value;

                if (componentType == Type.Mesh)
                    m_mesh.lineSpacing = value;
                else if (componentType == Type.Canvas)
                    m_canvas.lineSpacing = value;
            }
        }

        //offsetZ	How far should the text be offset from the transform.position.z when drawing.
        public float offsetZ
        {
            get
            {
                if (componentType == Type.Mesh)
                    return m_mesh.offsetZ;
                else if (componentType == Type.Canvas)
                    return m_canvas.rectTransform.localPosition.z;
                else
                    return -1;
            }

            set
            {
                m_widget.settings.offsetZ = value;

                if (componentType == Type.Mesh)
                    m_mesh.offsetZ = value;
                else if (componentType == Type.Canvas)
                    m_canvas.rectTransform.localPosition = new Vector3(m_canvas.rectTransform.localPosition.x, m_canvas.rectTransform.localPosition.y, value);
            }
        }

        //richText	Enable HTML-style tags for Text Formatting Markup.
        public bool supportRichText
        {
            get
            {
                if (componentType == Type.Mesh)
                    return m_mesh.richText;
                else if (componentType == Type.Canvas)
                    return m_canvas.supportRichText;
                else
                    return false;
            }

            set
            {
                m_widget.settings.supportRichText = value;

                if (componentType == Type.Mesh)
                    m_mesh.richText = value;
                else if (componentType == Type.Canvas)
                    m_canvas.supportRichText = value;
            }
        }

        //tabSize	How much space will be inserted for a tab '\t' character. This is a multiplum of the 'spacebar' character offset.
        public float tabSize
        {
            get
            {
                if (componentType == Type.Mesh)
                    return m_mesh.tabSize;
                else
                    return -1;
            }

            set
            {
                m_widget.settings.tabSize = value;

                if (componentType == Type.Mesh)
                    m_mesh.tabSize = value;
            }
        }

        //text	The text that is displayed.
        public string text
        {
            get
            {
                if (componentType == Type.Mesh)
                    return m_mesh.text;
                else if (componentType == Type.Canvas)
                    return m_canvas.text;
                else
                    return "";
            }

            set
            {
                if (componentType == Type.Mesh)
                    m_mesh.text = value;
                else if (componentType == Type.Canvas)
                    m_canvas.text = value;
            }
        }
    }
}