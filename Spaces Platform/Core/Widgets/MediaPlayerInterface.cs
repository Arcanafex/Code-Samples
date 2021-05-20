using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace Spaces.Core
{
    public class MediaPlayerInterface : ScriptableObject
    {
        public enum PlayerState
        {
            Waiting,
            Ready,
            Loading,
            Playing,
            Paused,
            Stopped
        }

        private MediaPlayerCtrl m_player;
        private string m_currentUrl;
        private bool isPlayerReady;
        private bool isSubscribed;

        private VideoWidget m_widget;

        public Texture screenBlank;
        public Color screenBlankColor;
        public GameObject loadingObject;

        public PlayerState state { get; private set; }

        public UnityEvent OnPlay;
        public UnityEvent OnPause;
        public UnityEvent OnUnpause;
        public UnityEvent OnStop;
        public UnityEvent OnEnd;

        private MediaPlayerCtrl Player
        {
            get
            {
                if (!m_player)
                {
                    if (!m_widget)
                        return null;

                    m_player = m_widget.GetComponent<MediaPlayerCtrl>();

                    if (!m_player)
                        m_player = m_widget.gameObject.AddComponent<MediaPlayerCtrl>();
                }

                return m_player;
            }
        }

        public Texture ScreenBlank
        {
            get
            {
                if (!screenBlank)
                {
                    Texture2D screenTex = new Texture2D(1, 1);
                    screenTex.SetPixel(1, 1, screenBlankColor);
                    screenTex.Apply();
                    screenBlank = screenTex;
                }

                return screenBlank;
            }

            set
            {
                screenBlank = value;
            }
        }

        public static MediaPlayerInterface CreateMediaPlayerInterface()
        {
            return CreateInstance<MediaPlayerInterface>();
        }

        void OnEnable()
        {
            if (!isSubscribed && Player)
            {
                Player.OnReady += SetPlayerReady;
                Player.OnEnd += SetPlayerEnded;

                isSubscribed = true;
            }
        }

        void OnDisable()
        {
            if (isSubscribed && Player)
            {
                Player.OnReady -= SetPlayerReady;
                Player.OnEnd -= SetPlayerEnded;

                isSubscribed = false;
            }
        }

        public void Initialize(VideoWidget widget)
        {
            if (loadingObject)
                loadingObject.SetActive(false);

            m_widget = widget;

            isPlayerReady = false;
            state = PlayerState.Waiting;

            if (OnPlay == null) OnPlay = new UnityEvent();
            if (OnPause == null) OnPause = new UnityEvent();
            if (OnUnpause == null) OnUnpause = new UnityEvent();
            if (OnStop == null) OnStop = new UnityEvent();
            if (OnEnd == null) OnEnd = new UnityEvent();

            if (!isSubscribed && Player)
            {
                Player.OnReady += SetPlayerReady;
                Player.OnEnd += SetPlayerEnded;

                isSubscribed = true;
            }
        }

        public void Play()
        {
            Play(m_currentUrl);
        }

        public void Play(string url = null)
        {
            if (string.IsNullOrEmpty(url))
                url = m_currentUrl;
            else if (m_currentUrl != url)
            {
                m_currentUrl = url;
                Player.Load(m_currentUrl);
                SetToLoading();
            }

            if (state == PlayerState.Paused)
            {
                OnUnpause.Invoke();
                Player.Play();
                state = PlayerState.Playing;
            }
            else
            {
                m_widget.StartCoroutine(PlayWhenReady());
            }
        }

        private IEnumerator PlayWhenReady()
        {
            // supremely hacky version of this right now due to problems with events coming from MediaPlayerCtrl.
            int i = 2;

            while (!isPlayerReady && i-- > 0)
            {
                yield return new WaitForEndOfFrame();
            }

            SetPlayerReady();
            //

            Player.Play();
            state = PlayerState.Playing;
            OnPlay.Invoke();
        }

        public void Stop()
        {
            Player.Stop();
            state = PlayerState.Stopped;
            OnStop.Invoke();
        }

        public void Pause()
        {
            Player.Pause();
            state = PlayerState.Paused;
            OnPause.Invoke();
        }

        public void Restart()
        {
            Player.SeekTo(0);
        }

        public void SkipForward(int sec)
        {
            Player.SeekTo(sec);
        }

        public void SkipBack(int sec)
        {
            Player.SeekTo(Mathf.Clamp(sec, 0, sec));
        }

        private void SetPlayerReady()
        {
            isPlayerReady = true;
            state = PlayerState.Ready;
        }

        private void SetPlayerEnded()
        {
            OnEnd.Invoke();
            state = PlayerState.Waiting;
        }

        private void SetPlayerNotReady()
        {
            isPlayerReady = false;
            state = PlayerState.Waiting;
        }

        public void SetLoop(bool loops)
        {
            Player.m_bLoop = loops;
        }

        public void SetTargetMaterials(GameObject[] displayTargets)
        {
            Player.m_TargetMaterial = displayTargets;
        }

        public void SetToBlank()
        {
            foreach (GameObject target in Player.m_TargetMaterial)
            {
                foreach (MeshFilter m in target.GetComponentsInChildren<MeshFilter>())
                {
                    if (m.gameObject.GetComponent<Renderer>())
                        m.gameObject.GetComponent<Renderer>().material.mainTexture = ScreenBlank;
                }

                foreach (UnityEngine.UI.RawImage r in target.GetComponentsInChildren<UnityEngine.UI.RawImage>())
                {
                    r.material.mainTexture = ScreenBlank;
                }
            }
        }

        public void SetToLoading()
        {
            state = PlayerState.Loading;
            if (loadingObject) loadingObject.SetActive(true);
        }

        public void FlipUVsVertically()
        {
            foreach (GameObject target in Player.m_TargetMaterial)
            {
                foreach (MeshFilter filter in target.GetComponentsInChildren<MeshFilter>())
                {
                    if (filter)
                    {
                        Mesh mesh = target.GetComponent<MeshFilter>().mesh;

                        Vector2[] invertedUV = new Vector2[mesh.uv.Length];

                        for (int i = 0; i < mesh.uv.Length; i++)
                        {
                            invertedUV[i] = new Vector2(mesh.uv[i].x, 1.0f - mesh.uv[i].y);
                        }

                        mesh.uv = invertedUV;
                    }
                }
            }
        }

    }
}