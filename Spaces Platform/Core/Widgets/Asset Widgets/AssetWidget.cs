using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Spaces.Core
{
    public abstract class AssetHandlerWidget : Widget
    {
        protected AssetWidget m_assetWidget;
        public virtual AssetWidget assetWidget
        {
            get
            {
                return m_assetWidget;
            }
        }

        public virtual void OnInstantiateAsset() { }
        public virtual void Initialize(AssetWidget assetWidget)
        {
            base.Initialize();
            m_assetWidget = assetWidget;
        }
        public abstract GameObject InstancePlaceholder();
    }

    public class AssetWidget : Widget, IProgressive, IAssetReference
    {
        public string assetID;
        protected Asset m_asset;
        public virtual Asset Asset { get { return m_asset; } }

        protected float progress;
        protected string progressMessage;
        protected AssetHandlerWidget m_handler;

        public UnityEvent OnAssetInstanced { get; set; }
        public SpaceEvent onAssetInstanced;

        protected List<Transform> instancedAssets;
        public virtual List<Transform> InstancedAssets
        {
            get
            {
                if (instancedAssets == null)
                    instancedAssets = new List<Transform>();

                // TODO: return this as a read only list.
                return instancedAssets;
            }
        }

        public bool GraphSetBySpace { get; set; }

        protected virtual bool isSubscribedToAsset { get; private set; }

        protected virtual void SubscribeToAsset()
        {
            if (m_asset == null)
                return;

            if (!isSubscribedToAsset)
            {
                isSubscribedToAsset = true;
                m_asset.onLoadAssetDone += GetAssetHandler;
            }
        }

        protected virtual void UnSubscribeFromAsset()
        {
            if (m_asset == null)
                return;

            if (isSubscribedToAsset)
            {
                isSubscribedToAsset = false;
                m_asset.onLoadAssetDone -= GetAssetHandler;
            }
        }

        public virtual bool IsInstanceAsset(Transform transform)
        {
            return InstancedAssets.Contains(transform);
        }

        protected virtual void OnDestroy()
        {
            UnSubscribeFromAsset();
        }

        public override void Initialize()
        {
            Initialize(m_asset);
        }

        public virtual void Initialize(Asset asset)
        {
            base.Initialize();
            SetAsset(asset);

            if (OnAssetInstanced == null)
                OnAssetInstanced = new UnityEvent();

            GetAsset();
        }

        public virtual void SetAsset(Asset asset)
        {
            if (asset == null)
            {
                Debug.LogWarning(this.name + " [Setting Null Asset]");
                return;
            }

            assetID = asset.id;

            if (m_asset != null)
                m_asset = asset;
        }

        protected virtual void GetAsset()
        {
            if (m_asset == null)
            {
                if (!string.IsNullOrEmpty(assetID))
                {
                    m_asset = Asset.AssetsManager.AddAsset(assetID);
                }
            }
            else
            {
                m_asset = Asset.AssetsManager.AddAsset(m_asset);
            }

            if (m_asset != null)
            {
                // Visualizer for asset download
                if (!m_asset.isLoaded)
                {
                    var progressMeter = OpenProgressMeter();
                    progressMeter.Initialize(m_asset);
                }

                SubscribeToAsset();
                StartCoroutine(m_asset.LoadAsset(this));
            }
            else
            {
                Debug.LogWarning(this.name + " [No Asset Specified]");
                GetAssetHandler(null);
            }
        }

        protected virtual void GetAssetHandler(Asset asset)
        {
            UnSubscribeFromAsset();

            if (asset != null && asset.metadata != null && asset.metadata.node != null && asset.metadata.node.localScale != Vector3.zero && !GraphSetBySpace)
            {
                // TODO: work out a better identification for legit Nodes than id == 0                
                if (asset.metadata.node.id != 0)
                    asset.metadata.node.RenderToGameObjectHierarchy(gameObject);

                if (string.IsNullOrEmpty(gameObject.name))
                    gameObject.name = asset.name;
            }

            //TODO: decide whether assetType should override existing type.
            if (gameObject && !m_handler && asset != null && !string.IsNullOrEmpty(asset.assetType))
            {
                switch (asset.assetType.ToLower())
                {
                    case "model":
                    case "fbx":
                        var modelWidget = gameObject.GetComponent<ModelWidget>() ? gameObject.GetComponent<ModelWidget>() : gameObject.AddComponent<ModelWidget>();
                        modelWidget.Initialize(this);
                        m_handler = modelWidget;
                        break;
                    case "audio":
                        var soundWidget = gameObject.GetComponent<SoundWidget>() ? gameObject.GetComponent<SoundWidget>() : gameObject.AddComponent<SoundWidget>();
                        soundWidget.Initialize(this);
                        m_handler = soundWidget;
                        break;
                    case "movfile":
                    case "video":
                        var videoWidget = gameObject.GetComponent<VideoWidget>() ? gameObject.GetComponent<VideoWidget>() : gameObject.AddComponent<VideoWidget>();
                        videoWidget.Initialize(this);
                        m_handler = videoWidget;
                        break;
                    case "image":
                    case "jpg":
                    case "jpeg":
                        var imageWidget = gameObject.GetComponent<ImageWidget>() ? gameObject.GetComponent<ImageWidget>() : gameObject.AddComponent<ImageWidget>();
                        imageWidget.Initialize(this);
                        m_handler = imageWidget;
                        break;
                    case "text":
                        var textWidget = gameObject.GetComponent<TextWidget>() ? gameObject.GetComponent<TextWidget>() : gameObject.AddComponent<TextWidget>();
                        textWidget.Initialize(this);
                        m_handler = textWidget;
                        break;
                    case "material":
                        //var materialWidget = gameObject.GetComponent<MaterialWidget>() ? gameObject.GetComponent<MaterialWidget>() : gameObject.AddComponent<MaterialWidget>();
                        //materialWidget.Initialize(this);
                        //m_handler = materialWidget;
                        break;
                    case "shader":
                        // Do something different here.
                        var shaderHandler = gameObject.GetComponent<MaterialWidget>() ? gameObject.GetComponent<MaterialWidget>() : gameObject.AddComponent<MaterialWidget>();
                        m_handler = shaderHandler;
                        break;
                    case "skybox":
                        var skyboxWidget = gameObject.GetComponent<SkyboxWidget>() ? gameObject.GetComponent<SkyboxWidget>() : gameObject.AddComponent<SkyboxWidget>();
                        skyboxWidget.Initialize(this);
                        m_handler = skyboxWidget;
                        break;
                    case "compound":
                    case "assetbundle":
                    case "scene":
                        break;
                    default:
                        break;

                }
            }

            if (asset != null && !asset.InProcess(Asset.Process.Error) && m_handler)
            {
                asset.EnqueueHandlerRequest(m_handler);
            }
            else
            {
                if (!m_handler)
                {
                    var modelWidget = gameObject.AddComponent<ModelWidget>();
                    modelWidget.Initialize(this);
                    m_handler = modelWidget;
                }

                var placeholder = m_handler.InstancePlaceholder();
                AddAssetInstance(placeholder.transform);
                placeholder.AddComponent<InstancedAssetWidget>();
            }
        }

        public void AddAssetInstance(Transform instancedTransform)
        {
            InstancedAssets.Add(instancedTransform);
            instancedTransform.gameObject.AddComponent<InstancedAssetWidget>();
            OnAssetInstanced.Invoke();
        }

        public virtual GameObject InstancePlaceholder()
        {
            var assetHandler = GetComponent<AssetHandlerWidget>();

            GameObject placeholder = assetHandler ? assetHandler.InstancePlaceholder() : gameObject.AddComponent<ModelWidget>().InstancePlaceholder();

            return placeholder;
        }

        public void ReloadAsset()
        {
            SubscribeToAsset();
            m_asset.ReloadAsset(this);
        }

        private ProgressMeterWidget OpenProgressMeter()
        {
            ProgressMeterWidget progressMeter = null;

            var avatar = AvatarWidget.UserAvatar;

            //Android the Camera.main doesn't work the same?
            Transform observer = avatar ? avatar.head : Camera.main.transform;

            Quaternion uiRot = Quaternion.LookRotation(-transform.forward, Vector3.up);

            var meterGO = UnityClient.UserSession.Instance.GetInstance("ProgressMeter", this.transform);

            if (meterGO)
            {
                meterGO.transform.rotation = uiRot;
                progressMeter = meterGO.GetComponent<ProgressMeterWidget>();
            }

            return progressMeter;
        }

        public string GetProgressMessage()
        {
            return progressMessage;
        }

        public float GetProgress()
        {
            return progress;
        }

        public string[] GetAssetIDs()
        {
            if (string.IsNullOrEmpty(assetID))
                return new string[0];
            else
                return new string[]{ assetID };
        }
    }
}