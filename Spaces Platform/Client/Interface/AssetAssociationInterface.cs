using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Spaces.Core;
using Spaces.Core.RestAPI;

namespace Spaces.UnityClient
{
    public class AssetAssociationInterface : AssetSelectionInterface
    {
        public UnityEngine.Events.UnityEvent OnAssetRemoved;
        private Color defaultColor;

        protected override void Start()
        {
            base.Start();

            var prefabText = AssetEntryPrefab.GetComponentInChildren<Text>();

            if (prefabText)
                defaultColor = prefabText.color;
        }

        protected override void Asset_Click(Button sender, Asset asset)
        {
            if (UserSession.Instance && UserSession.Instance.CurrentSpace != null)
            {
                UserSession.Instance.CurrentSpace.AddAsset(asset);
                SetButtonDisabled(sender);
            }

            OnAssetSelected.Invoke();
        }

        private void Asset_Remove(Button sender, Asset asset)
        {
            if (UserSession.Instance && UserSession.Instance.CurrentSpace != null)
            {
                UserSession.Instance.CurrentSpace.RemoveAsset(asset);
                var button = sender.transform.parent.GetComponent<Button>();

                if (button)
                    SetButtonEnabled(button);
            }

            OnAssetRemoved.Invoke();
        }

        protected override void InitalizeAssetInfo(Asset asset)
        {
            GameObject assetEntry = Instantiate(AssetEntryPrefab, AssetsPanel.transform) as GameObject;

            assetEntry.GetComponentInChildren<Text>().text = asset.name;
            var button = assetEntry.GetComponent<Button>();

            if (button)
                button.onClick.AddListener(delegate { Asset_Click(button, asset); });

            foreach (Transform child in assetEntry.transform)
            {
                var removeButton = child.GetComponent<Button>();

                if (removeButton)
                {
                    removeButton.onClick.AddListener(delegate { Asset_Remove(removeButton, asset); });
                    child.gameObject.SetActive(false);
                }
            }

            assetEntry.SetActive(true);

            if (UserSession.Instance.CurrentSpace != null && UserSession.Instance.CurrentSpace.assetIDs.Contains(asset.id))
                SetButtonDisabled(button);
        }


        private void SetButtonDisabled(Button button)
        {
            button.interactable = false;
            var text = button.GetComponentInChildren<Text>();

            if (text)
                text.color = Color.grey;

            foreach (Transform child in button.transform)
            {
                if (child.GetComponent<Button>())
                    child.gameObject.SetActive(true);
            }
        }

        private void SetButtonEnabled(Button button)
        {
            button.interactable = true;
            var text = button.GetComponentInChildren<Text>();

            if (text)
                text.color = defaultColor;

            foreach (Transform child in button.transform)
            {
                if (child.GetComponent<Button>())
                    child.gameObject.SetActive(false);
            }
        }
    }
}
