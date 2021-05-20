using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spaces.LBE
{

    public class RepositionManager : MonoBehaviour
    {
        public GameObject m_StageVisualization;
        public RallyPoint m_StartRallyPoint;
        public RallyPoint m_EndRallyPoint;
        public bool endZoneAutoActivated = true;

        public GameObject[] m_HideForReposition;
        

        //private void OnEnable()
        //{
        //    UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneManager_sceneLoaded;

        //}

        //private void OnDisable()
        //{
        //    UnityEngine.SceneManagement.SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
        //}

        //private void SceneManager_sceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        //{
        //    Reposition();
        //}

        private void Awake()
        {
            if (m_StageVisualization) m_StageVisualization.SetActive(true);
            if (m_StartRallyPoint) m_StartRallyPoint.gameObject.SetActive(false);
            if (m_EndRallyPoint) m_EndRallyPoint.gameObject.SetActive(false);
        }

        private void Start()
        {
            Reposition();
            m_StartRallyPoint.OnRallied.AddListener(RepositionDone);
        }

        public void Reposition()
        {
            Debug.Log(this.name + " [Reposition Started]");

            m_StageVisualization.SetActive(true);

            foreach (var obj in m_HideForReposition)
            {
                obj.SetActive(false);
            }

            TrackingVolume.Instance.ResetSceneOrigin();

            m_StartRallyPoint.gameObject.SetActive(true);
            StartCoroutine(BeginRallyOnSceneStart());
        }

        private IEnumerator BeginRallyOnSceneStart()
        {
            yield return new WaitForEndOfFrame();
            m_StartRallyPoint.Rally();
        }

        private void RepositionDone()
        {
            Debug.Log(this.name + " [Reposition Done]");

            m_StageVisualization.SetActive(false);

            if (endZoneAutoActivated)
            {
                ActivateEndZone();
            }

            foreach (var obj in m_HideForReposition)
            {
                obj.SetActive(true);
                Debug.Log(this.name + " [Reactivating] " + obj.name);
            }
        }

        public void ActivateEndZone()
        {
            m_EndRallyPoint.gameObject.SetActive(true);
        }

    }
}