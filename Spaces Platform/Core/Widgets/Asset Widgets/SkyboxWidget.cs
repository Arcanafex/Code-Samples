using UnityEngine;
using System.Collections;
using System.Linq;

namespace Spaces.Core
{
    public class SkyboxWidget : AssetHandlerWidget
    {
        public enum SkyboxType
        {
            None,
            SixSided,
            Cubemap,
            Procedural
        }

        public enum SunType
        {
            None,
            Simple,
            HighQuality
        }

        //[System.Serializable]
        //public class SkyboxMaterial
        //{
        //    [Range(0, 8)]
        //    public float Exposure;

        //    public Color Tint;

        //    public virtual Material CreateMaterial()
        //    {
        //        return null;
        //    }
        //}

        //[System.Serializable]
        //public class SixSidedSkybox : SkyboxMaterial
        //{
        //    [Range(0, 360)]
        //    public int Rotation;

        //    public Texture Front;
        //    public Texture Back;
        //    public Texture Left;
        //    public Texture Right;
        //    public Texture Up;
        //    public Texture Down;

        //    public override Material CreateMaterial()
        //    {
        //        var skyboxMaterial = new Material(Shader.Find("Skybox/6 Sided"));

        //        skyboxMaterial.SetColor("_Tint", Tint);
        //        skyboxMaterial.SetFloat("_Exposure", Exposure);
        //        skyboxMaterial.SetInt("_Rotation", Rotation);
        //        skyboxMaterial.SetTexture("_FrontTex", Front);
        //        skyboxMaterial.SetTexture("_BackTex", Back);
        //        skyboxMaterial.SetTexture("_LeftTex", Left);
        //        skyboxMaterial.SetTexture("_RightTex", Right);
        //        skyboxMaterial.SetTexture("_UpTex", Up);
        //        skyboxMaterial.SetTexture("_DownTex", Down);

        //        return skyboxMaterial;
        //    }
        //}

        //[System.Serializable]
        //public class CubemapSkybox : SkyboxMaterial
        //{
        //    [Range(0, 360)]
        //    public int Rotation;

        //    public Cubemap Cubemap;

        //    public override Material CreateMaterial()
        //    {
        //        var skyboxMaterial = new Material(Shader.Find("Skybox/Cubemap"));

        //        skyboxMaterial.SetColor("_Tint", Tint);
        //        skyboxMaterial.SetFloat("_Exposure", Exposure);
        //        skyboxMaterial.SetInt("_Rotation", Rotation);
        //        skyboxMaterial.SetTexture("_Tex", Cubemap);

        //        return skyboxMaterial;
        //    }
        //}

        //[System.Serializable]
        //public class ProceduralSkybox : SkyboxMaterial
        //{
        //    public enum SunType
        //    {
        //        None,
        //        Simple,
        //        HighQuality
        //    }

        //    public SunType Sun;

        //    [Range(0, 1)]
        //    public float SunSize;

        //    [Range(0,5)]
        //    public float AtmosphericThickness;

        //    public Color GroundColor;

        //    public override Material CreateMaterial()
        //    {
        //        var skyboxMaterial = new Material(Shader.Find("Skybox/Procedural"));

        //        skyboxMaterial.SetInt("_SunDisk", (int)Sun);
        //        skyboxMaterial.SetFloat("_SunSize", SunSize);
        //        skyboxMaterial.SetFloat("_AtmosphereThickness", AtmosphericThickness);
        //        skyboxMaterial.SetColor("_SkyTint", Tint);
        //        skyboxMaterial.SetColor("_GroundColor", GroundColor);
        //        skyboxMaterial.SetFloat("_Exposure", Exposure);

        //        return skyboxMaterial;
        //    }
        //}

        public SkyboxType type = SkyboxType.SixSided;
        public bool stereo;
        public bool applyOnStart;

        protected MaterialWidget m_materialWidget { get; set; }
        protected Material m_material
        {
            get
            {
                return m_materialWidget ? m_materialWidget.m_material : null;
            }
        }

        protected MaterialWidget m_secondaryMaterialWidget { get; set; }
        protected Material m_secondaryMaterial
        {
            get
            {
                return m_secondaryMaterialWidget.m_material;
            }
        }


        //public SunType Sun
        //{
        //    get
        //    {
        //        if (m_material.HasProperty("_SunDisk"))
        //        {
        //            int sun = m_material.GetInt("_SunDisk");
        //            return (SunType)sun;
        //        }
        //        else
        //        {
        //            return SunType.None;
        //        }
        //    }
        //    set
        //    {
        //        if (m_material.HasProperty("_SunDisk"))
        //        {
        //            m_material.SetInt("_SunDisk", (int)value);
        //        }
        //    }
        //}
        //public float SunSize
        //{
        //    get
        //    {
        //        if (m_material.HasProperty("_SunSize"))
        //        {
        //            return m_material.GetFloat("_SunSize");
        //        }
        //        else
        //        {
        //            return 0;
        //        }
        //    }

