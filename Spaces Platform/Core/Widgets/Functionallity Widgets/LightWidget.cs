using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spaces.Core
{
    public class LightWidget : Widget
    {
        [System.Serializable]
        public class Shadow
        {
            public LightShadows shadows = LightShadows.None;
            public int shadowCustomResolution;
            public float shadowBias = 0.05f;
            public float shadowNormalBias = 0.4f;
            public float shadowNearPlane = 0.2f;
            public UnityEngine.Rendering.LightShadowResolution shadowResolution = UnityEngine.Rendering.LightShadowResolution.FromQualitySettings;
            [Range(0, 1)] public float shadowStrength = 1;
        }

        public LightType type = LightType.Point;

        public float range = 10;
        public float spotAngle = 30;
        public Color color = Color.white;

        [Range(0, 8)]
        public float intensity = 1;
        public float bounceIntensity = 1;

        // areaSize The size of the area light.Editor only.
        // bakedIndex  A unique index, used internally for identifying lights contributing to lightmaps and/or light probes.

        // flare The flare asset to use for this light.
        // isBaked Is the light contribution already stored in lightmaps and/or lightprobes (Read Only).
        // lightmappingMode This property controls whether this light only affects lightmap baking, or dynamic objects, or both.

        public Shadow shadows;
        public TextureProperty cookie;
        public float cookieSize = 10;

        public bool drawHalo = false;
        public LightRenderMode renderMode = LightRenderMode.Auto;
        public int cullingMask = -1;

        private Light m_light;

        public override void Initialize()
        {
            base.Initialize();
            Initialize(m_light);
        }

        public void Initialize(Light light)
        {
            base.Initialize();

            if (light)
            {
                m_light = light;
            }

            GetLight();
        }

        public void GetLight()
        {
            if (!m_light)
                m_light = GetComponentInChildren<Light>();

            if (m_light)
            {
                UpdateWidget();
            }
            else
            {
                m_light = gameObject.AddComponent<Light>();
                UpdateLight();
            }
        }

        private void SetCookie()
        {
            if (type == LightType.Point)
            {
                var cubemap = new Cubemap(cookie.value.height, TextureFormat.ARGB32, false);

                cubemap.anisoLevel = cookie.anisoLevel;
                cubemap.wrapMode = cookie.wrapMode;
                var pixels = ((Texture2D)cookie.value).GetPixels();

                for (int face = 0; face < 6; face++)
                {
                    cubemap.SetPixels(pixels, (CubemapFace)face);
                }

                cubemap.Apply();
                m_light.cookie = cubemap;
            }
            else
            {
                m_light.cookie = cookie.value;
            }
        }

        public void UpdateLight()
        {
            if (!m_light)
            {
                Debug.LogWarning(this.name + " [No Light To Update]");
                return;
            }

            m_light.type = type;
            m_light.range = range;
            m_light.spotAngle = spotAngle;
            m_light.color = color;

            m_light.intensity = intensity;
            m_light.bounceIntensity = bounceIntensity;

            // shadows
            m_light.shadows = shadows.shadows;
            m_light.shadowCustomResolution = shadows.shadowCustomResolution;
            m_light.shadowBias = shadows.shadowBias;
            m_light.shadowNormalBias = shadows.shadowNormalBias;
            m_light.shadowNearPlane = shadows.shadowNearPlane;
            m_light.shadowResolution = shadows.shadowResolution;
            m_light.shadowStrength = shadows.shadowStrength;

            cookie.Fetch(this, SetCookie);

            m_light.cookieSize = cookieSize;
            m_light.renderMode = renderMode;
            m_light.cullingMask = cullingMask;

            DynamicGI.UpdateEnvironment();
        }

        public void UpdateWidget()
        {
            UpdateWidget(m_light);
        }

        public void UpdateWidget(Light light)
        {
            if (!light)
            {
                //Debug.LogWarning(this.name + " [Light is Null]");
                ResetToDefaults();
                return;
            }


            type = light.type;
            range = light.range;
            spotAngle = light.spotAngle;
            color = light.color;

            intensity = light.intensity;
            bounceIntensity = light.bounceIntensity;

            // shadows
            shadows.shadows = light.shadows;
            shadows.shadowCustomResolution = light.shadowCustomResolution;
            shadows.shadowBias = light.shadowBias;
            shadows.shadowNormalBias = light.shadowNormalBias;
            shadows.shadowNearPlane = light.shadowNearPlane;
            shadows.shadowResolution = light.shadowResolution;
            shadows.shadowStrength = light.shadowStrength;

            cookieSize = light.cookieSize;
            renderMode = light.renderMode;
            cullingMask = light.cullingMask;
        }

        private void ResetToDefaults()
        {
            range = 10;
            spotAngle = 30;

            color = new Color(255 / 255f, 244 / 255f, 214 / 255f, 255 / 255f);

            //color.r = 255/255f;
            //color.g = 244/255f;
            //color.b = 214/255f;
            //color.a = 255/255f;

            intensity = 1;
            bounceIntensity = 1;
            shadows = new Shadow();
            renderMode = LightRenderMode.Auto;
            cullingMask = -1;
        }
    }
}