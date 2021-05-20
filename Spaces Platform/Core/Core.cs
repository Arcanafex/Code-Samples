using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Spaces.Core
{
    public static class InternalShaders
    {
        public const string Standard = "Standard";
        public const string StandardDoubleSided = "Standard Double Sided";
        public const string UnlitColor = "Unlit Color";
        public const string UnlitTexture = "Unlit Texture";
        public const string UnlitCutout = "Unlit Cutout";
        public const string UnlitTransparent = "Unlit Transparent";
        public const string MobileDiffuse = "Mobile Diffuse";
        public const string MobileParticles = "Mobile Particles";
        public const string MobileSkybox = "Mobile Skybox";
        public const string MobileUnlit = "Mobile Unlit";
        public const string Skybox = "Skybox";
        public const string SkyboxCubemap = "Skybox Cubemap";
        public const string SkyboxProcedural = "Skybox Procedural";
        public const string Particles = "Particles";
        public const string ParticlesAdditive = "Particles Additive";
        public const string ParticlesAlphaBlended = "Particles Alpha Blended";
        public const string ParticlesAnimAlphaBlended = "Particles Anim Alpha Blended";
        public const string ParticlesMultiply = "Particles Multiply";

        public static Shader GetStandard() { return Shader.Find("Standard"); }
        public static Shader GetUnlitColor() { return Shader.Find("Unlit/Color"); }
        public static Shader GetUnlitTexture() { return Shader.Find("Unlit/Texture"); }
        public static Shader GetUnlitTransparent() { return Shader.Find("Unlit/Transparent"); }
        public static Shader GetUnlitCutout() { return Shader.Find("Unlit/Cutout"); }
        public static Shader GetMobileDiffuse() { return Shader.Find("Mobile/Diffuse"); }
        public static Shader GetStandardDoubleSided() { return Shader.Find("Standard Double Sided"); }
        public static Shader GetSkybox() { return Shader.Find("Skybox/6 Sided"); }
        public static Shader GetSkyboxCubemap() { return Shader.Find("Skybox/Cubemap"); }
        public static Shader GetSkyboxProcedural() { return Shader.Find("Skybox/Procedural"); }
        public static Shader GetParticles() { return Shader.Find("Particles/Alpha Blended Premultiply"); }
        public static Shader GetParticlesAdditive() { return Shader.Find("Particles/Additive"); }
        public static Shader GetParticlesAlphaBlended() { return Shader.Find("Particles/Alpha Blended"); }
        public static Shader GetParticlesAnimAlphaBlended() { return Shader.Find("Particles/Anim Alpha Blended"); }
        public static Shader GetParticlesMultiply() { return Shader.Find("Particles/Multiply"); }
    }

    [System.Serializable]
    public class SpaceEvent : ISerializableReference
    {
        public List<SerializableCall> actions;

        [System.Serializable]
        public class SerializableArgument
        {
            public Object m_ObjectArgument;
            public string m_ObjectArgumentID;
            public int m_IntArgument;
            public float m_FloatArgument;
            public string m_StringArgument;
            public bool m_BoolArgument;

            public SerializableArgument()
            {
            }
        }


        [System.Serializable]
        public class SerializableCall
        {
            public UnityEngine.Object m_Target;
            public string m_TargetID;
            public string m_MethodName;
            public PersistentListenerMode m_Mode = PersistentListenerMode.EventDefined;
            public SerializableArgument m_Argument;

            public SerializableCall()
            {

            }


        }

        public static implicit operator UnityEvent(SpaceEvent spaceEvent)
        {
            var unityEvent = new UnityEvent();
            return unityEvent;
        }

        public List<Object> GetReferencedObjects()
        {
            var references = new List<Object>();

            foreach (var call in actions)
            {
                if (call.m_Target && !references.Contains(call.m_Target))
                {
                    references.Add(call.m_Target);
                }

                if (call.m_Mode == PersistentListenerMode.Object)
                {
                    if (call.m_Argument.m_ObjectArgument && !references.Contains(call.m_Argument.m_ObjectArgument))
                        references.Add(call.m_Argument.m_ObjectArgument);
                }
            }

            return references;
        }

        public List<string> GetReferenceIDs()
        {
            var referenceIDs = new List<string>();

            foreach (var call in actions)
            {
                if (!string.IsNullOrEmpty(call.m_TargetID) && !referenceIDs.Contains(call.m_TargetID))
                {
                    referenceIDs.Add(call.m_TargetID);
                }

                if (call.m_Mode == PersistentListenerMode.Object)
                {
                    if (!string.IsNullOrEmpty(call.m_Argument.m_ObjectArgumentID) && !referenceIDs.Contains(call.m_Argument.m_ObjectArgumentID))
                        referenceIDs.Add(call.m_Argument.m_ObjectArgumentID);
                }
            }

            return referenceIDs;
        }

        public void SetReferenceID(Object obj, string referenceID)
        {
            foreach (var call in actions)
            {
                if (call.m_Target.Equals(obj))
                    call.m_TargetID = referenceID;

                if (call.m_Mode == PersistentListenerMode.Object && call.m_Argument.m_ObjectArgument.Equals(obj))
                    call.m_Argument.m_ObjectArgumentID = referenceID;
            }
        }

        public void SetReferencedObject(string referenceID, Object obj)
        {
            foreach (var call in actions)
            {
                if (call.m_TargetID == referenceID)
                    call.m_Target = obj;

                if (call.m_Mode == PersistentListenerMode.Object && call.m_Argument.m_ObjectArgumentID == referenceID)
                    call.m_Argument.m_ObjectArgument = obj;
            }
        }
    }


    [System.Serializable]
    public class MinMaxCurve
    {
        /// <summary>
        /// A single constant value for the entire curve.
        /// </summary>
        /// <param name="constant">Constant value.</param>
        public MinMaxCurve(float constant)
        {
            this.constant = constant;
            mode = ParticleSystemCurveMode.Constant;
        }

        /// <summary>
        /// Use one curve when evaluating numbers along this Min-Max curve.
        /// </summary>
        /// <param name="multiplier">A multiplier to be applied to the curve.</param>
        /// <param name="curve">A single curve for evaluating against.</param>
        public MinMaxCurve(float multiplier, AnimationCurve curve)
        {
            curveMultiplier = multiplier;
            this.curve = curve;
            mode = ParticleSystemCurveMode.Curve;
        }

        /// <summary>
        /// Randomly select values based on the interval between the minimum and maximum constants.
        /// </summary>
        /// <param name="min">The constant describing the minimum values to be evaluated.</param>
        /// <param name="max">The constant describing the maximum values to be evaluated.</param>
        public MinMaxCurve(float min, float max)
        {
            constantMin = min;
            constantMax = max;
            mode = ParticleSystemCurveMode.TwoConstants;
        }

        /// <summary>
        /// Randomly select values based on the interval between the minimum and maximum curves.
        /// </summary>
        /// <param name="multiplier">A multiplier to be applied to the curves.</param>
        /// <param name="min">The curve describing the minimum values to be evaluated.</param>
        /// <param name="max">The curve describing the maximum values to be evaluated.</param>
        public MinMaxCurve(float multiplier, AnimationCurve min, AnimationCurve max)
        {
            curveMultiplier = multiplier;
            curveMin = min;
            curveMax = max;
            mode = ParticleSystemCurveMode.TwoCurves;
        }

        public float constant;
        public float constantMax;
        public float constantMin;

        public AnimationCurve curve;
        public AnimationCurve curveMax;
        public AnimationCurve curveMin;
        public float curveMultiplier;

        public ParticleSystemCurveMode mode;

        public float Evaluate(float time)
        {
            return ((ParticleSystem.MinMaxCurve)this).Evaluate(time);
        }


        public float Evaluate(float time, float lerpFactor)
        {
            return ((ParticleSystem.MinMaxCurve)this).Evaluate(time, lerpFactor);
        }

        public static implicit operator MinMaxCurve(float constant)
        {
            return new MinMaxCurve(constant);
        }

        public static implicit operator MinMaxCurve(ParticleSystem.MinMaxCurve pMinMaxCurve)
        {
            switch (pMinMaxCurve.mode)
            {
                case ParticleSystemCurveMode.Constant:
                    return new MinMaxCurve(pMinMaxCurve.constant);
                case ParticleSystemCurveMode.Curve:
                    return new MinMaxCurve(pMinMaxCurve.curveMultiplier, pMinMaxCurve.curve);
                case ParticleSystemCurveMode.TwoConstants:
                    return new MinMaxCurve(pMinMaxCurve.constantMin, pMinMaxCurve.constantMax);
                case ParticleSystemCurveMode.TwoCurves:
                    return new MinMaxCurve(pMinMaxCurve.curveMultiplier, pMinMaxCurve.curveMin, pMinMaxCurve.curveMax);
                default:
                    return new MinMaxCurve(0);
            }
        }

        public static implicit operator ParticleSystem.MinMaxCurve(MinMaxCurve sMinMaxCurve)
        {
            if (sMinMaxCurve == null)
                sMinMaxCurve = new MinMaxCurve(1);

            switch (sMinMaxCurve.mode)
            {
                case ParticleSystemCurveMode.Constant:
                    return new ParticleSystem.MinMaxCurve(sMinMaxCurve.constant);
                case ParticleSystemCurveMode.Curve:
                    return new ParticleSystem.MinMaxCurve(sMinMaxCurve.curveMultiplier, sMinMaxCurve.curve);
                case ParticleSystemCurveMode.TwoConstants:
                    return new ParticleSystem.MinMaxCurve(sMinMaxCurve.constantMin, sMinMaxCurve.constantMax);
                case ParticleSystemCurveMode.TwoCurves:
                    return new ParticleSystem.MinMaxCurve(sMinMaxCurve.curveMultiplier, sMinMaxCurve.curveMin, sMinMaxCurve.curveMax);
                default:
                    return new ParticleSystem.MinMaxCurve(0);
            }
        }
    }

    [System.Serializable]
    public class MinMaxGradient
    {
        /// <summary>
        /// A single constant color for the entire gradient.
        /// </summary>
        /// <param name="color">Constant color.</param>
        public MinMaxGradient(Color color)
        {
            this.color = color;
            mode = ParticleSystemGradientMode.Color;
        }

        /// <summary>
        /// Use one gradient when evaluating numbers along this Min-Max gradient.
        /// </summary>
        /// <param name="gradient">A single gradient for evaluating against.</param>
        public MinMaxGradient(Gradient gradient)
        {
            this.gradient = gradient;
            mode = ParticleSystemGradientMode.Gradient;
        }

        /// <summary>
        /// Randomly select colors based on the interval between the minimum and maximum constants.
        /// </summary>
        /// <param name="min">The constant color describing the minimum colors to be evaluated.</param>
        /// <param name="max">The constant color describing the maximum colors to be evaluated.</param>
        public MinMaxGradient(Color min, Color max)
        {
            colorMin = min;
            colorMax = max;
            mode = ParticleSystemGradientMode.TwoColors;
        }

        /// <summary>
        /// Randomly select colors based on the interval between the minimum and maximum gradients.
        /// </summary>
        /// <param name="min">The gradient describing the minimum colors to be evaluated.</param>
        /// <param name="max">The gradient describing the maximum colors to be evaluated.</param>
        public MinMaxGradient(Gradient min, Gradient max)
        {
            gradientMin = min;
            gradientMax = max;
            mode = ParticleSystemGradientMode.TwoGradients;
        }

        public Color color;
        public Color colorMax;
        public Color colorMin;

        public Gradient gradient;
        public Gradient gradientMax;
        public Gradient gradientMin;

        public ParticleSystemGradientMode mode;

        /// <summary>
        /// Manually query the gradient to calculate colors based on what mode it is in.
        /// </summary>
        /// <param name="time">Normalized time (in the range 0 - 1, where 1 represents 100%) at which to evaluate the gradient. This is valid when ParticleSystem.MinMaxGradient.mode is set to ParticleSystemGradientMode.Gradient or ParticleSystemGradientMode.TwoGradients.</param>
        /// <returns>Calculated gradient/color value.</returns>
        public Color Evaluate(float time)
        {
            return ((ParticleSystem.MinMaxGradient)this).Evaluate(time);
        }

        /// <summary>
        /// Manually query the gradient to calculate colors based on what mode it is in.
        /// </summary>
        /// <param name="time">Normalized time (in the range 0 - 1, where 1 represents 100%) at which to evaluate the gradient. This is valid when ParticleSystem.MinMaxGradient.mode is set to ParticleSystemGradientMode.Gradient or ParticleSystemGradientMode.TwoGradients.</param>
        /// <param name="lerpFactor">Blend between the 2 gradients/colors (Valid when ParticleSystem.MinMaxGradient.mode is set to ParticleSystemGradientMode.TwoColors or ParticleSystemGradientMode.TwoGradients).</param>
        /// <returns>Calculated gradient/color value.</returns>
        public Color Evaluate(float time, float lerpFactor)
        {
            return ((ParticleSystem.MinMaxGradient)this).Evaluate(time, lerpFactor);
        }


        public static implicit operator MinMaxGradient(Color color)
        {
            return new MinMaxGradient(color);
        }

        public static implicit operator MinMaxGradient(Gradient gradient)
        {
            return new MinMaxGradient(gradient);
        }

        public static implicit operator MinMaxGradient(ParticleSystem.MinMaxGradient pMinMaxGradient)
        {
            switch (pMinMaxGradient.mode)
            {
                case ParticleSystemGradientMode.RandomColor:
                    var randomColor = new MinMaxGradient(pMinMaxGradient.color);
                    randomColor.mode = ParticleSystemGradientMode.RandomColor;
                    return randomColor;
                case ParticleSystemGradientMode.Color:
                    return new MinMaxGradient(pMinMaxGradient.color);
                case ParticleSystemGradientMode.TwoColors:
                    return new MinMaxGradient(pMinMaxGradient.colorMin, pMinMaxGradient.colorMax);
                case ParticleSystemGradientMode.Gradient:
                    return new MinMaxGradient(pMinMaxGradient.gradient);
                case ParticleSystemGradientMode.TwoGradients:
                    return new MinMaxGradient(pMinMaxGradient.gradientMin, pMinMaxGradient.gradientMax);
                default:
                    return new MinMaxGradient(pMinMaxGradient.color);
            }
        }

        public static implicit operator ParticleSystem.MinMaxGradient(MinMaxGradient sMinMaxGradient)
        {
            if (sMinMaxGradient == null)
                sMinMaxGradient = new Core.MinMaxGradient(Color.white);

            switch (sMinMaxGradient.mode)
            {
                case ParticleSystemGradientMode.RandomColor:
                    var randomColor = new ParticleSystem.MinMaxGradient(sMinMaxGradient.color);
                    randomColor.mode = ParticleSystemGradientMode.RandomColor;
                    return randomColor;
                case ParticleSystemGradientMode.Color:
                    return new ParticleSystem.MinMaxGradient(sMinMaxGradient.color);
                case ParticleSystemGradientMode.TwoColors:
                    return new ParticleSystem.MinMaxGradient(sMinMaxGradient.colorMin, sMinMaxGradient.colorMax);
                case ParticleSystemGradientMode.Gradient:
                    return new ParticleSystem.MinMaxGradient(sMinMaxGradient.gradient);
                case ParticleSystemGradientMode.TwoGradients:
                    return new ParticleSystem.MinMaxGradient(sMinMaxGradient.gradientMin, sMinMaxGradient.gradientMax);
                default:
                    return new ParticleSystem.MinMaxGradient();
            }
        }
    }

    #region Interfaces

    [System.Serializable]
    public class TextureProperty : IProgressive
    {
        public string assetId;
        public string url;
        [Range(1, 9)]
        public int anisoLevel = 1;
        public TextureWrapMode wrapMode = TextureWrapMode.Clamp;
        public bool useGrayscaleForAlpha;
        public Asset asset { get; set; }
        public Texture value { get; set; }
        public bool downloading { get; private set; }
        public bool isDone { get; private set; }
        public float progress { get; private set; }
        public string progressMessage { get; private set; }

        public void Fetch(Widget widget, System.Action onComplete)
        {
            if (!string.IsNullOrEmpty(assetId))
            {
                widget.StartCoroutine(FetchAsset(widget, onComplete));
            }
            else if (!string.IsNullOrEmpty(url))
            {
                widget.StartCoroutine(FetchTexture(onComplete));
            }
        }

        public IEnumerator FetchAsset(Widget widget, System.Action onComplete)
        {
            if (!string.IsNullOrEmpty(assetId))
            {
                asset = Asset.AssetsManager.AddAsset(assetId);

                if (asset != null)
                {
                    if (asset.texture)
                    {
                        value = asset.texture;
                    }
                    else
                    {
                        asset.onProcessEnd += OnAssetUpdate;

                        if (!asset.isLoaded && !asset.texture)
                            yield return widget.StartCoroutine(asset.LoadAssetInBackground(widget));

                        while (!value && !asset.InProcess(Asset.Process.Error))
                            yield return null;
                    }

                    CorrectTexture(value);

                    if (onComplete != null)
                        onComplete();
                }
            }
        }

        public IEnumerator FetchTexture(System.Action onComplete)
        {
            downloading = true;

            using (var request = UnityEngine.Networking.UnityWebRequest.GetTexture(url))
            {
                var async = request.Send();
                progressMessage = "Downloading";

                while (!async.isDone)
                {
                    yield return async.isDone;
                    isDone = async.isDone;
                    progress = async.progress;
                }

                if (request.isError)
                {
                    Debug.Log(request.error);
                    progressMessage = "Error";
                }
                else
                {
                    value = ((UnityEngine.Networking.DownloadHandlerTexture)request.downloadHandler).texture;
                    progressMessage = "Done";
                }

            }

            isDone = true;
            downloading = false;

            CorrectTexture(value);

            if (onComplete != null)
                onComplete();
        }

        protected void OnAssetUpdate(Asset sender, Asset.Process[] currentProcesses, Asset.Process endingProcess)
        {
            if (endingProcess == Asset.Process.Downloading)
            {
                value = sender.texture;
                asset.onProcessEnd -= OnAssetUpdate;
            }
        }

        public string GetProgressMessage()
        {
            return progressMessage;
        }

        public float GetProgress()
        {
            return progress;
        }

        public void CorrectTexture(Texture texture)
        {
            texture.wrapMode = wrapMode;

            if (useGrayscaleForAlpha)
            {
                var modTexture = texture as Texture2D;

                if (modTexture)
                {
                    var pixels = modTexture.GetPixels();

                    for (int i = 0; i < pixels.Length; i++)
                    {
                        pixels[i] = new Color(pixels[i].r, pixels[i].g, pixels[i].b, pixels[i].grayscale);
                    }

                    modTexture.SetPixels(pixels);
                    modTexture.Apply();
                    texture = modTexture;
                }
            }

        }
    }

    /// <summary>
    /// Interface which will receive event for start/end of Edit mode.
    /// </summary>

    public interface IEditable
    {
        void OnEditStart();
        void OnEditEnd();
    }


    /// <summary>
    /// Interface for querying referenced asset IDs.
    /// </summary>
    public interface IAssetReference
    {
        string[] GetAssetIDs();
    }

    public interface ISerializableReference
    {
        List<Object> GetReferencedObjects();
        List<string> GetReferenceIDs();
        void SetReferenceID(Object obj, string referenceID);
        void SetReferencedObject(string referenceID, Object obj);
    }

    /// <summary>
    /// Interface for any asset or asset-like thing that can be "Played".
    /// </summary>

    public interface IPlayable
    {
        void Play();
        void Pause();
        void Stop();
        void Restart();
    }

    /// <summary>
    /// Interface for things that have a continuous sequence it makes sense to slide to any point, forwards or backwards, along.
    /// </summary>

    public interface IScrubbable
    {
        void SeekTo(float cue);
        void Speed(float factor);
    }

    /// <summary>
    /// Interface for things that have a defined set of items to step through in order.
    /// </summary>

    public interface IPlaylist
    {
        void Next();
        void Previous();
        void First();
        void Last();
        void Select(int index);
    }

    public interface IComponentWidget
    {
        void AddWidget();
        void RemoveWidget();
    }

    public interface IDisplay<T>
    {
        void SetContent(T content);
    }

    #endregion
}
