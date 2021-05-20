using UnityEngine;
using System.Linq;

namespace Spaces.Core.RestAPI
{
    [CreateAssetMenu(menuName = "Spaces Platform Settings")]
    public class SpacesPlatformServicesSettings : ScriptableObject
    {
        public string TokenDescription;
        public string Token;
        public string ServiceDomain;

        private static SpacesPlatformServicesSettings settings;
        public static SpacesPlatformServicesSettings Settings
        {
            get
            {
                if (!settings)
                    settings = GetDefaultPlatformSettings();

                return settings;
            }
            internal set
            {
                settings = value;
            }
        }

        public static string GetAuthToken()
        {
            return Settings.Token;
        }

        private static string ServicesDomain
        {
            get
            {
                return Settings.ServiceDomain;
            }
        }

        public static SpacesPlatformServicesSettings GetDefaultPlatformSettings()
        {
            var settings = Resources.LoadAll<SpacesPlatformServicesSettings>("").FirstOrDefault();

            if (settings)
                return settings;
            else
                return new SpacesPlatformServicesSettings()
                {
                    TokenDescription = "Generic Default Settings",
                    Token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJhcGkuc3BhY2VzLmNvbSIsImlhdCI6MTQ3MDg3MDY2OSwiZXhwIjoxNTAyNDA2NjY5LCJhdWQiOiJhcGkuc3BhY2VzLmNvbSIsInN1YiI6ImdlbmVyaWMiLCJpZCI6IjdCNEIzODRELURFM0YtNEEzRi05RDhBLTBERDY0MDY2MEJDQSJ9.KCOXODLYoKJQJhg99Lu8hmW5eLJAc-JQsaSAUKlICt0",
                    ServiceDomain = @"http://api.spaces.com"
                };
        }

        public static void Refresh()
        {
            settings = null;
        }

        public void SetDefault()
        {
            settings = this;
        }

        public void SaveSettings()
        {
            PlayerPrefs.SetString(Constants.PLATFORM_SERVICES_SETTINGS, JsonUtility.ToJson(this));
            PlayerPrefs.Save();
        }

        public void LoadSavedSettings()
        {
            if (PlayerPrefs.HasKey(Constants.PLATFORM_SERVICES_SETTINGS))
                JsonUtility.FromJsonOverwrite(PlayerPrefs.GetString(Constants.PLATFORM_SERVICES_SETTINGS), this);
        }       
        
        //public void SaveToken()
        //{
        //    if (!string.IsNullOrEmpty(Token))
        //    {
        //        PlayerPrefs.SetString(Constants.AUTH_TOKEN, Token);
        //        PlayerPrefs.Save();
        //    }
        //}

        //public void SaveDomain()
        //{
        //    if (!string.IsNullOrEmpty(ServiceDomain))
        //    {
        //        PlayerPrefs.SetString(Constants.REST_DOMAIN, ServiceDomain);
        //        PlayerPrefs.Save();
        //    }
        //}

        //public void ClearToken()
        //{
        //    PlayerPrefs.DeleteKey(Constants.AUTH_TOKEN);
        //    PlayerPrefs.Save();
        //}


    }
}
