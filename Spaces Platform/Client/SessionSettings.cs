using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Spaces.UnityClient
{
    [CreateAssetMenu(menuName = "Spaces Session Settings")]
    public class SessionSettings : ScriptableObject
    {
        [System.Serializable]
        public class RegisteredPrefab
        {
            public string name;
            public GameObject prefab;

            public RegisteredPrefab (string name, GameObject prefab)
            {
                this.name = name;
                this.prefab = prefab;
            }
        }

        [System.Serializable]
        public class RegisteredShader
        {
            public string name;
            public Shader shader;
            public Core.ShaderInterface shaderInterface;
        }

        [System.Serializable]
        public class AvatarTemplate
        {
            public string name;
            public Core.AvatarWidget.AvatarType type;
            public Core.Constants.Platform platform;
            public string HMD;
            public GameObject prefab;
        }

        public Core.RestAPI.SpacesPlatformServicesSettings spacesPlatformSettiings;
        public bool debugClearStoredDataOnStart;

        public List<RegisteredPrefab> prefabs;
        public List<RegisteredShader> shaders;
        public List<AvatarTemplate> avatarTemplates;

        // Do the stuff to add missing parts to a Space upon entry.
        public bool doSpaceSetup;
        public bool autoSpawnAssetsInEmptySpace;

        public string LobbySpace;
        public string DefaultSpace;

        public void InitializeDefaults()
        {
            prefabs = new List<RegisteredPrefab>()
            {
                new RegisteredPrefab("LoginUI", null),
                new RegisteredPrefab("SpaceList", null),
                new RegisteredPrefab("AssetList", null),
                new RegisteredPrefab("EventSystem", null),
                new RegisteredPrefab("Avatar", null),
                new RegisteredPrefab("ProgressMeter", null),
                new RegisteredPrefab("ModelPlaceholder", null)
            };

            shaders = new List<RegisteredShader>()
            {
                new RegisteredShader() {name = Core.InternalShaders.Standard, shader = Core.InternalShaders.GetStandard() },
                new RegisteredShader() {name = Core.InternalShaders.UnlitColor, shader = Core.InternalShaders.GetUnlitColor() },
                new RegisteredShader() {name = Core.InternalShaders.UnlitTexture, shader = Core.InternalShaders.GetUnlitTexture() },
                new RegisteredShader() {name = Core.InternalShaders.UnlitTransparent, shader = Core.InternalShaders.GetUnlitTransparent() },
                new RegisteredShader() {name = Core.InternalShaders.UnlitCutout, shader = Core.InternalShaders.GetUnlitCutout() },
                new RegisteredShader() {name = Core.InternalShaders.MobileDiffuse, shader = Core.InternalShaders.GetMobileDiffuse() },
                new RegisteredShader() {name = Core.InternalShaders.StandardDoubleSided, shader = Core.InternalShaders.GetStandardDoubleSided() },
                new RegisteredShader() {name = Core.InternalShaders.Skybox, shader = Core.InternalShaders.GetSkybox() },
                new RegisteredShader() {name = Core.InternalShaders.SkyboxCubemap, shader = Core.InternalShaders.GetSkyboxCubemap() },
                new RegisteredShader() {name = Core.InternalShaders.SkyboxProcedural, shader = Core.InternalShaders.GetSkyboxProcedural() },
                new RegisteredShader() {name = Core.InternalShaders.Particles, shader = Core.InternalShaders.GetParticles() },
                new RegisteredShader() {name = Core.InternalShaders.ParticlesAdditive, shader = Core.InternalShaders.GetParticlesAdditive() },
                new RegisteredShader() {name = Core.InternalShaders.ParticlesAlphaBlended, shader = Core.InternalShaders.GetParticlesAlphaBlended() },
                new RegisteredShader() {name = Core.InternalShaders.ParticlesAnimAlphaBlended, shader = Core.InternalShaders.GetParticlesAnimAlphaBlended() },
                new RegisteredShader() {name = Core.InternalShaders.ParticlesMultiply, shader = Core.InternalShaders.GetParticlesMultiply() },
            };

            avatarTemplates = new List<AvatarTemplate>()
            {
                new AvatarTemplate() { name = "Standard Vive", HMD = "Vive", type = Core.AvatarWidget.AvatarType.Standard, platform = Core.Constants.Platform.win },
                new AvatarTemplate() { name = "Standard Vive", HMD = "Vive", type = Core.AvatarWidget.AvatarType.Stereo, platform = Core.Constants.Platform.win },
            };
        }

        public float UIDistance = 3;

        public GameObject GetPrefab(string prefabName)
        {
            GameObject prefab = null;

            if (prefabs != null)
            {
                var regPrefab = prefabs.FirstOrDefault(p => p.name == prefabName);

                if (regPrefab != null)
                {
                    prefab = regPrefab.prefab;
                }
                else
                {
                    Debug.LogWarning("[No Registered Prefab] " + prefabName);
                }
            }

            return prefab;
        }

        public void InitializePlatformServiceSettings()
        {
            Core.RestAPI.RestManager.PlatformSettings = ScriptableObject.Instantiate<Core.RestAPI.SpacesPlatformServicesSettings>(spacesPlatformSettiings);            
        }
    }
}