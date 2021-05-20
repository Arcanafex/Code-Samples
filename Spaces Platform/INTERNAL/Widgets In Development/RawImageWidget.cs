using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

namespace Spaces.Core
{
    /// <summary>
    /// Like an Image Widget, but specifically using the UI.Canvas and RawImage object.
    /// </summary>

    public class RawImageWidget : AssetHandlerWidget, IModelWidget, IDisplay<Texture>
    {
        public RawImage Image
        {
            get { return GetComponentInChildren<RawImage>(); }
        }

        public GameObject[] GetRenderedObjects()
        {
            List<GameObject> renderedGameObjects = new List<GameObject>();

            foreach (RawImage r in GetComponentsInChildren<RawImage>())
            {
                renderedGameObjects.Add(r.gameObject);
            }

            return renderedGameObjects.ToArray();
        }

        public void SetMaterial(Material mat)
        {
            foreach (RawImage r in GetComponentsInChildren<RawImage>())
            {
                r.material = mat;
            }
        }

        public void SetTexture(Texture tex)
        {
            foreach (RawImage r in GetComponentsInChildren<RawImage>())
            {
                r.material.mainTexture = tex;
            }
        }

        public void SetTexture(string texName, Texture tex)
        {
            foreach (RawImage r in GetComponentsInChildren<RawImage>())
            {
                r.material.SetTexture(texName, tex);
            }
        }

        public void SetColor(Color color)
        {
            foreach (RawImage r in GetComponentsInChildren<RawImage>())
            {
                r.material.color = color;
            }
        }

        public void SetContent(Texture content)
        {
            SetTexture(content);
        }

        public override GameObject InstancePlaceholder()
        {
            throw new NotImplementedException();
        }

        public void SubscribeForInstancing(ModelWidget.ModelInstanced listener)
        {
            throw new NotImplementedException();
        }

        public void UnsubscribeForInstancing(ModelWidget.ModelInstanced listener)
        {
            throw new NotImplementedException();
        }
    }
}
