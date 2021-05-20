
    using UnityEngine;
    using UnityEngine.UI;

namespace Spaces.UnityClient
{
    public class UI_Keyboard : MonoBehaviour
    {
        public enum KeyMode
        {
            Upper = 0,
            Lower = 1,
            Accent,
            Punctuation,
            Symbol
        }

        public int mode;
        public KeyMode[] modes;

        private InputField input;
        private UI_Key[] keys;

        public delegate void updateKeyMode(KeyMode mode);
        public event updateKeyMode onUpdateKeyMode;

        public void ClickKey(string character)
        {
            input.text += character;
        }

        public void ClickKey(UI_Key key)
        {
            input.text += key.value;
        }

        public void Backspace()
        {
            if (input.text.Length > 0)
            {
                input.text = input.text.Substring(0, input.text.Length - 1);
            }
        }

        public void Enter()
        {
            Debug.Log("You've typed [" + input.text + "]");
            input.text = "";
        }

        public void NextMode()
        {
            if (modes.Length > 1)
            {
                mode = (mode + 1) % modes.Length;
                UpdateMode(modes[mode]);
            }
        }

        public void LastMode()
        {
            if (modes.Length > 1)
            {
                mode = mode - 1 < 0 ? modes.Length - 1 : mode - 1;
                UpdateMode(modes[mode]);
            }
        }

        public void UpdateMode(KeyMode mode)
        {
            if (onUpdateKeyMode != null)
                onUpdateKeyMode(mode);
        }

        private void Start()
        {
            Initialize();
        }

        public void Initialize()
        {
            if (modes.Length == 0)
            {
                mode = 0;
                modes = new KeyMode[] { (KeyMode)mode };
            }

            input = GetComponentInChildren<InputField>();

            if (keys != null)
            {
                foreach (var key in keys)
                    UnSubscribe(key);
            }

            keys = GetComponentsInChildren<UI_Key>();

            foreach (var key in keys)
                Subscribe(key);

            UpdateMode(modes[mode]);
        }

        private void Subscribe(UI_Key key)
        {
            if (key)
                onUpdateKeyMode += key.UpdateMode;
        }

        private void UnSubscribe(UI_Key key)
        {
            if (!key)
                onUpdateKeyMode -= key.UpdateMode;
        }
    }
}