using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Spaces.Core
{
    public class AvatarWidget : Widget
    {
        [System.Serializable]
        public class AvatarAnatomyAsset
        {
            public AvatarAnatomyWidget.Location part;
            public string assetID;
            public AvatarAnatomyWidget widget { get; set; }
        }

        public enum AvatarType
        {
            Standard,
            Stereo,
            Monitor,
        }

        private static AvatarWidget instance;

        public static AvatarWidget UserAvatar
        {
            get
            {
                if (!instance)
                    instance = FindObjectOfType<AvatarWidget>();

                return instance;
            }
        }

        public Constants.Platform currentPlatform { get; set; }
        public AvatarType currentType;

        public List<AvatarAnatomyAsset> assets;
        
        public Transform head { get; set; }

        public void OpenNav()
        {
            UnityClient.UserSession.Instance.OpenNavInterface(head);
        }

        public void OpenAssets()
        {
            UnityClient.UserSession.Instance.OpenAssetInterface(head);
        }

        public override void Initialize()
        {
            Debug.Log("Initializing Avatar");

            base.Initialize();
            instance = this;
            //UnityClient.UserSession.Instance.Avatar = instance;

            if (GameObject.FindObjectsOfType<SkyboxWidget>().Any(sw => sw.stereo))
                currentType = AvatarType.Stereo;

            var avatarTemplate = UnityClient.UserSession.Instance.m_settings.avatarTemplates.FirstOrDefault(t => t.type == currentType);
            var avatarInstance = Instantiate(avatarTemplate.prefab, transform, false);
            avatarInstance.AddComponent<InstancedAssetWidget>();

            foreach (var part in GetComponentsInChildren<AvatarAnatomyWidget>())
            {
                foreach (var asset in assets.Where(p => p.part == part.part))
                {
                    asset.widget = part;
                    var thisWidget = part.gameObject.AddComponent<AssetWidget>();
                    thisWidget.assetID = asset.assetID;
                }

                if (part.part == AvatarAnatomyWidget.Location.Head)
                    head = part.transform;

            }

            if (!head)
                head = transform;

        }
    }
}