using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Networking;

namespace Spaces.Core
{
    /// <summary>
    /// A sound clip in a scene. Will create it’s own AudioSource component if it isn’t associated with a Playlist.
    /// </summary>

    public class SoundWidget : AssetHandlerWidget, IPlayable, IDisplay<AudioClip>
    {
        public AudioClip sound;
        public string url;
        public AudioType audioType;
        private AudioSource source;

        public float maxVolume = 1;
        public float minVolume = 0;
        public AnimationCurve fadeCurve;
        public float fadeDuration;

        public bool startAtMin;
        public bool playOnStart = true;
        public bool loopClip = true;

        public override void Initialize()
        {
            base.Initialize();

            GetOrAddAudioSource();

            if (startAtMin)
                source.volume = minVolume;
            else
                source.volume = maxVolume;

            if (loopClip)
                source.loop = true;
            else
                source.loop = false;

            if (assetWidget)
            {
                //WaitForAsset
            }
            else if (!string.IsNullOrEmpty(url))
            {
                StartCoroutine(GetAudio());
            }
            else
            {
                UpdateAudioSource();
            }
        }

        public override void Initialize(AssetWidget assetWidget)
        {
            if (assetWidget.Asset != null && assetWidget.Asset.audio)
            {
                SetContent(assetWidget.Asset.audio);
                Initialize();

                if (sound && !source.isPlaying && playOnStart)
                    FadeIn(fadeDuration);
            }
            else
            {
                Initialize();
                assetWidget.OnAssetInstanced.AddListener(delegate { WaitForAssetInstance(assetWidget); });
            }
        }

        public void Pause()
        {
            source.Pause();
        }

        public void Play()
        {
            source.Play();//.PlayOneShot(sound);
        }

        public void Restart()
        {
            source.time = 0;
        }

        public void Stop()
        {
            source.Stop();
        }

        protected AudioSource GetOrAddAudioSource()
        {
            source = GetComponentInParent<AudioSource>();

            if (!source)
                source = gameObject.AddComponent<AudioSource>();

            return source;
        }

        private void WaitForAssetInstance(AssetWidget assetWidget)
        {
            assetWidget.OnAssetInstanced.RemoveListener(delegate { WaitForAssetInstance(assetWidget); });

            SetContent(assetWidget.Asset.audio);

            if (sound && !source.isPlaying && playOnStart)
                FadeIn(fadeDuration);
        }


        public void FadeIn(float duration)
        {
            Play();
            StopAllCoroutines();
            StartCoroutine(Fade(maxVolume, duration));
        }

        public void FadeOut(float duration)
        {
            StopAllCoroutines();
            StartCoroutine(Fade(minVolume, duration));
        }

        private IEnumerator Fade(float target, float duration)
        {
            float elapsedTime = 0;
            float startLevel = source.volume;

            while (elapsedTime < duration && source.volume != target)
            {
                elapsedTime += Time.deltaTime;
                source.volume = Mathf.Lerp(startLevel, target, fadeCurve.Evaluate(elapsedTime / duration));
                yield return null;
            }

            if (source.volume == 0)
                Stop();
        }

        private IEnumerator GetAudio()
        {
            if (!string.IsNullOrEmpty(url))
            {
                using (var request = UnityWebRequest.GetAudioClip(url, audioType))
                {
                    var async = request.Send();

                    var progressMeter = OpenProgressMeter();
                    progressMeter.Text = "Downloading";

                    while (!async.isDone)
                    {
                        progressMeter.Value = async.progress;
                        yield return async.isDone;
                    }

                    if (request.isError)
                    {
                        Debug.Log(this.name + " [Audio Download Error]" + request.error);
                    }
                    else
                    {
                        SetContent(((DownloadHandlerAudioClip)request.downloadHandler).audioClip);
                        UpdateAudioSource();
                    }

                    progressMeter.Value = 1;
                    progressMeter.Dispose();
                }
            }
        }


        public void SetContent(AudioClip content)
        {
            sound = content;
        }

        public void UpdateAudioSource()
        {
            if (sound)
            {
                if (source)
                    source.clip = sound;

                if (!source.isPlaying && playOnStart)
                    FadeIn(fadeDuration);
            }
        }

        public override void OnInstantiateAsset()
        {
            UpdateAudioSource();
        }

        public override GameObject InstancePlaceholder()
        {
            return ModelWidget.GeneratePrimitive(ModelWidget.Primitive.Sphere, transform);
        }

        private ProgressMeterWidget OpenProgressMeter()
        {
            ProgressMeterWidget progressMeter = null;

            var avatar = AvatarWidget.UserAvatar; //UnityClient.UserSession.Instance.Avatar;
            Transform observer = avatar ? avatar.head : Camera.main.transform;

            Quaternion uiRot = Quaternion.LookRotation(-transform.forward, Vector3.up);

            var meterGO = UnityClient.UserSession.Instance.GetInstance("ProgressMeter", this.transform);

            if (meterGO)
            {
                meterGO.transform.rotation = uiRot;
                progressMeter = meterGO.GetComponent<ProgressMeterWidget>();
            }

            return progressMeter;
        }
    }
}
