using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;

namespace TMPC
{
    /// <summary>
    /// Handles stadium loading and applying surface materials to a StadiumLook script.
    /// </summary>
    public class StadiumLoader : MonoBehaviour
    {
        //
        // Summary:
        //     Stadium size category
        public enum StadiumSize
        {
            VerySmall = 1,
            Small = 2,
            Medium = 3,
            Big = 4
        }

        //
        // Summary:
        //     Stadium venue type
        public enum StadiumType
        {
            Outdoor = 0,
            Indoor = 1
        }

        public enum Surface
        {
            Clay = 0,
            Grass = 1,
            Hard = 2
        }

        public delegate void Loading(LoadingEventType eventType);
        public event Loading OnLoading;

        public Transform StadiumContainer;
        public string RootAssetPath = "Assets/3D/Stadium";

        public Surface MatchSurface;
        public int SurfaceVariant = 1;
        public StadiumSize Size = StadiumSize.Small;
        public StadiumType VenueType = StadiumType.Outdoor;

        public int AdBillboard1 = 1;
        public int AdBillboard2 = 2;
        public Color SeatColor = Color.cyan;
        public int ExtrasProfileIndex = 1;
        [Range(0f, 1f)] public float CrowdSize = 0.6f;

        public bool LoadOnStart = false;

        public float Progress { get; set; }

        private Dictionary<string, GameObject> m_loadedStadiumModels = new Dictionary<string, GameObject>();
        private StadiumLook m_stadiumLook;
        private ExtrasProfileController m_extrasProfileController;
        private CrowdManager m_crowdManager;
        private MatchSimulationManager m_matchSimulationManager;

        private void Start()
        {
            if (LoadOnStart)
                LoadStadium(Size, VenueType);
        }

        public void LoadStadium(StadiumSize size, StadiumType venueType)
        {
            Size = size;
            VenueType = venueType;

            StartCoroutine(Load($"{RootAssetPath}/{size}_Stadium_{venueType}.prefab", StadiumContainer));
        }

        public void SetStadiumSize(float index)
        {
            SetStadiumSize((int)index);
        }

        public void SetStadiumSize(int index)
        {
            int sizeIndex = Mathf.Clamp(index, (int)StadiumSize.VerySmall, (int)StadiumSize.Big);
            SetStadiumSize((StadiumSize)sizeIndex);
        }

        public void SetStadiumSize(StadiumSize size)
        {
            Size = size;
            LoadStadium(Size, VenueType);
        }

        public void SetVenueType(int index)
        {
            int venueIndex = Mathf.Clamp(index, (int)StadiumType.Outdoor, (int)StadiumType.Indoor);
            SetVenueType((StadiumType)venueIndex);
        }

        public void SetVenueType(StadiumType type)
        {
            VenueType = type;
            LoadStadium(Size, VenueType);
        }

        public void SetSurfaceType(int index)
        {
            int surfaceIndex = Mathf.Clamp(index, (int)Surface.Clay, (int)Surface.Hard);
            SetSurfaceType((Surface)surfaceIndex);
        }

        public void SetSurfaceType(Surface surface)
        {
            MatchSurface = surface;

            switch(MatchSurface)
            {
                case Surface.Clay: m_stadiumLook?.SetSurface(ReboundCG.Tennis.MatchSurface.Clay, SurfaceVariant); break;
                case Surface.Grass: m_stadiumLook?.SetSurface(ReboundCG.Tennis.MatchSurface.Grass, SurfaceVariant); break;
                case Surface.Hard: m_stadiumLook?.SetSurface(ReboundCG.Tennis.MatchSurface.Hard, SurfaceVariant); break;
            }
        }

        public void SetSurfaceTypeVariant(float index)
        {
            SetSurfaceTypeVariant((int)index);
        }

        public void SetSurfaceTypeVariant(int index)
        {
            int variationIndex = Math.Max(index, 1);
            SurfaceVariant = variationIndex;
            SetSurfaceType(MatchSurface);
        }

        public void SetBillboard1(string id)
        {
            if (int.TryParse(id, out int idInt))
                SetBillboard1(idInt);
        }

        public void SetBillboard1(int id)
        {
            AdBillboard1 = id;
            m_stadiumLook?.SetBillboard1Id(AdBillboard1);
        }

        public void SetBillboard2(string id)
        {
            if (int.TryParse(id, out int idInt))
                SetBillboard2(idInt);
        }

        public void SetBillboard2(int id)
        {
            AdBillboard2 = id;
            m_stadiumLook?.SetBillboard2Id(AdBillboard2);
        }

        public void SetSeatColor(string colorCode)
        {
            if (ColorUtility.TryParseHtmlString(colorCode, out Color seatColor))
                SetSeatColor(seatColor);
        }

        public void SetSeatColor(Color seatColor)
        {
            SeatColor = seatColor;
            m_stadiumLook?.SetSeatColor(SeatColor);
        }

        public void SetExtrasProfile(string index)
        {
            if (int.TryParse(index, out int indexInt))
                SetExtrasProfile(indexInt);
        }

        public void SetExtrasProfile(int index)
        {
            ExtrasProfileIndex = Math.Max(index, 0);

            if (m_extrasProfileController)
            {
                m_extrasProfileController.SetProfile(ExtrasProfileIndex);
            }
        }

        public void SetCrowdSize(float fill)
        {
            CrowdSize = fill;

            if (m_crowdManager)
            {
                m_crowdManager.SpawnCrowd(fill);
            }
        }

        public void SetMatchSimulationManager(MatchSimulationManager matchSimulationManager)
        {
            m_matchSimulationManager = matchSimulationManager;
        }

        public void CrowdClap()
        {
            m_crowdManager?.Clap();
        }

        public void CrowdStandClap()
        {
            m_crowdManager?.StandingClap();
        }

        public IEnumerator Load(string id, Transform parent = null)
        {
            OnLoading?.Invoke(LoadingEventType.LoadingStart);

            if (!m_loadedStadiumModels.TryGetValue(id, out GameObject stadium))
            {
                Progress = 0;
                var handle = Addressables.InstantiateAsync(id, parent);

                while (!handle.IsDone)
                {
                    Progress = handle.PercentComplete;
                    OnLoading?.Invoke(LoadingEventType.LoadingProgressChange);
                    yield return null;
                }

                Progress = 1;

                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    OnLoading?.Invoke(LoadingEventType.LoadingFailed);
                    throw new NullReferenceException("Failed to load prefab : " + id);
                }
                else
                {
                    stadium = handle.Result;
                    m_loadedStadiumModels.Add(id, stadium);
                }
            }

            if (!stadium)
                throw new NullReferenceException("Game object not found in prefab : " + id);
            else
            {
                // Disable up existing model
                if (m_stadiumLook)
                {
                    var oldStadium = m_stadiumLook.gameObject;
                    oldStadium.SetActive(false);
                }

                stadium.SetActive(true);
                m_stadiumLook = stadium.GetComponent<StadiumLook>();
                m_extrasProfileController = stadium.GetComponent<ExtrasProfileController>();
                m_crowdManager = stadium.GetComponent<CrowdManager>();
            }

            SetSurfaceType(MatchSurface);
            SetBillboard1(AdBillboard1);
            SetBillboard2(AdBillboard2);
            SetSeatColor(SeatColor);
            SetExtrasProfile(ExtrasProfileIndex);
            SetCrowdSize(CrowdSize);
            m_crowdManager?.SetMatchSimulationManager(m_matchSimulationManager);

            OnLoading?.Invoke(LoadingEventType.LoadingDone);
        }
    }
}
