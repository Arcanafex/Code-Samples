using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Spaces.Core
{
    public class PlaylistWidget : Widget, IPlaylist, IPlayable
    {
        public bool loop;
        public bool playOnStart;
        public bool autoplayNext;

        public bool loadSpacesAssets;
        public Constants.AssetType assetType;

        //public float crossFadeDuration;
        //public float imageDisplayDuration;
        private float elapsedTime;

        private int currentIndex;
        public int CurrentIndex
        {
            get { return currentIndex; }
            set
            {
                currentIndex = value;
                //Select(currentIndex);
            }
        }

        public Material displayMaterial;
        private MediaPlayerInterface m_Player;
        private AudioSource audioSource;
        private bool wasPlaying;

        // TODO: determine how to manage the parentWidget as both model and sound

        private bool isPlaying;
        public bool IsPlaying
        {
            get { return IsPlaying; }
        }

        private List<Widget> playableList;
        public override Widget[] childWidgets
        {
            get
            {
                return playableList.ToArray();
            }
        }

        void Start()
        {
            elapsedTime = 0;

            RefreshPlayableList();
            GetOrSetMediaPlayers();

            if (playOnStart)
               First();
        }
        
        private void RefreshPlayableList()
        {
            if (playableList == null)
                playableList = new List<Widget>();
            else
                playableList.Clear();

            foreach (Transform child in transform)
            {
                Widget playable = child.GetComponents<Widget>().FirstOrDefault(w => w.GetComponent<IPlayable>() != null);

                if (playable != null)
                    playableList.Add(playable);
            }

            foreach (Asset asset in Spaces.UnityClient.UserSession.Instance.CurrentSpace.Assets.Where(a => a.assetType == assetType.ToString()))
            {
                GameObject go = new GameObject(asset.name);
                go.transform.SetParent(transform);

                if (assetType == Constants.AssetType.video)// || assetType == Constants.AssetType.movfile)
                {
                    var widget = go.AddComponent<VideoWidget>();
                    widget.assetWidget.assetID = asset.id;
                    widget.flipUVVertical = true;
                    playableList.Add(widget);
                }
                else if (assetType == Constants.AssetType.image)
                {
                    var widget = go.AddComponent<ImageWidget>();
                    widget.assetWidget.assetID = asset.id;
                    playableList.Add(widget);
                }
            }
        }

        void Update()
        {
            if (audioSource)
            {
                if (wasPlaying && !audioSource.isPlaying && autoplayNext)
                    Next();

                wasPlaying = audioSource.isPlaying;
            }

            //increment elapsed time if displaying image
        }

        private void InitializeDisplayMaterials()
        {
            Widget model;

            // If no parent, create one.
            if (!transform.parent)
            {
                //model = ModelWidget.CreateDefaultModelWidget(transform);
            }
            else
            {
                model = transform.GetComponentsInParent<Widget>().FirstOrDefault(w => w.GetComponent<IModelWidget>() != null);
            }

            //if (model != null)
            //{
            //    ((IModelWidget)model).SetMaterial(displayMaterial);
            //    ((IModelWidget)model).SetTexture(m_Player.ScreenBlank);
            //    m_Player.SetTargetMaterials(((IModelWidget)model).GetRenderedObjects());
            //}
        }

        private void GetOrSetMediaPlayers()
        {
            if (GetComponentInChildren<SoundWidget>())
            {
                audioSource = GetComponentInParent<AudioSource>();

                if (!audioSource)
                    audioSource = gameObject.AddComponent<AudioSource>();
            }

            if (GetComponentInChildren<VideoWidget>())
            {
                m_Player = gameObject.GetComponentInParent<MediaPlayerInterface>();

                //if (!m_Player)
                //    m_Player = gameObject.AddComponent<MediaPlayerInterface>();

                InitializeDisplayMaterials();
            }
        }

        public void First()
        {
            if (playableList.Count < 1)
                return;

            if (CurrentIndex != 0)
                Stop();

            CurrentIndex = 0;
            Play();
        }

        public void Last()
        {
            if (playableList.Count < 1)
                return;

            if (CurrentIndex != playableList.Count - 1)
                Stop();

            CurrentIndex = playableList.Count - 1;
            Play();
        }

        public void Next()
        {
            if (loop)
            {
                Stop();
                CurrentIndex = (CurrentIndex + 1) % playableList.Count;
                Play();
            }
            else if (CurrentIndex + 1 < playableList.Count)
            {
                Stop();
                CurrentIndex += 1;
                Play();
            }
            else
            {
                //CurrentIndex + 1 == playableList.Count
                //Keep playing.
            }
        }

        public void Previous()
        {
            if (loop)
            {
                Stop();
                CurrentIndex = CurrentIndex == 0 ? CurrentIndex = playableList.Count - 1 : CurrentIndex -= 1;
                Play();
            }
            else if (CurrentIndex > 0)
            {
                Stop();
                CurrentIndex -= 1;
                Play();
            }
            else
            {
                //CurrentIndex + 1 == playableList.Count
            }
        }

        public void Select(int index)
        {
            if (!(index < playableList.Count))
                return;

            if (index != CurrentIndex)
                Stop();

            CurrentIndex = index;
            Play();
        }

        public void Play()
        {
            isPlaying = true;

            if (CurrentIndex > -1 && CurrentIndex < playableList.Count)
                ((IPlayable)playableList[CurrentIndex]).Play();
        }

        public void Pause()
        {
            isPlaying = false;

            if (CurrentIndex > -1 && CurrentIndex < playableList.Count)
                ((IPlayable)playableList[CurrentIndex]).Pause();
        }

        public void Stop()
        {
            isPlaying = false;

            if (CurrentIndex > -1 && CurrentIndex < playableList.Count)
                ((IPlayable)playableList[CurrentIndex]).Stop();
        }

        public void Restart()
        {
            if (CurrentIndex > -1 && CurrentIndex < playableList.Count)
                ((IPlayable)playableList[CurrentIndex]).Restart();
        }

        public bool Add(Widget playableWidget)
        {
            bool success = false;

            if (playableWidget.GetComponent<IPlayable>() != null && !playableList.Contains(playableWidget))
            {
                playableList.Add(playableWidget);

                if (!playableWidget.transform.IsChildOf(transform))
                    playableWidget.transform.SetParent(transform);

                success = true;
            }

            return success;
        }
        
    }
}