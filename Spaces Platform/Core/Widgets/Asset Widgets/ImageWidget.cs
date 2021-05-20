using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.Networking;

namespace Spaces.Core
{
    /// <summary>
    /// An image which is displayed on a default quad (like the Video Widget) unless attached to a model widget for display.
    /// </summary>

    public class ImageWidget : AssetHandlerWidget, IDisplay<Texture>, IDisplay<string>
    {
        public Texture m_image;
        public string m_url;

        private AssetHandlerWidget m_display;
        private bool waitingOnDisplay;
        private bool waitingOnAsset;
        private bool waitingOnDisplayAsset;

        public override void Initialize()
        {
            base.Initialize();

            if (!m_display)
            {
                //TODO: replace the GetComponentInParent call with a definitive algorithm for locating correct object.
                var existingDisplay = GetComponentsInParent<AssetHandlerWidget>().FirstOrDefault(w => w is IModelWidget);

                if (!existingDisplay)
                {
                    var displayAsset = GetComponentsInParent<AssetWidget>().FirstOrDefault(a => a != assetWidget);

                    if (displayAsset)
                    {
                        waitingOnDisplayAsset = true;
                        displayAsset.OnAssetInstanced.AddListener(delegate { OnDisplayAssetReady(displayAsset); });
                    }
                    else
                    {
                        var defaultDisplay = gameObject.AddComponent<ModelWidget>();
                        defaultDisplay.Initialize(ModelWidget.Primitive.Quad);
                        existingDisplay = defaultDisplay;
                    }
                }

                if (!waitingOnDisplayAsset)
                    SetDisplay(existingDisplay);
            }

            if (!string.IsNullOrEmpty(m_url) && !assetWidget)
            {
                SetContent(m_url);
            }
        }

        public override void Initialize(AssetWidget assetWidget)
        {
            if (!assetWidget)
            {
                Debug.LogWarning(this.name + " [Initializing with Null AssetWidget]");
                return;
            }

            base.Initialize(assetWidget);

            if (assetWidget.Asset.isLoaded)
            {
                waitingOnAsset = false;
                SetContent(assetWidget.Asset.texture);

                if (m_display && !waitingOnDisplay)
                    UpdateDisplay();
            }
            else
            {
                waitingOnAsset = true;
                assetWidget.OnAssetInstanced.AddListener(OnAssetReady);
            }
        }

        public void SetDisplay(AssetHandlerWidget displayWidget)
        {
            if (displayWidget is IModelWidget)
            {                
                m_display = displayWidget;
            }
            else
            {
                Debug.LogWarning(this.name + " [Incompatible Display] " + displayWidget.name + " cannot be initialized as a display.");
                return;
            }

            if (m_display)
            {
                if (((IModelWidget)m_display).GetRenderedObjects().Length > 0)
                {
                    waitingOnDisplay = false;

                    if (!waitingOnAsset)
                        UpdateDisplay();
                }
                else
                {
                    waitingOnDisplay = true;
                    ((IModelWidget)m_display).SubscribeForInstancing(OnDisplayReady);
                }
            }
        }

        private void OnAssetReady()
        {
            waitingOnAsset = false;
            assetWidget.OnAssetInstanced.RemoveListener(OnAssetReady);
            SetContent(assetWidget.Asset.texture);

            if (m_display && !waitingOnDisplay)
                UpdateDisplay();
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

            if (!waitingOnAsset)
                UpdateDisplay();
        }

        protected IEnumerator FetchImage()
        {
            waitingOnAsset = true;
            using (var request = UnityWebRequest.GetTexture(m_url))
            {
                var async = request.Send();

                var progressMeter = OpenProgressMeter();

                if (progressMeter)
                    progressMeter.Text = "Downloading";

                while (!async.isDone)
                {
                    yield return async.isDone;
                    progressMeter.Value = async.progress;
                }

                if (request.isError)
                {
                    Debug.Log(request.error);
                }
                else
                {
                    SetContent(((DownloadHandlerTexture)request.downloadHandler).texture);
                }

                progressMeter.Dispose();
            }

            waitingOnAsset = false;

            if (!waitingOnDisplay)
                UpdateDisplay();
        }

        public void SetContent(string content)
        {
            m_url = content;
            StartCoroutine(FetchImage());
        }

        public void SetContent(Texture content)
        {
            m_image = content;
        }

        public void UpdateDisplay()
        {
            ((IModelWidget)m_display).SetTexture(m_image);
        }

        public override GameObject InstancePlaceholder()
        {
            GameObject placeholder = null;

            if (UnityClient.UserSession.Instance.m_settings.GetPrefab("Image Frame"))
            {
                placeholder = Instantiate(UnityClient.UserSession.Instance.m_settings.GetPrefab("Image Frame"), transform, false) as GameObject;
            }
            else
            {
                placeholder = GameObject.CreatePrimitive(PrimitiveType.Quad);
                placeholder.transform.SetParent(transform, false);
            }

            return placeholder;
        }

        private ProgressMeterWidget OpenProgressMeter()
        {
            ProgressMeterWidget progressMeter = null;

            var avatar = AvatarWidget.UserAvatar; //UnityClient.UserSession.Instance.Avatar;
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
    }
}
