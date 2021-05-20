using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Spaces.UnityClient
{
    public class SpaceNameInterface : MonoBehaviour
    {
        public InputField Input;
        public Text ConfirmationText;

        public void Close()
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
        }

        public void UpdateSpaceName()
        {
            ConfirmationText.text = Input.text;   
        }

        public void CreateSpace()
        {
            var space = Core.Space.Create(Input.text);
            space.Enter(Core.AvatarWidget.UserAvatar);
        }
    }
}