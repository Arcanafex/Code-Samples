using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Spaces.Core
{
    /// <summary>
    /// A video clip. Will create its own player and will by default have a quad created to display it if it isn’t attached to a model widget for display.
    /// </summary>

    public class VideoWidget : AssetHandlerWidget, IDisplay<string>, IPlayable
    {
        public string m_video;
        public Constants.DisplayFormat displayFormat;
        //public bool GenerateDisplay;
        public bool AutoPlay = true;
        public bool LoopVideo = true;
        public bool RestartOnHide;
        public bool flipUVVertical = true;
        //public bool overrideMaterial;

        private AssetHandlerWidget m_display;
        //private Material displayMat;
        private bool waitingOnDisplay;
        private bool waitingOnDisplayAsset;
        private bool waitingOnAsset;
        private bool playRequested;

        private MediaPlayerInterface m_Player;

        void Update()
        {
            if (playRequested)
            {
                playRequested = false;
                Play();
            }
        }


        public override void Initialize()
        {
            base.Initialize();

            // Create MediaPlayerInterface instance.
            m_Player = MediaPlayerInterface.CreateMediaPlayerInterface();
            m_Player.Initialize(this);            

            if (LoopVideo && m_Player)
                m_Player.SetLoop(true);

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
                        defaultDisplay.Initialize(ModelWidget.Primitive.CurvedScreen);
                        existingDisplay = defaultDisplay;
                    }
                }

                if (!waitingOnDisplayAsset)
                    SetDisplay(existingDisplay);
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

                if (m_display && !waitingOnDisplay)
                    UpdateDisplay();
            }
            else
            {
                waitingOnAsset = true;
                assetWidget.OnAssetInstanced.AddListener(OnAssetReady);
            }

            Initialize();
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

                //TODO: Verify this solution is workable in a compiled build.
                //if (!displayMat)
                //    displayMat = new Material(Shader.Find("Unlit/Texture"));

                if (((IModelWidget)m_display).GetRenderedObjects().Length > 0)
                {
                    waitingOnDisplay = false;
                    //var renderer = m_display.GetComponentInChildren<Renderer>();
                    //displayMat = renderer ? renderer.material : new Material(Shader.Find("Unlit/Texture")); ;

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
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            //if (overrideMaterial)
            //    ((IModelWidget)m_display).SetMaterial(displayMat);

            if (m_Player)
            {
                m_Player.SetTargetMaterials(((IModelWidget)m_display).GetRenderedObjects());

            #if UNITY_STANDALONE_WIN
                if (flipUVVertical)
                    m_Player.FlipUVsVertically();
            #endif

                playRequested = AutoPlay;
            }
        }

        private void SetToBlack()
        {
            if (m_Player)
                m_Player.SetToBlank();
        }

        public void SetContent(string url)
        {
            m_video = url;
        }

        public void Play()
        {
            if (!string.IsNullOrEmpty(m_video))
            {
                playRequested = false;

                if (m_Player)
                    m_Player.Play(m_video);
            }
            else
            {
                playRequested = true;
            }
        }

        public void Pause()
        {
            if (m_Player)
                m_Player.Pause();
        }

        public void Stop()
        {
            if (m_Player)
                m_Player.Stop();

            SetToBlack();
        }

        public void Restart()
        {
            if (m_Player)
                m_Player.Restart();
        }

        public override GameObject InstancePlaceholder()
        {
            //return new GameObject(this.name + " [Video Placeholder]");
            SetContent("Spaces_logo_canyon_alpha720.mov");
            return m_display.gameObject;
        }
    }
}