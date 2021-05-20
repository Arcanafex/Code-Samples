using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Spaces.Core
{
    public class MaterialWidget : AssetHandlerWidget, IAssetReference
    {
        [System.Serializable]
        public abstract class MaterialProperty : ShaderInterface.ShaderProperty
        {
        }

        [System.Serializable]
        public class MaterialFloatProperty : MaterialProperty
        {
            public float value;
        }

        [System.Serializable]
        public class MaterialRangeProperty : MaterialProperty
        {
            public float value;
        }

        [System.Serializable]
        public class MaterialColorProperty : MaterialProperty
        {
            public Color value;
        }

        [System.Serializable]
        public class MaterialVectorProperty : MaterialProperty
        {
            public Vector4 value;
        }

        [System.Serializable]
        public class MaterialTextureProperty : MaterialProperty, IProgressive
        {
            public enum SourceMode
            {
                Asset,
                URL
            }

            public string assetId;
            public string url;
            public SourceMode mode = SourceMode.Asset;
            [Range(1,9)] public int anisoLevel = 1;
            public TextureWrapMode wrapMode;
            public Vector2 tiling = Vector2.one;
            public Vector2 offset = Vector2.zero;
            public Asset asset { get; set; }
            public Texture value { get; set; }
            public bool downloading { get; private set; }
            public bool isDone { get; private set; }
            public float progress { get; private set; }
            public string progressMessage { get; private set; }

            public IEnumerator FetchAsset(MaterialWidget widget, Material target)
            {
                if (!string.IsNullOrEmpty(assetId))
                {
                    asset = Asset.AssetsManager.AddAsset(assetId);

                    if (asset != null)
                    {
                        if (asset.texture)
                        {
                            value = asset.texture;
                        }
                        else
                        {
                            asset.onProcessEnd += OnAssetUpdate;

                            if (!asset.isLoaded && !asset.texture)
                                yield return widget.StartCoroutine(asset.LoadAssetInBackground(widget));

                            while (!value && !asset.InProcess(Asset.Process.Error))
                                yield return null;                            
                        }

                        if (value)
                        {
                            CorrectTexture(value);
                            target.SetTexture(name, value);
                        }
                    }
                }
            }

            public IEnumerator FetchTexture(Material target)
            {
                downloading = true;

                using (var request = UnityEngine.Networking.UnityWebRequest.GetTexture(url))
                {
                    var async = request.Send();
                    progressMessage = "Downloading";

                    while (!async.isDone)
                    {
                        yield return async.isDone;
                        isDone = async.isDone;
                        progress = async.progress;
                    }

                    if (request.isError)
                    {
                        Debug.Log(request.error);
                        progressMessage = "Error";
                    }
                    else
                    {
                        value = ((UnityEngine.Networking.DownloadHandlerTexture)request.downloadHandler).texture;
                        progressMessage = "Done";
                    }

                }

                isDone = true;
                downloading = false;

                if (value && target.HasProperty(name))
                {
                    CorrectTexture(value);
                    target.SetTexture(name, value);
                }
            }

            protected void OnAssetUpdate(Asset sender, Asset.Process[] currentProcesses, Asset.Process endingProcess)
            {
                if (endingProcess == Asset.Process.Downloading)
                {
                    value = sender.texture;
                    asset.onProcessEnd -= OnAssetUpdate;
                }
            }

            public string GetProgressMessage()
            {
                return progressMessage;
            }

            public float GetProgress()
            {
                return progress;
            }

            public void CorrectTexture(Texture texture)
            {
                texture.wrapMode = wrapMode;
            }
        }

        public string shaderAssetID;
        //private Asset shaderAsset;

        public string defaultShader;
        public string shaderName;
        private ShaderInterface m_shaderInterface;
        public string materialName;
        public Material m_material { get; private set; }

        private AssetHandlerWidget m_display;
        private bool waitingOnDisplay;
        private bool waitingOnDisplayAsset;
        private bool waitingOnShader;

        public List<string> keywords;
        private List<string> lastKeywords;
        private List<string> removedKeywords;

        public List<MaterialFloatProperty> floats;
        public List<MaterialRangeProperty> ranges;
        public List<MaterialColorProperty> colors;
        public List<MaterialVectorProperty> vectors;
        public List<MaterialTextureProperty> textures;
        
        private void Awake()
        {
            keywords = new List<string>();
            floats = new List<MaterialFloatProperty>();
            ranges = new List<MaterialRangeProperty>();
            colors = new List<MaterialColorProperty>();
            vectors = new List<MaterialVectorProperty>();
            textures = new List<MaterialTextureProperty>();
        }

        public override void Initialize()
        {
            base.Initialize();

            GetDisplay();
        }

        public void Initialize(Material material)
        {
            base.Initialize();

            if (material != null)
            {
                //Debug.Log(this.name + " [Initializing Material] ");// + (m_material ? m_material.name : " NO MATERIAL") + " - " + (m_material.shader ? m_material.shader.name : " NO SHADER"));

                m_material = material;
                materialName = material.name;

            }
            //else
            //{
            //    Debug.Log(this.name + " [Initializing Null Material]");
            //}

            GetDisplay();
        }

        public void GetDisplay()
        {
            if (m_display)
                return;

            var targetDisplay = GetComponentsInParent<AssetHandlerWidget>().FirstOrDefault(m => m is IModelWidget || m is SkyboxWidget || m is ParticleWidget);

            if (!targetDisplay)
            {
                targetDisplay = gameObject.AddComponent<ModelWidget>();
                ((ModelWidget)targetDisplay).type = ModelWidget.Type.Primitive;
                ((ModelWidget)targetDisplay).primitive = ModelWidget.Primitive.Sphere;
                targetDisplay.Initialize();
            }

            SetDisplay(targetDisplay);
        }

        //public void GetMaterial()
        //{
        //    var renderer = !m_display ? m_display.GetComponentInChildren<Renderer>() : null;
        //    var shaderInterface = ShaderInterface.GetInterface(defaultShader);

        //    if (renderer)
        //    {
        //        m_material = renderer.material;
        //        shaderInterface = ShaderInterface.GetInterface(m_material.shader);
        //    }
        //    else
        //    {
        //        m_material = new Material(Shader.Find(shaderInterface.shaderName));
        //    }

        //    SetShaderInterface(shaderInterface);
        //}

        public void SetShaderInterface(ShaderInterface shaderInterface)
        {
            if (!shaderInterface)
                return;

            m_shaderInterface = shaderInterface;

            if (m_shaderInterface)
            {
                m_shaderInterface.shader = Shader.Find(m_shaderInterface.shaderName);

                if (!m_material)
                {
                    m_material = new Material(m_shaderInterface.shader);

                    if (!string.IsNullOrEmpty(materialName))
                        m_material.name = materialName;
                }
            }
            //    if (Debug.isDebugBuild) Debug.Log(this.name + " [Shader Interface] " + m_shaderInterface.name + " (" + shaderInterface.shaderName + ")");
            //else if (Debug.isDebugBuild) 
            //    Debug.Log(this.name + " [Shader Interface NULL]");

            InitializeShaderInterface();
        }

        private void InitializeShaderInterface()
        {
            if (m_shaderInterface)
            {
                shaderName = m_shaderInterface.shaderName;

                var shaderDef = UnityClient.UserSession.Instance.m_settings.shaders.FirstOrDefault(s => s.shader.name == shaderName);

                if (shaderDef != null)
                    defaultShader = shaderDef.name;

                foreach (var item in m_shaderInterface.properties)
                {
                    switch (item.type)
                    {
                        case ShaderInterface.ShaderPropertyType.Float:
                            {
                                var floatProp = floats.FirstOrDefault(p => p.name == item.name);

                                if (floatProp == null)
                                {
                                    floatProp = new MaterialFloatProperty();
                                    floats.Add(floatProp);
                                }

                                JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(item), floatProp);
                                break;
                            }

                        case ShaderInterface.ShaderPropertyType.Range:
                            {
                                var rangeProp = ranges.FirstOrDefault(p => p.name == item.name);

                                if (rangeProp == null)
                                {
                                    rangeProp = new MaterialRangeProperty();
                                    rangeProp.value = item.range.def;
                                    ranges.Add(rangeProp);
                                }

                                JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(item), rangeProp);
                                break;
                            }

                        case ShaderInterface.ShaderPropertyType.Vector:
                            {
                                var vecProp = vectors.FirstOrDefault(p => p.name == item.name);

                                if (vecProp == null)
                                {
                                    vecProp = new MaterialVectorProperty();
                                    vectors.Add(vecProp);
                                }
                                JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(item), vecProp);
                                break;
                            }

                        case ShaderInterface.ShaderPropertyType.Color:
                            {
                                var colorProp = colors.FirstOrDefault(p => p.name == item.name);

                                if (colorProp == null)
                                {
                                    colorProp = new MaterialColorProperty();

                                    if (m_display is SkyboxWidget)
                                    {
                                        colorProp.value = Color.gray;
                                    }
                                            
                                    colors.Add(colorProp);
                                }

                                JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(item), colorProp);
                                break;
                            }

                        case ShaderInterface.ShaderPropertyType.TexEnv:
                            {
                                var texProp = textures.FirstOrDefault(p => p.name == item.name);

                                if (texProp == null)
                                {
                                    texProp = new MaterialTextureProperty();
                                    textures.Add(texProp);

                                    if (m_display is SkyboxWidget)
                                        texProp.wrapMode = TextureWrapMode.Clamp;
                                }

                                JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(item), texProp);
                                break;
                            }

                        default:
                            Debug.Log(this.name + "[Unknown Property Type] " + item.name + " (" + item.type.ToString() + ")");
                            break;
                    }
                }
            }
        }

        public void SetShader(ShaderInterface shaderInterface)
        {
            m_shaderInterface = shaderInterface;
            m_material.shader = m_shaderInterface.shader;
            shaderName = m_shaderInterface.shaderName;
        }

        public void SetColor(string propertyName, Color color)
        {
            var property = colors.FirstOrDefault(p => p.name == propertyName);

            if (property != null)
            {
                property.value = color;

                if (m_material.HasProperty(property.name))
                    m_material.SetColor(property.name, color);
            }
        }

        public void SetFloat(string propertyName, float value)
        {
            var property = floats.FirstOrDefault(p => p.name == propertyName);

            if (property != null)
            {
                property.value = value;

                if (m_material.HasProperty(property.name))
                    m_material.SetFloat(property.name, value);
            }
        }

        public void SetVector(string propertyName, Vector3 value)
        {
            var property = vectors.FirstOrDefault(p => p.name == propertyName);

            if (property != null)
            {
                property.value = value;

                if (m_material.HasProperty(property.name))
                    m_material.SetVector(property.name, value);
            }
        }

        public void SetTexture(string propertyName, Asset imageAsset)
        {
            var property = textures.FirstOrDefault(p => p.name == propertyName);

            if (property != null)
            {
                property.asset = imageAsset;
                property.assetId = imageAsset.id;

                if (m_material.HasProperty(property.name))
                {
                    StartCoroutine(property.FetchAsset(this, m_material));
                }
            }
        }

        public void SetTexture(string propertyName, Texture texture)
        {
            var property = textures.FirstOrDefault(p => p.name == propertyName);

            if (property != null)
            {
                property.value = texture;

                if (m_material.HasProperty(property.name))
                {
                    m_material.SetTexture(property.name, property.value);
                }
            }
        }

        public void SetDisplay(AssetHandlerWidget displayWidget)
        {
            if (displayWidget is IModelWidget || displayWidget is SkyboxWidget || displayWidget is ParticleWidget)
            {
                m_display = displayWidget;
            }
            else
            {
                Debug.LogWarning(this.name + " [Incompatible Surface] " + displayWidget.name + " cannot be initialized to display " + this.name + ".");
                return;
            }

            //if (m_display && !m_material)
            //{
            //    if (m_display is IModelWidget)
            //    {
            //        //TODO: Address issue of multiple materials
            //        if (((IModelWidget)m_display).GetRenderedObjects().Length > 0)
            //        {
            //            waitingOnDisplay = false;
            //            //GetMaterial();
            //            GenerateMaterialWidgets(m_display);
            //        }
            //        else
            //        {
            //            waitingOnDisplay = true;
            //            ((IModelWidget)m_display).SubscribeForInstancing(OnDisplayReady);
            //        }
            //    }
            //}
        }

        private void OnDisplayAssetReady(AssetWidget displayAsset)
        {
            waitingOnDisplayAsset = false;
            displayAsset.OnAssetInstanced.RemoveListener(delegate { OnDisplayAssetReady(displayAsset); });

            var model = displayAsset.GetComponents<AssetHandlerWidget>().FirstOrDefault(w => w is IModelWidget);
            SetDisplay(model);
        }

        private void OnDisplayReady(ModelWidget modelWidget)
        {
            waitingOnDisplay = false;
            modelWidget.UnsubscribeForInstancing(OnDisplayReady);

            GenerateMaterialWidgets(modelWidget);
            //GetMaterial();
        }

        private void UpdateDisplayMaterials()
        {
            if (m_material && m_display && m_display is IModelWidget)
                ((IModelWidget)m_display).SetMaterial(m_material);
        }

        public void UpdateMaterial()
        {
            //TODO: Need to implement setup of Shader Assets.
            ShaderInterface shaderInterface = null;
            
            if (!string.IsNullOrEmpty(shaderName))
                shaderInterface = ShaderInterface.GetInterface(Shader.Find(shaderName));

            if (!shaderInterface)
                shaderInterface = ShaderInterface.GetInterface(defaultShader);

                SetShaderInterface(shaderInterface);

            if (!m_shaderInterface)
            {
                //if (Debug.isDebugBuild) Debug.LogError(this.name + " [Shader Interface Null]");
                return;
            }
            //else if (Debug.isDebugBuild) 
            //{
            //    Debug.Log(this.name + " [Shader Interface Ready] " + defaultShader.ToString() + " - " + (m_shaderInterface.shader ? m_shaderInterface.shader.name : " NO SHADER"));
            //}

            m_material.shader = m_shaderInterface.shader;

            //if (Debug.isDebugBuild) Debug.Log(this.name + " [Update Widget State] " + (m_material ? m_material.name : " NO MATERIAL") + " - " + (m_material.shader ? m_material.shader.name : " NO SHADER"));


            floats.ForEach(p => { if (m_material.HasProperty(p.name)) m_material.SetFloat(p.name, p.value); });//else if (Debug.isDebugBuild) Debug.LogWarning(m_material.name + " [No Float Property] " + p.name); });
            ranges.ForEach(p => { if (m_material.HasProperty(p.name)) m_material.SetFloat(p.name, p.value); });//else if (Debug.isDebugBuild) Debug.LogWarning(m_material.name + " [No Range Property] " + p.name); });
            colors.ForEach(p => { if (m_material.HasProperty(p.name)) m_material.SetColor(p.name, p.value); });//else if (Debug.isDebugBuild) Debug.LogWarning(m_material.name + " [No Color Property] " + p.name); });
            vectors.ForEach(p => { if (m_material.HasProperty(p.name)) m_material.SetVector(p.name, p.value); });//else if (Debug.isDebugBuild) Debug.LogWarning(m_material.name + " [No Vector Property] " + p.name); });

            foreach (var texProperty in textures)
            {
                if (m_material.HasProperty(texProperty.name))
                {
                    if (!string.IsNullOrEmpty(texProperty.assetId))
                    {
                        StartCoroutine(texProperty.FetchAsset(this, m_material));
                    }
                    else if (!string.IsNullOrEmpty(texProperty.url))
                    {
                        StartCoroutine(texProperty.FetchTexture(m_material));
                    }

                    m_material.SetTextureScale(texProperty.name, texProperty.tiling);
                    m_material.SetTextureOffset(texProperty.name, texProperty.offset);
                }
            }

            DynamicGI.UpdateEnvironment();

            foreach(var renderer in m_display.GetComponentsInChildren<Renderer>())
                DynamicGI.UpdateMaterials(renderer);

            UpdateMaterialKeywords();
            
        }

        public void UpdateWidgetState()
        {
            //if (Debug.isDebugBuild) Debug.Log(this.name + " [Update Widget State] " + (m_material ? m_material.name : " NO MATERIAL") + " - " + (m_material.shader ? m_material.shader.name : " NO SHADER"));

            if (!m_material)
            {
                var renderer = m_display.GetComponentInChildren<Renderer>();

                if (renderer)
                    m_material = renderer.material;
                else
                    return;
            }
            else
            {
                foreach (string kw in m_material.shaderKeywords)
                    Debug.Log(kw);
            }

            var shaderInterface = ShaderInterface.GetInterface(m_material.shader);

            if (shaderInterface)
            {
                SetShaderInterface(shaderInterface);
            }
            else
            {
                SetShaderInterface(ShaderInterface.GetInterface(defaultShader));
            }

            UpdateWidgetKeywords();

            floats.ForEach(p => { if (m_material.HasProperty(p.name)) p.value = m_material.GetFloat(p.name); });//else if (Debug.isDebugBuild) Debug.LogWarning(m_material.name + " [No Float Property] " + p.name); });
            ranges.ForEach(p => { if (m_material.HasProperty(p.name)) p.value = m_material.GetFloat(p.name); });//else if (Debug.isDebugBuild) Debug.LogWarning(m_material.name + " [No Range Property] " + p.name); });
            colors.ForEach(p => { if (m_material.HasProperty(p.name)) p.value = m_material.GetColor(p.name); });//else if (Debug.isDebugBuild) Debug.LogWarning(m_material.name + " [No Color Property] " + p.name); });
            vectors.ForEach(p => { if (m_material.HasProperty(p.name)) p.value = m_material.GetVector(p.name); });//else if (Debug.isDebugBuild) Debug.LogWarning(m_material.name + " [No Vector Property] " + p.name); });
            textures.ForEach(p => 
            {
                if (m_material.HasProperty(p.name))
                {
                    p.value = m_material.GetTexture(p.name);
                    p.tiling = m_material.GetTextureScale(p.name);
                    p.offset = m_material.GetTextureOffset(p.name);
                }
            });//else if (Debug.isDebugBuild) Debug.LogWarning(m_material.name + " [No Texture Property] " + p.name); });
        }

        protected void UpdateMaterialKeywords()
        {
            if (m_material)
            {
                foreach (var keyword in ShaderKeywords)
                {
                    m_material.DisableKeyword(keyword);
                }

                keywords.ForEach(keyword => { if (ShaderKeywords.Contains(keyword)) m_material.EnableKeyword(keyword); });
            }
        }

        protected void UpdateWidgetKeywords()
        {
            if (m_material)
            {
                if (keywords == null)
                    keywords = new List<string>();
                else
                    keywords.Clear();

                foreach (var keyword in ShaderKeywords)
                {
                    if (m_material.IsKeywordEnabled(keyword))
                        keywords.Add(keyword);
                }
            }
        }

        // TODO: Come up with a more universal, less hacky way of determining keywords
        public static string[] ShaderKeywords
        {
            get
            {
                return new string[]
                {
                    "_NORMALMAP",
                    "_ALPHATEST_ON",
                    "_ALPHABLEND_ON",
                    "_ALPHAPREMULTIPLY_ON",
                    "_EMISSION",
                    "_PARALLAXMAP",
                    "_DETAIL_MULX2",
                    "_METALLICGLOSSMAP",
                    "_SPECGLOSSMAP"
                };
            }
        }


        //private IEnumerator FetchShaderAsset()
        //{
        //    shaderAsset = Asset.AssetsManager.AddAsset(shaderAssetID);

        //    StartCoroutine(shaderAsset.LoadAsset());
        //}


        public override GameObject InstancePlaceholder()
        {
            throw new System.NotImplementedException();
        }

        public static List<MaterialWidget> GenerateMaterialWidgets(AssetHandlerWidget displayWidget)
        {
            List<MaterialWidget> matWidgets = displayWidget.GetComponents<MaterialWidget>().ToList();
            List<MaterialWidget> usedMatWidgets = new List<MaterialWidget>();

            // TODO: Implement cleanup to destroy materials when disposing of this stuff.
            if (displayWidget is SkyboxWidget)
            {
                if (((SkyboxWidget)displayWidget).stereo)
                {
                    string shaderType = InternalShaders.Skybox;

                    if (((SkyboxWidget)displayWidget).type == SkyboxWidget.SkyboxType.Cubemap)
                    {
                        shaderType = InternalShaders.Skybox;
                    }

                    var leftMatWidget = matWidgets.FirstOrDefault(w => w.materialName == "left eye");

                    if (!leftMatWidget)
                    {
                        leftMatWidget = displayWidget.gameObject.AddComponent<MaterialWidget>();
                        leftMatWidget.materialName = "left eye";
                    }

                    leftMatWidget.defaultShader = shaderType;
                    leftMatWidget.Initialize(leftMatWidget.m_material);
                    leftMatWidget.UpdateMaterial();


                    var rightMatWidget = matWidgets.FirstOrDefault(w => w.materialName == "right eye");

                    if (!rightMatWidget)
                    {
                        rightMatWidget = displayWidget.gameObject.AddComponent<MaterialWidget>();
                        rightMatWidget.materialName = "right eye";
                    }

                    rightMatWidget.defaultShader = shaderType;
                    rightMatWidget.Initialize(rightMatWidget.m_material);
                    rightMatWidget.UpdateMaterial();

                    usedMatWidgets.Add(leftMatWidget);
                    usedMatWidgets.Add(rightMatWidget);
                }
                else
                {
                    var materialWidget = matWidgets.FirstOrDefault();

                    if (!materialWidget)
                    {
                        materialWidget = displayWidget.gameObject.AddComponent<MaterialWidget>();
                        materialWidget.materialName = "skybox";
                    }

                    switch (((SkyboxWidget)displayWidget).type)
                    {
                        case SkyboxWidget.SkyboxType.SixSided:
                            materialWidget.defaultShader = InternalShaders.Skybox;
                            break;
                        case SkyboxWidget.SkyboxType.Cubemap:
                            materialWidget.defaultShader = InternalShaders.SkyboxCubemap;
                            break;
                        case SkyboxWidget.SkyboxType.Procedural:
                            materialWidget.defaultShader = InternalShaders.SkyboxProcedural;
                            break;
                        default:
                            break;
                    }

                    
                    materialWidget.Initialize(materialWidget.m_material);
                    materialWidget.UpdateMaterial();

                    usedMatWidgets.Add(materialWidget);
                }
            }
            else
            {
                var renderers = new List<Renderer>();

                if (displayWidget is ModelWidget)
                {
                    foreach(var go in displayWidget.GetComponentsInChildren<InstancedAssetWidget>())
                    {
                        renderers.AddRange(go.GetComponentsInChildren<Renderer>());
                    }
                }
                else
                    renderers.AddRange(displayWidget.GetComponentsInChildren<Renderer>());

                for (int r = 0; r < renderers.Count; r++)
                {
                    for (int i = 0; i < renderers[r].materials.Length; i++)
                    {
                        string materialName = renderers[r].materials[i].name + "[" + r + "]";

                        if (displayWidget is ParticleWidget)
                        {
                            //TODO: This needs better handling to facilitate possible future subemitter support

                            if (i == 0)
                                materialName = "Particle_Material";
                            else
                                materialName = "Trail_Material";
                        }

                        var matWidget = matWidgets.FirstOrDefault(m => !usedMatWidgets.Contains(m) && (m.materialName.Contains(materialName) || m.materialName.Contains(renderers[r].materials[i].name) ));

                        if (matWidget)
                        {
                            if (displayWidget is ModelWidget)
                                renderers[r].materials[i].name = materialName;

                            usedMatWidgets.Add(matWidget);
                            matWidget.Initialize(renderers[r].materials[i]);
                            matWidget.UpdateMaterial();
                        }
                        else
                        {
                            matWidget = displayWidget.gameObject.AddComponent<MaterialWidget>();
                            

                            if (displayWidget is ParticleWidget)
                            {
                                //TODO: This needs better handling to facilitate possible future subemitter support
                                if (i == 0)
                                    renderers[r].materials[i].name = "Particle_Material";
                                else
                                    renderers[r].materials[i].name = "Trail_Material";

                                renderers[r].materials[i].shader = Shader.Find(ShaderInterface.GetInterface(InternalShaders.Particles).shaderName);
                            }
                            else
                            {
                                renderers[r].materials[i].name = materialName;
                            }

                            matWidget.Initialize(renderers[r].materials[i]);
                            matWidget.UpdateWidgetState();
                            usedMatWidgets.Add(matWidget);
                        }
                    }
                }
            }

            matWidgets.ForEach(m => { if (!usedMatWidgets.Contains(m)) GameObject.Destroy(m); });

            return usedMatWidgets;
        }

        public string[] GetAssetIDs()
        {
            List<string> assetIDs = new List<string>();

            textures.ForEach(t => { if (!string.IsNullOrEmpty(t.assetId)) assetIDs.Add(t.assetId); });

            if (!string.IsNullOrEmpty(shaderAssetID))
                assetIDs.Add(shaderAssetID);

            return assetIDs.ToArray();
        }

        private IEnumerator checkKeywords()
        {
            lastKeywords = keywords;

            if (removedKeywords == null)
                removedKeywords = new List<string>();

            while (this.enabled)
            {


                yield return new WaitForSeconds(0.5f);
            }
        }
    }
}