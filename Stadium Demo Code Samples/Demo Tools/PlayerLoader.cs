using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;
using ReboundCG.Tennis;
using Zenject;

namespace TMPC
{
    public enum RacketType
    {
        racket
    }

    public enum OutfitType
    {
        UNKNOWN = -1,
        tshirtshort,
        poloshort,
        tank1skirt,
        tank2skirt
    }

    public enum ShoesType
    {
        shoes
    }

    public enum RacketHand
    {
        Right,
        Left
    }

    public enum LoadingEventType
    {
        LoadingFailed = -1,
        NotStarted = 0,
        LoadingStart = 1,
        LoadingProgressChange,
        LoadingDone
    }

    /// <summary>
    /// Component for loading player model and applying settings to a PlayerLook script.
    /// </summary>
    public class PlayerLoader : MonoBehaviour
    {
        public delegate void Loading(LoadingEventType eventType);
        public event Loading OnLoading;

        public string RootAssetPath = "Assets/3D/Characters";
        public Gender Gender = Gender.MALE;
        public PlayerLook.SkinType SkinType = PlayerLook.SkinType.Black;
        public int Hairdo = 1;
        public Color HairColor = Color.white;
        public Wristbands Wristbands = Wristbands.None;
        public Handedness Handedness = Handedness.RightHanded;

        public Color AccessoryColor = Color.blue;


        // Racket
        public GameObject RacketPrefab;
        public string Racket = "racket";
        public int RacketBrandId = 4;
        public int RacketId = 46;

        // Outfit
        public OutfitType Outfit = OutfitType.tshirtshort;
        public int OutfitBrandId = 17;
        public int OutfitId = 39;

        // Shoes
        public string Shoes = "shoes";
        public int ShoesBrandId = 3;
        public int ShoesId = 19;

        public bool LoadOnStart = false;

        public float Progress { get; set; }
        public TennisPlayerView PlayerView => m_playerView;

        private SignalBus m_signalBus;
        private MatchSimulationManager m_matchSimulationManager;

        private Racket m_racket;
        private TennisPlayerView m_playerView;
        private PlayerLook m_playerLook;
        private Dictionary<string, GameObject> m_loadedPlayerModels;

        private MatchInitializationData m_matchData;
        public class MatchInitializationData
        {
            public bool ReverseDisplay;
            public Color Color;
            public ReboundCG.Tennis.Simulation.IMatchPlayer MatchPlayer;

            public ITennisPlayerLook PlayerLookData;
            public IEquipmentItem TechData;
            public IEquipmentItem OutfitData;
            public IEquipmentItem ShoesData;
        }

        private bool m_updateRequested = false;

        private void Awake()
        {
            m_loadedPlayerModels = new Dictionary<string, GameObject>();
           
        }

        private void Start()
        {
            if (LoadOnStart)
                UpdatePlayerModel();
        }

        public void InitializeMatchPlayer(
            ReboundCG.Tennis.Simulation.IMatchPlayer matchPlayer,
            bool reverseDisplay,
            Color playerColor,
            ITennisPlayerLook playerLook,
            IEquipmentItem tech,
            IEquipmentItem outfit,
            IEquipmentItem shoes,
            SignalBus signalBus,
            MatchSimulationManager matchSimulationManager
            )
        {
            m_signalBus = signalBus;
            m_matchSimulationManager = matchSimulationManager;

            if (m_matchData == null)
                m_matchData = new MatchInitializationData();

            m_matchData.MatchPlayer = matchPlayer;
            m_matchData.ReverseDisplay = reverseDisplay;
            m_matchData.Color = playerColor;

            m_matchData.PlayerLookData = playerLook;
            m_matchData.TechData = tech;
            m_matchData.OutfitData = outfit;
            m_matchData.ShoesData = shoes;

            if (m_matchData.PlayerLookData != null)
            {
                Gender = m_matchData.PlayerLookData.Gender;
                SetSkinType(m_matchData.PlayerLookData.Skin);
                SetHairdo(m_matchData.PlayerLookData.HairModel);

                if (ColorUtility.TryParseHtmlString(playerLook.HairColor, out Color hairColor))
                    HairColor = hairColor;

                Wristbands = m_matchData.PlayerLookData.Wristbands;
            }

            if (m_matchData.OutfitData != null)
            {
                Outfit = GetOutfitType(m_matchData.OutfitData.Model);

                ColorUtility.TryParseHtmlString(m_matchData.OutfitData.Color2, out Color color2);
                AccessoryColor = color2;

                OutfitBrandId = m_matchData.OutfitData.EquipmentBrandId;
                OutfitId = m_matchData.OutfitData.Id;
            }

            if (m_matchData.ShoesData != null)
            {
                ShoesBrandId = m_matchData.ShoesData.EquipmentBrandId;
                ShoesId = m_matchData.ShoesData.Id;
            }

            if (m_matchData.TechData != null)
            {
                RacketBrandId = m_matchData.TechData.EquipmentBrandId;
                RacketId = m_matchData.TechData.Id;
            }
        }