        //    set
        //    {
        //        if (m_material.HasProperty("_SunSize"))
        //        {
        //            m_material.SetFloat("_SunSize", Mathf.Clamp(value, 0, 1));
        //        }
        //    }
        //}
        //public float AtmosphericThickness
        //{
        //    get
        //    {
        //        if (m_material.HasProperty("_AtmosphereThickness"))
        //        {
        //            return m_material.GetFloat("_AtmosphereThickness");
        //        }
        //        else
        //        {
        //            return 0;
        //        }
        //    }

        //    set
        //    {
        //        if (m_material.HasProperty("_AtmosphereThickness"))
        //        {
        //            m_material.SetFloat("_AtmosphereThickness", Mathf.Clamp(value, 0, 5));
        //        }
        //    }
        //}
        //public float Exposure
        //{
        //    get
        //    {
        //        if (m_material.HasProperty("_Exposure"))
        //        {
        //            return m_material.GetFloat("_Exposure");
        //        }
        //        else
        //        {
        //            return 0;
        //        }
        //    }

        //    set
        //    {
        //        if (m_material.HasProperty("_Exposure"))
        //        {
        //            m_material.SetFloat("_Exposure", Mathf.Clamp(value, 0, 8));
        //        }
        //    }
        //}
        //public Color Tint
        //{
        //    get
        //    {
        //        if (m_material.HasProperty("_Tint"))
        //        {
        //            return m_material.GetColor("_Tint");
        //        }
        //        else if (m_material.HasProperty("_SkyTint"))
        //        {
        //            return m_material.GetColor("_SkyTint");
        //        }
        //        else
        //        {
        //            return Color.black;
        //        }
        //    }

        //    set
        //    {
        //        if (m_material.HasProperty("_Tint"))
        //        {
        //            m_material.SetColor("_Tint", value);
        //        }
        //        else if (m_material.HasProperty("_SkyTint"))
        //        {
        //            m_material.SetColor("_SkyTint", value);
        //        }
        //    }
        //}
        //public Color GroundColor
        //{
        //    get
        //    {
        //        if (m_material.HasProperty("_GroundColor"))
        //        {
        //            return m_material.GetColor("_GroundColor");
        //        }
        //        else
        //        {
        //            return Color.black;
        //        }
        //    }

        //    set
        //    {
        //        if (m_material.HasProperty("_GroundColor"))
        //        {
        //            m_material.SetColor("_GroundColor", value);
        //        }
        //    }
        //}

        //public int Rotation
        //{
        //    get
        //    {
        //        if (m_material.HasProperty("_Rotation"))
        //        {
        //            return m_material.GetInt("_Rotation");
        //        }
        //        else
        //        {
        //            return 0;
        //        }
        //    }

        //    set
        //    {
        //        if (m_material.HasProperty("_Rotation"))
        //        {
        //            m_material.SetInt("_Rotation", Mathf.Clamp(value, 0, 360));
        //        }
        //    }
        //}
        //public Texture Cubemap
        //{
        //    get
        //    {
        //        if (m_material.HasProperty("_Tex"))
        //        {
        //            return m_material.GetTexture("_Tex");
        //        }
        //        else
        //        {
        //            return Texture2D.blackTexture;
        //        }
        //    }

        //    set
        //    {
        //        if (m_material.HasProperty("_Tex"))
        //        {
        //            m_material.SetTexture("_Tex", value);
        //        }
        //    }
        //}

