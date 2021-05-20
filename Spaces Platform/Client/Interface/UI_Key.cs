using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;


namespace Spaces.UnityClient
{
    public class UI_Key : MonoBehaviour
    {
        [System.Serializable]
        public class KeyMapping
        {
            public UI_Keyboard.KeyMode mode;
            public string value;
            public string display;

            public KeyMapping(UI_Keyboard.KeyMode mode, string value, string display = "")
            {
                this.mode = mode;
                this.value = value;
                this.display = display;
            }
        }

        [System.Serializable]
        public class KeyMap
        {
            public List<KeyMapping> keyMappings;

            public string this[UI_Keyboard.KeyMode mode]
            {
                get
                {
                    var mapping = keyMappings.FirstOrDefault(m => m.mode == mode);

                    return mapping != null ? mapping.value : string.Empty;
                }
                set
                {
                    var mapping = keyMappings.FirstOrDefault(m => m.mode == mode);

                    if (mapping != null)
                    {
                        mapping.value = value;
                    }
                    else
                    {
                        keyMappings.Add(new KeyMapping(mode, value));
                    }
                }
            }

            public bool HasMapping(UI_Keyboard.KeyMode mode)
            {
                return keyMappings.Any(m => m.mode == mode);
            }

            public string GetDisplay(UI_Keyboard.KeyMode mode)
            {
                var mapping = keyMappings.FirstOrDefault(m => m.mode == mode);

                if (mapping == null)
                    return "□";
                else
                    return string.IsNullOrEmpty(mapping.display) ? mapping.value : mapping.display;
            }
        }

        public KeyMap keyMap;
        private UI_Keyboard.KeyMode mode;
        private Text keytext;

        public string value
        {
            get
            {
                return keyMap[mode];
            }
        }

        void Start()
        {
            keytext = GetComponentInChildren<Text>();
        }

        public void UpdateMode(UI_Keyboard.KeyMode mode)
        {
            if (keyMap.HasMapping(mode))
            {
                this.mode = mode;

                if (!keytext)
                    keytext = GetComponentInChildren<Text>();

                if (keytext)
                    keytext.text = keyMap.GetDisplay(mode);
                else
                    Debug.LogError(this.name + " [Key Text component missing]");
            }
        }
    }
}