        public OutfitType GetOutfitType(string modelName)
        {
            switch (modelName)
            {
                case "tshirtshort": return OutfitType.tshirtshort;
                case "poloshort": return OutfitType.poloshort;
                case "tank1skirt": return OutfitType.tank1skirt;
                case "tank2skirt": return OutfitType.tank2skirt;
                default: return OutfitType.UNKNOWN;
            }
        }

        public void UpdatePlayerModel()
        {
            if (m_racket == null)
            {
                var racket = Instantiate(RacketPrefab);
                m_racket = racket.GetComponent<Racket>();

                if (!m_racket)
                    Destroy(racket);
            }

            if (!isActiveAndEnabled)
                m_updateRequested = true;
            else
                StartCoroutine(Load($"{RootAssetPath}/player_{Gender}_{Outfit}.prefab", transform));
        }

        private void Update()
        {
            if (m_updateRequested)
            {
                m_updateRequested = false;
                UpdatePlayerModel();
            }
        }

        public IEnumerator Load(string id, Transform parent = null)
        {
            OnLoading?.Invoke(LoadingEventType.LoadingStart);

            if (!m_loadedPlayerModels.TryGetValue(id, out GameObject player))
            {
                Progress = 0;
                var handle = Addressables.InstantiateAsync(id, parent.position, parent.rotation);

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
                    player = handle.Result;
                    m_loadedPlayerModels.Add(id, player);
                }
            }


            if (!player)
                throw new NullReferenceException("Game object not found in prefab : " + id);
            else
            {
                // Disable existing model
                if (m_playerLook)
                {
                    var oldPlayer = m_playerLook.gameObject;
                    oldPlayer.SetActive(false);
                }

                player.SetActive(true);
                m_playerView = player.GetComponent<TennisPlayerView>();
                m_playerLook = player.GetComponent<PlayerLook>();

                if (m_playerView && m_racket)
                {
                    m_playerView.SetRacket(m_racket.transform);
                    m_playerView.SetShotPoint(m_racket.ShotPoint);
                }

                if (m_playerLook)
                {
                    m_playerLook.SetRootPath(RootAssetPath);
                    m_playerLook.RacketRenderer = m_racket?.RacketRenderer;
                }

                var ikInterpolation = player.GetComponent<IKInterpolation>();
                if (ikInterpolation && m_racket)
                {
                    ikInterpolation.LeftHandTarget = m_racket.LeftHandIKTarget;
                    ikInterpolation.RightHandTarget = m_racket.RightHandIKTarget;
                }
            }

            if (m_matchData != null)
            {
                m_playerView?.Initialize(m_matchData.MatchPlayer, m_matchData.ReverseDisplay, m_signalBus, m_matchSimulationManager);
                m_playerLook?.InitializeLook(m_matchData.PlayerLookData, m_matchData.TechData, m_matchData.OutfitData, m_matchData.ShoesData);
            }
            else
            {

                m_playerLook?.SetGender((int)Gender);
                m_playerLook?.SetSkin(SkinType);
                SetHairdo(Hairdo);
                m_playerLook?.SetCoiffure(Hairdo, HairColor);
                m_playerLook?.SetWristbands((int)Wristbands, AccessoryColor);
                m_playerView?.UpdateHandedness(Handedness == Handedness.LeftHanded);
            }

            OnLoading?.Invoke(LoadingEventType.LoadingDone);
        }

        public void SetHandedness(bool left)
        {
            if (left)
                Handedness = Handedness.LeftHanded;
            else
                Handedness = Handedness.RightHanded;

            m_playerView?.UpdateHandedness(Handedness == Handedness.LeftHanded);
        }

        public void SetOutfitModel(int index)
        {
            SetOutfitModel((OutfitType)index);
        }

        public void SetOutfitModel(OutfitType outfit)
        {
            if (Gender == Gender.FEMALE && outfit < OutfitType.tank1skirt)
                outfit = OutfitType.tank1skirt;
            else if (Gender == Gender.MALE && outfit > OutfitType.poloshort)
                outfit = OutfitType.tshirtshort;

            Outfit = outfit;
            UpdatePlayerModel();
        }

        public void SetGenderMale(bool male)
        {
            if (male && Gender != Gender.MALE)
            {
                Gender = Gender.MALE;
                m_playerLook?.SetGender((int)Gender);
                SetOutfitModel(Outfit);
            }
        }

        public void SetGenderFemale(bool female)
        {
            if (female && Gender != Gender.FEMALE)
            {
                Gender = Gender.FEMALE;
                m_playerLook?.SetGender((int)Gender);
                SetOutfitModel(Outfit);
            }
        }

        public void SetSkinType(float index)
        {
            SetSkinType((int)index);
        }

        public void SetSkinType(int index)
        {
            int typeIndex = Mathf.Clamp(index, (int)PlayerLook.SkinType.VeryFair, (int)PlayerLook.SkinType.Black);
            SetSkinType((PlayerLook.SkinType)typeIndex);
        }

        public void SetSkinType(PlayerLook.SkinType skinType)
        {
            SkinType = skinType;
            m_playerLook?.SetSkin(SkinType);
        }

