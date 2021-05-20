using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Spaces.UnityClient
{
    public class AssetSelectionInterface : MonoBehaviour
    {
        public VerticalLayoutGroup AssetsPanel;
        public GameObject AssetEntryPrefab;
        //public string[] TagFilter;

        public UnityEvent OnAssetSelected;
        public UnityEvent OnAssetListUpdated;

        protected virtual void Start()
        {
            Refresh();
        }

        public virtual void Refresh()
        {
            Core.Asset.AssetsManager.Clear();
            Core.Asset.GetAssetList(OnGetAssetListResponse);

            foreach (Transform child in AssetsPanel.transform)
            {
                if (child.gameObject.activeInHierarchy)
                    Destroy(child.gameObject);
            }
        }

        protected virtual void OnGetAssetListResponse(bool error, Core.RestAPI.RestGetAssetListResponseData response)
        {
            if (error)
            {
                Debug.Log(this.name + " [Error] Get Asset List returned error");
            }
            else
            {
                InitializeAssetEntries();
                OnAssetListUpdated.Invoke();
            }
        }

        protected virtual void InitializeAssetEntries()
        {
            foreach (var asset in Core.Asset.AssetsManager.GetAssets())
            {
                InitalizeAssetInfo(asset);
            }
        }

        protected virtual void InitalizeAssetInfo(Core.Asset asset)
        {
            if (!string.IsNullOrEmpty(asset.id))
            {
                GameObject assetEntry = Instantiate(AssetEntryPrefab, AssetsPanel.transform) as GameObject;
                assetEntry.GetComponentInChildren<Text>().text = asset.name;

                var button = assetEntry.GetComponent<Button>();
                button.onClick.AddListener(delegate { Asset_Click(button, asset); }); ;
                assetEntry.SetActive(true);
            }
        }

        protected virtual void Asset_Click(Button sender, Core.Asset asset)
        {
            //Spawns the selected asset
            asset.SpawnAssetInstance();

            OnAssetSelected.Invoke();
        }

        public virtual void Close()
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }
}