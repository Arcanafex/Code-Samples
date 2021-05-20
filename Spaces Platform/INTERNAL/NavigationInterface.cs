using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using Spaces.Core;

namespace Spaces.UnityClient
{
    public class NavigationInterface : MonoBehaviour
    {
        public VerticalLayoutGroup SpacesPanel;
        public GameObject SpaceEntryPrefab;

        void Start()
        {
            InitializeSpaceEntries();
        }

        void InitializeSpaceEntries()
        {
            var spaceList = Core.Space.SpacesManager.GetSpaces();

            if (spaceList.Count == 0)
                return;

            foreach (Transform child in SpacesPanel.transform)
                Destroy(child.gameObject);

            for (int u = 0; u < spaceList.Count; u++)
            {
                var space = spaceList[u];

                GameObject userEntry = Instantiate(SpaceEntryPrefab);
                userEntry.transform.SetParent(SpacesPanel.transform, false);
                userEntry.GetComponentInChildren<Text>().text = space.name;
                userEntry.GetComponent<Button>().onClick.AddListener(delegate { SpaceEntry_Click(space); }); ;
            }
        }

        void SpaceEntry_Click(Spaces.Core.Space space)
        {
            //Debug.Log(this.name + " So, you'd like to visit " + space.name + " (" + space.id + ")? That sounds nice!");
            space.Enter(GameObject.FindObjectOfType<AvatarWidget>());
        }

    }
}