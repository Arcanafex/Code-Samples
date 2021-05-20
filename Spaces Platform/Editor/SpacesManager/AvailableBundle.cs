using UnityEngine;
using System.Collections;

namespace Spaces.Manager
{
    /// <summary>
    /// A bundle that can be uploaded.
    /// </summary>
    [System.Serializable]
    public class AvailableBundle
    {
        public enum State
        {
            Error = -1,
            Idle = 0,
            Local = 1,
            Creating = 2,
            AddingAsset = 3,
            GettingUploadPath = 4,
            Uploading = 5,
            Complete = 6,
        }

        [SerializeField]
        public State BundleState;
        [SerializeField]
        public State LastState;

        /// <summary>
        /// The path to the bundles.
        /// </summary>
        [SerializeField]
        public string PathToBundle;

        /// <summary>
        /// The file name.
        /// </summary>
        [SerializeField]
        public string BundleName;

        /// <summary>
        /// The size of the file.
        /// </summary>
        [SerializeField]
        public double Size;

        /// <summary>
        /// The name of the space when uploaded
        /// </summary>
        [SerializeField]
        public string AssetName;

        //private string currentAssetID;
        private Core.Asset currentAsset;


        /// <summary>
        /// Constant to convert from bytes to megabytes.
        /// </summary>
        public const double BYTES_TO_MEGABYTES = (1024d * 1024d);

        //public Spaces.Common.UserSpace userSpace;
        public bool isCached
        {
            get { return ((int)BundleState) > 0; }
        }

        public delegate void StateChanged(State oldState, State newState);
        public event StateChanged onStateChanged;

        /// <summary>
        /// Constructs an available bundle.
        /// </summary>
        /// <param name="argPath">Path to the bundle file.</param>
        public AvailableBundle(string argPath)
        {
            PathToBundle = argPath;
            BundleName = System.IO.Path.GetFileNameWithoutExtension(PathToBundle);
            Size = ((double)new System.IO.FileInfo(PathToBundle).Length / BYTES_TO_MEGABYTES);

            AssetName = string.Empty;
        }

        public AvailableBundle()
        { }

        public Spaces.Core.Asset CreateAsset(string assetName)
        {
            AssetName = assetName;
            UpdateState(State.Creating);

            var createdAsset = Spaces.Core.Asset.Create(assetName, Core.Constants.AssetType.assetbundle.ToString(), PathToBundle);
            //createdAsset.onStateEnd += AssetUpdated;
            currentAsset = createdAsset;

            return createdAsset;
        }

        private void AssetUpdated(Spaces.Core.Asset asset, Spaces.Core.Asset.Process[] lastState, Spaces.Core.Asset.Process newState)
        {
            //if (currentAsset != asset)
            //    currentAsset = asset;

            //if (lastState == Core.Asset.State.Creating && newState != Core.Asset.State.Error)
            //{
            //    asset.AddData(this.PathToBundle);
            //}

            //if (newState == Core.Asset.State.RequestingEndpoint)
            //    UpdateState(State.GettingUploadPath);
            //else if (newState == Core.Asset.State.Uploading)
            //    UpdateState(State.Uploading);
            //else if (lastState == Core.Asset.State.Uploading && newState == Core.Asset.State.Idle)
            //    UpdateState(State.Complete);
            //else if (newState == Core.Asset.State.Error)
            //    UpdateState(State.Error);
        }


        public void RetryFromLastState()
        {
            if (BundleState == State.Error)
            {
                switch (LastState)
                {
                    case State.Creating:
                        CreateAsset(AssetName);
                        break;

                    case State.GettingUploadPath:
                        //GetUploadPath(currentAssetID);
                        currentAsset.AddData(this.PathToBundle);
                        break;

                    case State.Uploading:
                        //GetUploadPath(currentAssetID);
                        currentAsset.AddData(this.PathToBundle);
                        break;

                    default:
                        break;
                }

            }
        }

        private void UpdateState(State newState)
        {
            if (BundleState != newState)
            {
                LastState = BundleState;
                BundleState = newState;

                if (onStateChanged != null)
                    onStateChanged(LastState, BundleState);
            }
        }

        public Spaces.Core.StatusMessage GetStatusMessage()
        {
            switch (BundleState)
            {
                case State.Error:
                    return new Spaces.Core.StatusMessage()
                    {
                        statusMessage = "Something went gone wrong while in state: " + LastState,
                        progressing = false
                    };
                case State.Idle:
                    return new Spaces.Core.StatusMessage()
                    {
                        statusMessage = "",
                        progressing = false
                    };
                case State.Creating:
                    return new Spaces.Core.StatusMessage()
                    {
                        statusMessage = "Creating Asset. Please wait",
                        progressing = true
                    };
                case State.GettingUploadPath:
                    return new Spaces.Core.StatusMessage()
                    {
                        statusMessage = "Requesting upload path. Please wait",
                        progressing = true
                    };
                case State.Uploading:
                    return new Spaces.Core.StatusMessage()
                    {
                        statusMessage = "Uploading Space. Please wait",
                        progressing = true
                    };
                case State.Complete:
                    return new Spaces.Core.StatusMessage()
                    {
                        statusMessage = "Complete.",
                        progressing = false
                    };
                default:
                    return new Spaces.Core.StatusMessage();
            }
        }
    }
}