        public void SetRightWristband(bool visible)
        {
            switch(Wristbands)
            {
                case Wristbands.None:
                case Wristbands.Right:
                    Wristbands = visible ? Wristbands.Right : Wristbands.None;
                    break;
                case Wristbands.Left:
                case Wristbands.Both:
                    Wristbands = visible ? Wristbands.Both : Wristbands.Left;
                    break;
            }

            m_playerLook?.SetWristbands((int)Wristbands, AccessoryColor);
        }

        public void SetLeftWristband(bool visible)
        {
            switch (Wristbands)
            {
                case Wristbands.None:
                case Wristbands.Left:
                    Wristbands = visible ? Wristbands.Left : Wristbands.None;
                    break;
                case Wristbands.Right:
                case Wristbands.Both:
                    Wristbands = visible ? Wristbands.Both : Wristbands.Right;
                    break;
            }

            m_playerLook?.SetWristbands((int)Wristbands, AccessoryColor);
        }

        public void SetHairdo(float hairIndex)
        {
            SetHairdo((int)hairIndex);
        }

        public void SetHairdo(int hairIndex)
        {
            if (m_playerLook)
            {
                if (m_playerLook.LoadingHair)
                    return;

                for (int i = 0; i < m_playerLook.HairAnchor.childCount; i++)
                {
                    var hairdo = m_playerLook.HairAnchor.GetChild(i);
                    hairdo.gameObject.SetActive(false);
                    Destroy(hairdo.gameObject);
                }

                switch (Gender)
                {
                    case Gender.FEMALE: Hairdo = Mathf.Clamp(hairIndex, 1, 11); break;
                    case Gender.MALE: Hairdo = Mathf.Clamp(hairIndex, 1, 14); break;
                    default: return;
                }

                m_playerLook?.SetCoiffure(Hairdo, HairColor);
            }
        }

        public void SetHairColor(string hexCode)
        {
            if (ColorUtility.TryParseHtmlString(hexCode, out Color hairColor))
                SetHairColor(hairColor);
        }

        public void SetHairColor(Color hairColor)
        {
            HairColor = hairColor;
            m_playerLook?.SetCoiffure(Hairdo, HairColor);
        }

        public void SetAccessoriesColor(string hexCode)
        {
            if (ColorUtility.TryParseHtmlString(hexCode, out Color accessoryColor))
                SetAccessoriesColor(accessoryColor);
        }

        public void SetAccessoriesColor(Color accessoryColor)
        {
            AccessoryColor = accessoryColor;
            m_playerLook?.UpdateAccessoryColor(AccessoryColor);
        }

        public void SetRacketBrand(string brandId)
        {
            if (int.TryParse(brandId, out int brandIdInt))
                SetRacketBrand(brandIdInt);
        }

        public void SetRacketBrand(int brandId)
        {
            RacketBrandId = brandId;
            SetRacket(RacketBrandId, RacketId);
        }

        public void SetRacketId(string itemId)
        {
            if (int.TryParse(itemId, out int idInt))
                SetRacketId(idInt);
        }

        public void SetRacketId(int itemId)
        {
            RacketId = itemId;
            SetRacket(RacketBrandId, RacketId);
        }

        public void SetRacket(int brandId, int itemId)
        {
            RacketBrandId = brandId;
            RacketId = itemId;
            m_playerLook?.SetRacket(Racket, RacketBrandId, RacketId, Gender);
        }

        public void SetOutfitBrand(string brandId)
        {
            if (int.TryParse(brandId, out int brandIdInt))
                SetOutfitBrand(brandIdInt);
        }

        public void SetOutfitBrand(int brandId)
        {
            OutfitBrandId = brandId;
            SetOutfit(OutfitBrandId, OutfitId);
        }

        public void SetOutfitId(string itemId)
        {
            if (int.TryParse(itemId, out int idInt))
                SetOutfitId(idInt);
        }

        public void SetOutfitId(int itemId)
        {
            OutfitId = itemId;
            SetOutfit(OutfitBrandId, OutfitId);
        }

        public void SetOutfit(int brandId, int itemId)
        {
            OutfitBrandId = brandId;
            OutfitId = itemId;
            m_playerLook?.SetOutfit(Outfit.ToString(), OutfitBrandId, OutfitId, Gender);
        }
 
        public void SetShoesBrand(string brandId)
        {
            if (int.TryParse(brandId, out int brandIdInt))
                SetShoesBrand(brandIdInt);
        }

        public void SetShoesBrand(int brandId)
        {
            ShoesBrandId = brandId;
            SetShoes(ShoesBrandId, ShoesId);
        }

        public void SetShoesId(string itemId)
        {
            if (int.TryParse(itemId, out int idInt))
                SetShoesId(idInt);
        }

        public void SetShoesId(int itemId)
        {
            ShoesId = itemId;
            SetShoes(ShoesBrandId, ShoesId);
        }

        public void SetShoes(int brandId, int itemId)
        {
            ShoesBrandId = brandId;
            ShoesId = itemId;
            m_playerLook?.SetShoes(Shoes, ShoesBrandId, ShoesId, Gender);
        }
    }
}
