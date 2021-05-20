using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Linq;


namespace Spaces.UnityClient
{
    public class SpacesNavInterface : MonoBehaviour
    {
        public VerticalLayoutGroup SpacesPanel;
        public GameObject SpaceEntryPrefab;
        public GameObject SpacesKeyboardPrefab;
        //public string[] TagFilter; 

        public UnityEvent OnSpaceSelected;
        public UnityEvent OnSpaceListUpdated;

        protected virtual void Start()
        {
            Refresh();
        }

        public virtual void Refresh()
        {
            Core.Space.SpacesManager.Clear();
            Core.Space.GetSpaceList(OnGetSpacesListResponse);

            foreach (Transform child in SpacesPanel.transform)
            {
                if (child.gameObject.activeInHierarchy)
                    Destroy(child.gameObject);
            }
        }

        protected virtual void OnGetSpacesListResponse(bool error, Core.RestAPI.RestGetSpaceListResponseData spacesData)
        {
            if (error)
            {
                Debug.LogError(this.name + " [Update Available Spaces] Something went wrong getting the Space list!");
            }
            else
            {
                InitializeSpaceEntries();
                OnSpaceListUpdated.Invoke();
            }
        }

        protected virtual void InitializeSpaceEntries()
        {
            foreach(var space in Core.Space.SpacesManager.GetSpaces())
            {
                InitializeSpaceEntry(space);
            }
        }

        protected virtual void InitializeSpaceEntry(Core.Space space)
        {
            GameObject spaceEntry = Instantiate<GameObject>(SpaceEntryPrefab, SpacesPanel.transform, false);
            spaceEntry.GetComponentInChildren<Text>().text = space.name;

            var button = spaceEntry.GetComponent<Button>();
            button.gameObject.name = space.name;
            button.onClick.AddListener(delegate { Space_Click(space); });

            spaceEntry.SetActive(true);
        }

        protected virtual void Space_Click(Spaces.Core.Space space)
        {
            var progressMeter = OpenProgressMeter();
            progressMeter.Initialize(space);
            //space.onLoadSpaceDone += progressMeter.Dispose;
            space.Enter(Core.AvatarWidget.UserAvatar);//UserSession.Instance.Avatar);
            OnSpaceSelected.Invoke();
        }

        public void Close()
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
        }

        public void CreateSpace()
        {
            var keyboard = Instantiate(SpacesKeyboardPrefab);
            keyboard.transform.position = transform.position;
            keyboard.transform.rotation = transform.rotation;
        }

        public void SaveCurrentSpace()
        {
            UserSession.Instance.SaveCurrentSpace();
        }

        public void GoHome()
        {
            UserSession.Instance.GoToLobby();
        }

        protected Core.ProgressMeterWidget OpenProgressMeter()
        {
            Core.ProgressMeterWidget progressMeter = null;

            var avatar = Core.AvatarWidget.UserAvatar;// UserSession.Instance.Avatar;
            Transform observer = avatar ? avatar.head : Camera.main.transform;

            Vector3 offset = Vector3.Scale(avatar.head.forward, new Vector3(1, 0, 1)).normalized * 5;
            Vector3 uiPos = observer.position + offset;
            Quaternion uiRot = Quaternion.LookRotation(observer.position - uiPos, Vector3.up);

            var meterGO = UserSession.Instance.GetInstance("ProgressMeter", observer);

            if (meterGO)
            {
                meterGO.transform.position = uiPos;
                meterGO.transform.rotation = uiRot;
                progressMeter = meterGO.GetComponent<Core.ProgressMeterWidget>();
                progressMeter.FaceCamera();
            }

            return progressMeter;
        }
    }
}