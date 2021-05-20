using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace Spaces.Core
{
    public class EnvironmentWidget : Widget
    {
        [System.Serializable]
        public class SkyboxSettings
        {
            public SkyboxWidget skybox;
            [Range(0, 8)]
            public float ambientIntensity = 1;
        }

        [System.Serializable]
        public class GradientAmbientColors
        {
            public Color ambientEquatorColor = new Color(29 / 255f, 32 / 255f, 34 / 255f, 255 / 255f);
            public Color ambientGroundColor = new Color(12 / 255f, 11 / 255f, 9 / 255f, 255 / 255f);
        }

        [System.Serializable]
        public class ReflectionProbeSettings
        {
            [Header("Type")]
            public ReflectionProbeMode mode = ReflectionProbeMode.Realtime;
            public ReflectionProbeRefreshMode refreshMode = ReflectionProbeRefreshMode.ViaScripting;
            public ReflectionProbeTimeSlicingMode timeSlicingMode = ReflectionProbeTimeSlicingMode.AllFacesAtOnce;

            [Header("Runtime settings")]
            public int importance;
            public float intensity;
            public bool boxProjection;
            public float blendDistance;
            public Vector3 size;
            public Vector3 center;

            [Header("Cubemap capture settings")]
            public int resolution;
            public bool hdr;
            public float shadowDistance;
            public ReflectionProbeClearFlags clearFlags = ReflectionProbeClearFlags.Skybox;
            public Color backgroundColor;
            public int cullingMask = -1;

            [Space]
            public float nearClipPlane = 0.3f;
            public float farClipPlane = 1000;

            public ReflectionProbeSettings()
            {
                Initialize();
            }

            public ReflectionProbeSettings(ReflectionProbe probe)
            {
                this.mode = probe.mode;
                this.refreshMode = probe.refreshMode;
                this.timeSlicingMode = probe.timeSlicingMode;
                this.importance = probe.importance;
                this.intensity = probe.intensity;
                this.boxProjection = probe.boxProjection;
                this.blendDistance = probe.blendDistance;
                this.size = probe.size;
                this.center = probe.center;
                this.resolution = probe.resolution;
                this.hdr = probe.hdr;
                this.shadowDistance = probe.shadowDistance;
                this.clearFlags = probe.clearFlags;
                this.backgroundColor = probe.backgroundColor;
                this.cullingMask = probe.cullingMask;
                this.nearClipPlane = probe.nearClipPlane;
                this.farClipPlane = probe.farClipPlane;
            }

            public void Initialize()
            {
                mode = ReflectionProbeMode.Realtime;
                refreshMode = ReflectionProbeRefreshMode.ViaScripting;
                timeSlicingMode = ReflectionProbeTimeSlicingMode.AllFacesAtOnce;
                importance = 1;
                intensity = 1;
                boxProjection = true;
                blendDistance = 50;
                size = Vector3.one * 100;
                center = Vector3.zero;
                resolution = 128;
                hdr = true;
                shadowDistance = 100;
                clearFlags = ReflectionProbeClearFlags.Skybox;
                backgroundColor = Color.black;
                cullingMask = -1;
                nearClipPlane = 0.3f;
                farClipPlane = 1000;
            }

            public void CopyTo(ReflectionProbe probe)
            {
                probe.mode = mode;
                probe.refreshMode = refreshMode;
                probe.timeSlicingMode = timeSlicingMode;
                probe.importance = importance;
                probe.intensity = intensity;
                probe.boxProjection = boxProjection;
                probe.blendDistance = blendDistance;
                probe.size = size;
                probe.center = center;
                probe.resolution = resolution;
                probe.hdr = hdr;
                probe.shadowDistance = shadowDistance;
                probe.clearFlags = clearFlags;
                probe.backgroundColor = backgroundColor;
                probe.cullingMask = cullingMask;
                probe.nearClipPlane = nearClipPlane;
                probe.farClipPlane = farClipPlane;
            }

            public static implicit operator ReflectionProbeSettings(ReflectionProbe probe)
            {
                return new ReflectionProbeSettings(probe);
            }
        }

        public bool applyOnStart;

        public LightWidget sun;

        // Ambient source
        public UnityEngine.Rendering.AmbientMode ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
        public UnityEngine.Rendering.SphericalHarmonicsL2 ambientProbe;

        public SkyboxSettings skyboxSettings;
        public Color ambientColor = new Color(54 / 255f, 58 / 255f, 66 / 255f, 255 / 255f);
        public GradientAmbientColors gradientAmbientColors;

        public ReflectionProbeSettings reflectionProbeSettings;
        public ReflectionProbe m_probe { get; private set; }

        // Reflection source
        public UnityEngine.Rendering.DefaultReflectionMode defaultReflectionMode = UnityEngine.Rendering.DefaultReflectionMode.Skybox;

        //[Tooltip("Custom cubemap applied with 'Custom' Reflection Mode.")]
        //public MaterialWidget.MaterialTextureProperty customReflection;
        //private Cubemap m_customReflection;

        public int defaultReflectionResolution = 128;
        [Range(0, 1)]
        public float reflectionIntensity = 1;
        [Range(1, 5)]
        public int reflectionBounces = 1;

        public float flareFadeSpeed = 3;
        public float flareStrength = 1;

        public bool fog = false;
        public FogMode fogMode = FogMode.ExponentialSquared;
        public Color fogColor = new Color(128 / 255f, 128 / 255f, 128 / 255f, 255 / 255f);
        public float fogDensity = 0.01f;
        public float fogStartDistance;
        public float fogEndDistance = 300;

        public float haloStrength = 0.5f;

        protected override void Start()
        {
            base.Start();

            if (applyOnStart)
                UpdateEnvironment();
        }

        public override void Initialize()
        {
            base.Initialize();

            if (skyboxSettings == null)
                skyboxSettings = new SkyboxSettings();

            InitializeSkybox();
            sun = GetComponentInChildren<LightWidget>();
            
            InitializeProbe();
        }

        public void InitializeSkybox()
        {
            skyboxSettings.skybox = GetComponentInChildren<SkyboxWidget>();

            if (!skyboxSettings.skybox)
                skyboxSettings.skybox = FindObjectsOfType<SkyboxWidget>().FirstOrDefault();
        }

        public void InitializeProbe()
        {
            if (!m_probe)
            {
                m_probe = GetComponent<ReflectionProbe>();

                if (!m_probe)
                    m_probe = gameObject.AddComponent<ReflectionProbe>();
            }

            reflectionProbeSettings.CopyTo(m_probe);
        }

        public void UpdateEnvironment()
        {
            if (skyboxSettings != null && skyboxSettings.skybox)
                skyboxSettings.skybox.ApplySkybox();

            RenderSettings.ambientMode = ambientMode;
            RenderSettings.ambientIntensity = skyboxSettings.ambientIntensity;
            RenderSettings.ambientSkyColor = ambientColor;
            RenderSettings.ambientEquatorColor = gradientAmbientColors.ambientEquatorColor;
            RenderSettings.ambientGroundColor = gradientAmbientColors.ambientGroundColor;

            RenderSettings.ambientProbe = ambientProbe;

            //public MaterialWidget.MaterialTextureProperty customReflection;
            //private Cubemap m_customReflection;
            RenderSettings.defaultReflectionMode = defaultReflectionMode;
            RenderSettings.defaultReflectionResolution = defaultReflectionResolution;
            RenderSettings.reflectionBounces = reflectionBounces;
            RenderSettings.reflectionIntensity = reflectionIntensity;

            RenderSettings.flareFadeSpeed = flareFadeSpeed;
            RenderSettings.flareStrength = flareStrength;

            RenderSettings.fog = fog;
            RenderSettings.fogMode = fogMode;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogDensity = fogDensity;
            RenderSettings.fogStartDistance = fogStartDistance;
            RenderSettings.fogEndDistance = fogEndDistance;

            RenderSettings.haloStrength = haloStrength;

            DynamicGI.UpdateEnvironment();
            m_probe.RenderProbe();
            new List<Renderer>(FindObjectsOfType<Renderer>()).ForEach(r => DynamicGI.UpdateMaterials(r));
        }

        //private void UpdateWidget()
        //{
        //    ambientLight = RenderSettings.ambientLight;
        //    ambientEquatorColor = RenderSettings.ambientEquatorColor;
        //    ambientGroundColor = RenderSettings.ambientGroundColor;
        //    ambientSkyColor = RenderSettings.ambientSkyColor;
        //    ambientIntensity = RenderSettings.ambientIntensity;

        //    ambientMode = RenderSettings.ambientMode;
        //    ambientProbe = RenderSettings.ambientProbe;

        //    //public MaterialWidget.MaterialTextureProperty customReflection;
        //    //private Cubemap m_customReflection;
        //    defaultReflectionMode = RenderSettings.defaultReflectionMode;
        //    defaultReflectionResolution = RenderSettings.defaultReflectionResolution;
        //    reflectionBounces = RenderSettings.reflectionBounces;
        //    reflectionIntensity = RenderSettings.reflectionIntensity;

        //    flareFadeSpeed = RenderSettings.flareFadeSpeed;
        //    flareStrength = RenderSettings.flareStrength;

        //    fog = RenderSettings.fog;
        //    fogMode = RenderSettings.fogMode;
        //    fogColor = RenderSettings.fogColor;
        //    fogDensity = RenderSettings.fogDensity;
        //    fogStartDistance = RenderSettings.fogStartDistance;
        //    fogEndDistance = RenderSettings.fogEndDistance;

        //    haloStrength = RenderSettings.haloStrength;

        //    //public SkyboxWidget skybox;
        //    //public LightWidget sun;
        //}

        public void ResetToDefaults()
        {
            ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
            skyboxSettings.ambientIntensity = 1;
            ambientColor = new Color(54 / 255f, 58 / 255f, 66 / 255f, 255 / 255f);
            gradientAmbientColors.ambientEquatorColor = new Color(29 / 255f, 32 / 255f, 34 / 255f, 255 / 255f);
            gradientAmbientColors.ambientGroundColor = new Color(12 / 255f, 11 / 255f, 9 / 255f, 255 / 255f);

            defaultReflectionMode = UnityEngine.Rendering.DefaultReflectionMode.Skybox;

            defaultReflectionResolution = 128;
            reflectionIntensity = 1;
            reflectionBounces = 1;
            flareFadeSpeed = 3;
            flareStrength = 1;

            fog = false;
            fogMode = FogMode.ExponentialSquared;
            fogColor = new Color(128 / 255f, 128 / 255f, 128 / 255f, 255 / 255f);
            fogDensity = 0.01f;
            fogStartDistance = 0;
            fogEndDistance = 300;

            haloStrength = 0.5f;
        }
    }
}