        //public Texture Front
        //{
        //    get
        //    {
        //        if (m_material.HasProperty("_FrontTex"))
        //            return m_material.GetTexture("_FrontTex");
        //        else
        //            return Texture2D.blackTexture;
        //    }
        //    set
        //    {
        //        if (m_material.HasProperty("_FrontTex"))
        //            m_material.SetTexture("_FrontTex", value);
        //    }
        //}
        //public Texture Back
        //{
        //    get
        //    {
        //        if (m_material.HasProperty("_BackTex"))
        //            return m_material.GetTexture("_BackTex");
        //        else
        //            return Texture2D.blackTexture;
        //    }
        //    set
        //    {
        //        if (m_material.HasProperty("_BackTex"))
        //            m_material.SetTexture("_BackTex", value);
        //    }
        //}
        //public Texture Left
        //{
        //    get
        //    {
        //        if (m_material.HasProperty("_LeftTex"))
        //            return m_material.GetTexture("_LeftTex");
        //        else
        //            return Texture2D.blackTexture;
        //    }
        //    set
        //    {
        //        if (m_material.HasProperty("_LeftTex"))
        //            m_material.SetTexture("_LeftTex", value);
        //    }
        //}
        //public Texture Right
        //{
        //    get
        //    {
        //        if (m_material.HasProperty("_RightTex"))
        //            return m_material.GetTexture("_RightTex");
        //        else
        //            return Texture2D.blackTexture;
        //    }
        //    set
        //    {
        //        if (m_material.HasProperty("_RightTex"))
        //            m_material.SetTexture("_RightTex", value);
        //    }
        //}
        //public Texture Up
        //{
        //    get
        //    {
        //        if (m_material.HasProperty("_UpTex"))
        //            return m_material.GetTexture("_UpTex");
        //        else
        //            return Texture2D.blackTexture;
        //    }
        //    set
        //    {
        //        if (m_material.HasProperty("_UpTex"))
        //            m_material.SetTexture("_UpTex", value);
        //    }
        //}
        //public Texture Down
        //{
        //    get
        //    {
        //        if (m_material.HasProperty("_DownTex"))
        //            return m_material.GetTexture("_DownTex");
        //        else
        //            return Texture2D.blackTexture;
        //    }
        //    set
        //    {
        //        if (m_material.HasProperty("_DownTex"))
        //            m_material.SetTexture("_DownTex", value);
        //    }
        //}

        //public void SetCubemap(Texture Front, Texture Back, Texture Left, Texture Right, Texture Up, Texture Down)
        //{
        //    if (m_material.HasProperty("_FrontTex")) m_material.SetTexture("_FrontTex", Front);
        //    if (m_material.HasProperty("_BackTex")) m_material.SetTexture("_BackTex", Back);
        //    if (m_material.HasProperty("_LeftTex")) m_material.SetTexture("_LeftTex", Left);
        //    if (m_material.HasProperty("_RightTex")) m_material.SetTexture("_RightTex", Right);
        //    if (m_material.HasProperty("_UpTex")) m_material.SetTexture("_UpTex", Up);
        //    if (m_material.HasProperty("_DownTex")) m_material.SetTexture("_DownTex", Down);
        //}


        protected override void Start()
        {
            base.Start();

            if (m_material && applyOnStart)
            {
                ApplySkybox();
                //UpdateSkybox();
            }
        }

        public override void Initialize()
        {
            base.Initialize();

            var materials = MaterialWidget.GenerateMaterialWidgets(this);

            m_materialWidget = materials.FirstOrDefault();

            if (stereo)
                m_secondaryMaterialWidget = materials[1];
            //Initialize(m_materialWidget);
        }

        public void Initialize(MaterialWidget material)
        {
            if (m_materialWidget != material)
                m_materialWidget = material;
        }

        //public void UpdateSkybox()
        //{
        //    //TODO: Make sure there is no timing problem with applying.

        //    ApplySkybox();
        //}

        public static void UpdateSkybox(Material skyboxMaterial)
        {
            RenderSettings.skybox = skyboxMaterial;
            DynamicGI.UpdateEnvironment();
        }

        public static void UpdateSkybox(Material leftSkyboxMaterial, Material rightSkyboxMaterial)
        {
            RenderSettings.skybox = leftSkyboxMaterial;

            if (AvatarWidget.UserAvatar)
            {
                var parts = AvatarWidget.UserAvatar.gameObject.GetComponentsInChildren<AvatarAnatomyWidget>();

                var leftEye = parts.FirstOrDefault(p => p.part == AvatarAnatomyWidget.Location.LeftEye);
                var rightEye = parts.FirstOrDefault(p => p.part == AvatarAnatomyWidget.Location.RightEye);

                if (leftEye)
                {
                    var leftSkybox = leftEye.GetComponent<Skybox>();

                    if (!leftSkybox)
                        leftSkybox = leftEye.gameObject.AddComponent<Skybox>();

                    leftSkybox.material = leftSkyboxMaterial;
                }

                if (rightEye)
                {
                    var rightSkybox = rightEye.GetComponent<Skybox>();

                    if (!rightSkybox)
                        rightSkybox = rightEye.gameObject.AddComponent<Skybox>();

                    rightSkybox.material = rightSkyboxMaterial;
                }

            }

            DynamicGI.UpdateEnvironment();
        }

        [ContextMenu("Apply Skybox")]
        public void ApplySkybox()
        {
            if (m_material)
            {
                m_materialWidget.UpdateMaterial();

                if (stereo && m_secondaryMaterial)
                {
                    m_secondaryMaterialWidget.UpdateMaterial();
                    UpdateSkybox(m_material, m_secondaryMaterial);
                }
                else
                {
                    UpdateSkybox(m_material);
                }
            }
            else
            {
                //Initialize();
            }
        }

        public override GameObject InstancePlaceholder()
        {
            throw new System.NotImplementedException();
        }
    }
}