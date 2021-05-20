using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Spaces.Extensions;
using System;

namespace Spaces.Core
{
    public interface IModelWidget
    {
        void SetMaterial(Material mat);
        void SetTexture(Texture tex);
        void SetColor(Color color);
        GameObject[] GetRenderedObjects();
        void SubscribeForInstancing(ModelWidget.ModelInstanced listener);
        void UnsubscribeForInstancing(ModelWidget.ModelInstanced listener);
    }

    /// <summary>
    /// A 3D asset object. The basic building block for any geometry in a space.
    /// </summary>

    public class ModelWidget : AssetHandlerWidget, IModelWidget
    {
        //TODO: combine this with the Video "Create Display" code
        public enum Primitive
        {
            Cube,
            Sphere,
            Cylinder,
            Capsule,
            Quad,
            Plane,
            InvertedCube,
            InvertedSphere,
            CurvedScreen,
        }

        public enum Type
        {
            Empty,
            Primitive,
            Prefab,
            Asset
        }

        public Type type;
        public Primitive primitive;
        public string prefabName;
        public bool colliderOnly;

        public delegate void ModelInstanced(ModelWidget model);
        public event ModelInstanced onModelInstanced;

        public override void Initialize()
        {
            base.Initialize();

            if (type == Type.Primitive)
            {
                GeneratePrimitive(primitive, transform);
                OnInstantiateAsset();
            }
            else if (type == Type.Prefab)
            {
                if (!string.IsNullOrEmpty(prefabName))
                {
                    GameObject prefab = UnityClient.UserSession.Instance.GetInstance(prefabName, transform);
                    prefab.AddComponent<InstancedAssetWidget>();
                    OnInstantiateAsset();
                }
            }
        }

        public override void Initialize(AssetWidget assetWidget)
        {
            base.Initialize(assetWidget);
            this.type = Type.Asset;
            initialized = true;
        }

        public void Initialize(Primitive primitive)
        {
            this.type = Type.Primitive;
            this.primitive = primitive;
            Initialize();
        }

        public static GameObject GeneratePrimitive(Primitive type, Transform parent)
        {
            GameObject primitive;

            switch (type)
            {
                case Primitive.Cube:
                    {
                        primitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        primitive.transform.SetParent(parent, false);
                        break;
                    }
                case Primitive.Sphere:
                    {
                        primitive = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        primitive.transform.SetParent(parent, false);
                        break;
                    }
                case Primitive.Capsule:
                    {
                        primitive = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                        primitive.transform.SetParent(parent, false);
                        break;
                    }
                case Primitive.Cylinder:
                    {
                        primitive = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                        primitive.transform.SetParent(parent, false);
                        break;
                    }
                case Primitive.Quad:
                    {
                        primitive = GameObject.CreatePrimitive(PrimitiveType.Quad);
                        primitive.transform.SetParent(parent, false);
                        break;
                    }
                case Primitive.Plane:
                    {
                        primitive = GameObject.CreatePrimitive(PrimitiveType.Plane);
                        primitive.transform.SetParent(parent, false);
                        break;
                    }
                case Primitive.InvertedCube:
                    {
                        primitive = UnityClient.UserSession.Instance.GetInstance("Cubemap Video Display", parent);
                        break;
                    }
                case Primitive.InvertedSphere:
                    {
                        primitive = UnityClient.UserSession.Instance.GetInstance("Spherical Video Display", parent);
                        break;
                    }
                case Primitive.CurvedScreen:
                    {
                        primitive = UnityClient.UserSession.Instance.GetInstance("Standard Video Display", parent);
                        break;
                    }
                default:
                    primitive = null;
                    break;
            }

            if (primitive)
                primitive.AddComponent<InstancedAssetWidget>();

            return primitive;
        }

        //public void FitWithinWorldBounds(Bounds bounds)
        //{
        //    var modelBounds = transform.CalculateBounds();
        //    int modelBoundsMax = GetMaxDimension(modelBounds.size);
        //    int fitBoundsMax = GetMaxDimension(bounds.size);

