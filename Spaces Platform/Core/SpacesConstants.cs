using UnityEngine;

namespace Spaces.Core
{
    public class Constants
    {
        #region PlayerPref Keys
        //public const string AUTH_TOKEN = "AuthenticationToken";
        public const string CURRENT_USER = "CurrentUser";
        public const string CURRENT_SPACE = "CurrentSpace";
        //public const string LOCAL_USERS = "LocalUsers";
        public const string SPACES_LIST = "SpacesList";
        public const string ASSETS_LIST = "AssetsList";
        //public const string REST_DOMAIN = "RestAPIDomain";
        public const string PLATFORM_SERVICES_SETTINGS = "PlatformServicesSettings";
        #endregion        

        #region Local Directories
        public static string localDataPath = Application.persistentDataPath;

        public static string SPACES_BUNDLES = string.Format(@"{0}/{1}", localDataPath, "AssetBundles");
        public static string CACHE = string.Format(@"{0}/{1}", localDataPath, "AssetCache");
        public static string SPACES_LOCAL = string.Format(@"{0}/{1}", localDataPath, "AssetCache/Local");
        public static string SPACES_CACHE = string.Format(@"{0}/{1}", localDataPath, "AssetCache/Spaces");
        public static string ASSETS_CACHE = string.Format(@"{0}/{1}", localDataPath, "AssetCache/Assets");

#if UNITY_EDITOR
        public static string SESSION_SETTINGS = string.Format(@"DebugSessionSettings");
#else
        public static string SESSION_SETTINGS = string.Format(@"_DefaultSessionSettings");
#endif

        #endregion

        #region Default User
        public const string GUEST_USER_NAME = "Guest";
        public const string GUEST_FIRST_NAME = "Guest";
        public const string GUEST_MIDDLE_INITIAL = "Q";
        public const string GUEST_LAST_NAME = "User";
        public const string GUEST_ID = "00000000-0000-0000-0000-000000000000";
        #endregion

        public static Spaces.Core.User GetDefaultUser()
        {
            Spaces.Core.User rando = new Spaces.Core.User()
            {
                //created = System.DateTime.Now,
                firstName = Spaces.Core.Constants.GUEST_FIRST_NAME,
                lastName = Spaces.Core.Constants.GUEST_LAST_NAME,
                middleName = Spaces.Core.Constants.GUEST_MIDDLE_INITIAL,
                name = Spaces.Core.Constants.GUEST_USER_NAME,
                //lastLogin = System.DateTime.Now
            };

            return rando;
        }

        public enum AssetType
        {
            compound,
            assetbundle,// DEPRECATED
            scene,      // DEPRECATED
            model,
            video,
            image,
            audio,
            template
        }

        public enum Platform
        {
            win,
            ios,
            droid
        }

        public enum DisplayFormat
        {
            Standard2D,
            //Stereo2D,
            Spherical,
            //Hemispherical,
            Cubemap,
        }

        public enum FrameFormat
        {
            Standard,
            Equirectangular,
            Cubic,
        }

        public enum StereoFormat
        {
            LeftRight,
            RightLeft,
            LeftOverUnder,
            RightOverUnder
        }
    }



}