        //    float scaleFactor = 1;

        //    if (modelBoundsMax == 0)
        //        scaleFactor = bounds.size.x / modelBounds.size.x;
        //    else if (modelBoundsMax == 1)
        //        scaleFactor = bounds.size.y / modelBounds.size.y;
        //    else
        //        scaleFactor = bounds.size.z / modelBounds.size.z;
        //}

        //private int GetMaxDimension(Vector3 volume)
        //{
        //    return volume.x >= volume.y ?
        //        volume.x >= volume.z ? 0 : 2
        //        : volume.y >= volume.z ? 1 : 2;
        //}

        public void SetMaterial(Material mat)
        {
            foreach (MeshFilter r in GetComponentsInChildren<MeshFilter>())
            {
                if (r.gameObject.GetComponent<Renderer>())
                    r.gameObject.GetComponent<Renderer>().material = mat;
            }
        }

        public void SetTexture(Texture tex)
        {
            foreach (MeshFilter r in GetComponentsInChildren<MeshFilter>())
            {
                if (r.gameObject.GetComponent<Renderer>())
                    r.gameObject.GetComponent<Renderer>().material.mainTexture = tex;
                    //r.gameObject.GetComponent<Renderer>().sharedMaterial.mainTexture = tex;
            }
        }

        public void SetTexture(string texName, Texture tex)
        {
            foreach (MeshFilter r in GetComponentsInChildren<MeshFilter>())
            {
                if (r.gameObject.GetComponent<Renderer>())
                    r.gameObject.GetComponent<Renderer>().material.SetTexture(texName, tex);
            }
        }

        public void SetColor(Color color)
        {
            foreach (MeshFilter r in GetComponentsInChildren<MeshFilter>())
            {
                if (r.gameObject.GetComponent<Renderer>())
                    r.gameObject.GetComponent<Renderer>().material.color = color;
            }
        }

        public GameObject[] GetRenderedObjects()
        {
            List<GameObject> renderedGameObjects = new List<GameObject>();

            //foreach (MeshFilter r in GetComponentsInChildren<MeshFilter>())
            //{
            //    if (r.gameObject.GetComponent<Renderer>())
            //        renderedGameObjects.Add(r.gameObject);
            //}
            foreach (var r in GetComponentsInChildren<Renderer>())
            {
                renderedGameObjects.Add(r.gameObject);
            }


            return renderedGameObjects.ToArray();
        }

        public virtual void SubscribeForInstancing(ModelInstanced listener)
        {
            try
            {
                onModelInstanced += listener;
            }
            catch (Exception ex)
            {
                Debug.LogError(this.name + " [Subscribe Listener Error] " + ex.Message);
            }
        }

        public virtual void UnsubscribeForInstancing(ModelInstanced listener)
        {
            try
            {
                onModelInstanced -= listener;
            }
            catch (Exception ex)
            {
                Debug.LogError(this.name + " [Subscribe Listener Error] " + ex.Message);
            }
        }

        public override void OnInstantiateAsset()
        {
            //Debug.Log(transform.CalculateBounds().ToString());
            MaterialWidget.GenerateMaterialWidgets(this);

            if (onModelInstanced != null)
                onModelInstanced(this);
        }

        public override GameObject InstancePlaceholder()
        {
            GameObject placeholder = null;

            if (UnityClient.UserSession.Instance.modelPlaceholderPrefab)
            {
                placeholder = Instantiate(UnityClient.UserSession.Instance.modelPlaceholderPrefab, transform, false) as GameObject;
            }
            else
            {
                placeholder = GameObject.CreatePrimitive(PrimitiveType.Cube);
                placeholder.transform.SetParent(transform, false);
            }

            if (onModelInstanced != null)
                onModelInstanced(this);

            return placeholder;
        }

        private IEnumerator WaitingForRenderer()
        {
            while (!GetComponentInChildren<Renderer>())
            {
                yield return null;
            }

            OnInstantiateAsset();
        }
    }
}