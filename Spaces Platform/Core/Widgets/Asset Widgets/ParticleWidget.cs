using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Spaces.Extensions;

namespace Spaces.Core
{
    public class ParticleWidget : AssetHandlerWidget, IPlayable, IAssetReference
    {
        [System.Serializable]
        public class Burst
        {
            public Burst() { }

            public Burst(ParticleSystem.Burst burst)
            {
                this.time = burst.time;
                this.minCount = burst.minCount;
                this.maxCount = burst.maxCount;
            }

            /// <summary>
            /// Construct a new Burst with a time and count.
            /// </summary>
            /// <param name="time">Time to emit the burst.</param>
            /// <param name="count">Number of particles to emit.</param>
            public Burst(float time, short count)
            {
                this.time = time;
                this.minCount = count;
                this.maxCount = count;
            }
            /// <summary>
            /// Construct a new Burst with a time and count.
            /// </summary>
            /// <param name="time">Time to emit the burst.</param>
            /// <param name="minCount">Minimum number of particles to emit.</param>
            /// <param name="maxCount">Maximum number of particles to emit.</param>
            public Burst(float time, short minCount, short maxCount)
            {
                this.time = time;
                this.minCount = minCount;
                this.maxCount = maxCount;
            }

            /// <summary>
            /// Maximum number of bursts to be emitted.
            /// </summary>
            public short maxCount;

            /// <summary>
            /// Minimum number of bursts to be emitted.
            /// </summary>
            public short minCount;

            /// <summary>
            /// The time that each burst occurs.
            /// </summary>
            public float time;

            public static implicit operator Burst(ParticleSystem.Burst burst)
            {
                return new Burst(burst);
            }

            public static implicit operator ParticleSystem.Burst(Burst burst)
            {
                if (burst.minCount == burst.maxCount)
                    return new ParticleSystem.Burst(burst.time, burst.minCount);
                else
                    return new ParticleSystem.Burst(burst.time, burst.minCount, burst.maxCount);
            }
        }

        public bool useAutoRandomSeed = true;
        public uint randomSeed;

        public MainModule mainModule;
        public EmissionModule emission;
        public ShapeModule shape;
        public ShapeModel shapeModel;
        public VelocityOverLifetimeModule velocityOverLifetime;
        public LimitVelocityOverLifetimeModule limitVelocityOverLifetime;
        public InheritVelocityModule inheritVelocity;
        public ForceOverLifetimeModule forceOverLifetime;
        public ColorOverLifetimeModule colorOverLifetime;
        public ColorBySpeedModule colorBySpeed;
        public SizeOverLifetimeModule sizeOverLifetime;
        public SizeBySpeedModule sizeBySpeed;
        public RotationOverLifetimeModule rotationOverLifetime;
        public RotationBySpeedModule rotationBySpeed;
        public ExternalForcesModule externalForces;
        public NoiseModule noise;
        public CollisionModule collision;
        public TriggerModule trigger;
        public SubEmittersModule subEmitters;
        public TextureSheetAnimationModule textureSheetAnimation;
        public LightsModule lights;
        public TrailModule trails;
        public RenderModule render;

        public ShapeModel renderModel;

        private ParticleSystem m_particleSystem;
        private ParticleSystemRenderer m_particleRenderer;
        private MaterialWidget m_particleMaterialWidget;
        private MaterialWidget m_trailMaterialWidget;
        /*
        /// <summary>
        /// Is the particle system currently emitting particles? A particle system may stop emitting when its emission module has finished, it has been paused or if the system has been stopped using ParticleSystem.Stop|Stop with the ParticleSystemStopBehavior.StopEmitting|StopEmitting flag. Resume emitting by calling ParticleSystem.Play|Play.
        /// </summary>
        public bool isEmitting { get; private set; }

        public bool isPaused { get; private set; }

        public bool isPlaying { get; private set; }

        public bool isStopped { get; private set; }

        public int particleCount
        {
            get
            {
                return m_particleSystem.particleCount;
            }
        }

        public float time { get; set; }
        

        /// <summary>
        /// Remove all particles in the particle system.
        /// </summary>
        /// <param name="withChildren">Clear all child particle systems as well.</param>
        public void Clear(bool withChildren = true)
        { }

        /// <summary>
        /// Emit count particles immediately.
        /// </summary>
        /// <param name="count">Number of particles to emit.</param>
        public void Emit(int count)
        { }
        public void Emit(ParticleSystem.EmitParams emitParams, int count)
        { }
        public int GetCustomParticleData(List<Vector4> customData, ParticleSystemCustomData streamIndex)
        { return 0; }

        /// <summary>
        /// Does the system have any live particles (or will produce more)?
        /// </summary>
        /// <param name="withChildren">Check all child particle systems as well.</param>
        /// <returns>True if the particle system is still "alive", false if the particle system is done emitting particles and all particles are dead.</returns>
        public bool IsAlive(bool withChildren = true)
        { return false; }

        /// <summary>
        /// Pauses the system so no new particles are emitted and the existing particles are not updated.
        /// </summary>
        /// <param name="withChildren">Pause all child particle systems as well.</param>
        public void Pause(bool withChildren = true)
        { }

        /// <summary>
        /// Starts the particle system.
        /// </summary>
        /// <param name="withChildren">Play all child particle systems as well.</param>
        public void Play(bool withChildren = true)
        { }
        public void SetCustomParticleData(List<Vector4> customData, ParticleSystemCustomData streamIndex)
        { }

        /// <summary>
        /// Fastforwards the particle system by simulating particles over given period of time, then pauses it.
        /// </summary>
        /// <param name="t">Time period in seconds to advance the ParticleSystem simulation by. If restart is true, the ParticleSystem will be reset to 0 time, and then advanced by this value. If restart is false, the ParticleSystem simulation will be advanced in time from its current state by this value.</param>
        /// <param name="withChildren">Fastforward all child particle systems as well.</param>
        /// <param name="restart">Restart and start from the beginning.</param>
        /// <param name="fixedTimeStep">Only update the system at fixed intervals, based on the value in "Fixed Time" in the Time options.</param>
        public void Simulate(float t, bool withChildren = true, bool restart = true, bool fixedTimeStep = true)
        { }

        /// <summary>
        /// Stops playing the particle system using the supplied stop behaviour.
        /// </summary>
        /// <param name="withChildren">Stop all child particle systems as well.</param>
        /// <param name="stopBehavior">Stop emitting or stop emitting and clear the system.</param>
        public void Stop(bool withChildren = true, ParticleSystemStopBehavior stopBehavior = ParticleSystemStopBehavior.StopEmitting)
        { }
        */


        protected override void Start()
        {
            base.Start();

            //Debug.Log("playing " + m_particleSystem.isPlaying);
            //Debug.Log("paused " + m_particleSystem.isPaused);
            //Debug.Log("stopped " + m_particleSystem.isStopped);
            //Debug.Log("emitting " + m_particleSystem.isEmitting);
            //Debug.Log("alive " + m_particleSystem.IsAlive());
        }
        public override void Initialize()
        {
            base.Initialize();
            Initialize(m_particleSystem);            
        }

        public void Initialize(ParticleSystem particleSystem)
        {
            base.Initialize();

            if (particleSystem)
            {
                m_particleSystem = particleSystem;
                m_particleRenderer = m_particleSystem.GetComponent<ParticleSystemRenderer>();

                shapeModel.Initialize(this, delegate { shape.CopyTo(m_particleSystem.shape); });
                renderModel.Initialize(this, delegate { render.CopyTo(m_particleRenderer); });

                UpdateMaterials();
                UpdateWidget();
            }
            else
            {
                GetParticleSystem();
            }

            if (!m_particleSystem.isPlaying)
                m_particleSystem.Play();

        }

        public void GetParticleSystem()
        {
            if (!m_particleSystem)
                m_particleSystem = GetComponentInChildren<ParticleSystem>();

            if (m_particleSystem)
            {
                m_particleRenderer = m_particleSystem.GetComponent<ParticleSystemRenderer>();

                shapeModel.Initialize(this, delegate { shape.CopyTo(m_particleSystem.shape); });
                renderModel.Initialize(this, delegate { render.CopyTo(m_particleRenderer); });

                UpdateMaterials();
                UpdateWidget();
            }
            else
            {
                m_particleSystem = gameObject.AddComponent<ParticleSystem>();
                m_particleRenderer = m_particleSystem.GetComponent<ParticleSystemRenderer>();

                shapeModel.Initialize(this, delegate { shape.CopyTo(m_particleSystem.shape); });
                renderModel.Initialize(this, delegate { render.CopyTo(m_particleRenderer); });

                UpdateMaterials();

                if (mainModule.playOnAwake)
                    m_particleSystem.Play();

                UpdateParticleSystem();
            }

        }

        private void UpdateMaterials()
        {
            var materialWidgets = MaterialWidget.GenerateMaterialWidgets(this);

            if (materialWidgets.Count > 0)
            {
                m_particleMaterialWidget = materialWidgets[0];
                render.material = materialWidgets[0].m_material;

                if (materialWidgets.Count > 1)
                {
                    m_trailMaterialWidget = materialWidgets[1];
                    render.trailMaterial = materialWidgets[1].m_material;
                }
            }
        }

        public void UpdateParticleSystem()
        {
            if (!m_particleSystem)
            {
                Debug.LogWarning(this.name + " [No Particle System To Update]");
                return;
            }


            bool wasPlaying = m_particleSystem.isPlaying;
            //Debug.Log("Was Playing: " + wasPlaying);

            m_particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            m_particleSystem.Pause();//.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            //Debug.Log("Now: " + m_particleSystem.isPlaying);

            if (m_particleRenderer)
            {
                render.model = renderModel;
                render.CopyTo(m_particleRenderer);
            }

            mainModule.CopyTo(m_particleSystem.main);
            emission.CopyTo(m_particleSystem.emission);

            shape.model = shapeModel;
            shape.CopyTo(m_particleSystem.shape);

            velocityOverLifetime.CopyTo(m_particleSystem.velocityOverLifetime);
            limitVelocityOverLifetime.CopyTo(m_particleSystem.limitVelocityOverLifetime);
            inheritVelocity.CopyTo(m_particleSystem.inheritVelocity);
            forceOverLifetime.CopyTo(m_particleSystem.forceOverLifetime);
            colorOverLifetime.CopyTo(m_particleSystem.colorOverLifetime);
            colorBySpeed.CopyTo(m_particleSystem.colorBySpeed);
            sizeOverLifetime.CopyTo(m_particleSystem.sizeOverLifetime);
            sizeBySpeed.CopyTo(m_particleSystem.sizeBySpeed);
            rotationOverLifetime.CopyTo(m_particleSystem.rotationOverLifetime);
            rotationBySpeed.CopyTo(m_particleSystem.rotationBySpeed);
            externalForces.CopyTo(m_particleSystem.externalForces);
            noise.CopyTo(m_particleSystem.noise);
            collision.CopyTo(m_particleSystem.collision);
            trigger.CopyTo(m_particleSystem.trigger);
            //subEmitters.CopyTo(m_particleSystem.subEmitters);
            textureSheetAnimation.CopyTo(m_particleSystem.textureSheetAnimation);
            lights.CopyTo(m_particleSystem.lights);
            trails.CopyTo(m_particleSystem.trails);


            if (wasPlaying)
            {
                m_particleSystem.Stop();
                m_particleSystem.Play();
            }

        }

        public void UpdateWidget()
        {
            UpdateWidget(m_particleSystem);
        }

        public void UpdateWidget(ParticleSystem particleSystem)
        {
            if (!particleSystem)
            {
                ResetToDefaults();
                return;
            }

            if (m_particleRenderer)
            {
                render = new RenderModule(m_particleRenderer);
                render.model = renderModel;
            }

            mainModule = particleSystem.main;
            emission = particleSystem.emission;
            shape = particleSystem.shape;
            shape.model = shapeModel;
            velocityOverLifetime = particleSystem.velocityOverLifetime;
            limitVelocityOverLifetime = particleSystem.limitVelocityOverLifetime;
            inheritVelocity = particleSystem.inheritVelocity;
            forceOverLifetime = particleSystem.forceOverLifetime;
            colorOverLifetime = particleSystem.colorOverLifetime;
            colorBySpeed = particleSystem.colorBySpeed;
            sizeOverLifetime = particleSystem.sizeOverLifetime;
            sizeBySpeed = particleSystem.sizeBySpeed;
            rotationOverLifetime = particleSystem.rotationOverLifetime;
            rotationBySpeed = particleSystem.rotationBySpeed;
            externalForces = particleSystem.externalForces;
            noise = particleSystem.noise;
            collision = particleSystem.collision;
            trigger = particleSystem.trigger;
            //subEmitters = particleSystem.subEmitters;
            textureSheetAnimation = particleSystem.textureSheetAnimation;
            lights = particleSystem.lights;
            trails = particleSystem.trails;
            //Debug.Log(JsonUtility.ToJson(this));
        }

        private void ResetToDefaults()
        {

        }

        public override GameObject InstancePlaceholder()
        {
            throw new System.NotImplementedException();
        }

        //private void Update()
        //{
        //    if (Input.GetKeyDown(KeyCode.P))
        //        Play();

        //    if (Input.GetKeyDown(KeyCode.O))
        //        Stop();

        //    if (Input.GetKeyDown(KeyCode.L))
        //        Pause();
        //}

        public void Play()
        {
            m_particleSystem.Play();
        }

        public void Pause()
        {
            m_particleSystem.Pause();
        }

        public void Stop()
        {
            m_particleSystem.Stop();
        }

        public void Restart()
        {
            Stop();
            Play();
        }

        public string[] GetAssetIDs()
        {
            List<string> assetIDs = new List<string>();

            if (shape.model != null && !string.IsNullOrEmpty(shape.model.assetID))
                assetIDs.Add(shape.model.assetID);

            if (render.renderMode == ParticleSystemRenderMode.Mesh && render.model != null && !string.IsNullOrEmpty(render.model.assetID))
                assetIDs.Add(render.model.assetID);

            return assetIDs.ToArray();
        }

        [System.Serializable]
        public class RenderModule
        {
            public ParticleSystemRenderer renderer { get; set; }
            public ShapeModel model { get; set; }

            /// <summary>
            /// Control the direction that particles face.
            /// </summary>
            public ParticleSystemRenderSpace alignment;

            /// <summary>
            /// How much are the particles stretched depending on the Camera's speed.
            /// </summary>
            public float cameraVelocityScale;

            /// <summary>
            /// How much are the particles stretched in their direction of motion.
            /// </summary>
            public float lengthScale;

            /// <summary>
            /// Clamp the maximum particle size.
            /// </summary>
            public float maxParticleSize;

            /// <summary>
            /// Mesh used as particle instead of billboarded texture.
            /// </summary>
            public Mesh mesh
            {
                get
                {
                    if (model != null && model.gameObject)
                    {
                        var filter = model.gameObject.GetComponentInChildren<MeshFilter>();
                        return filter ? filter.sharedMesh : null;
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            /// <summary>
            /// The number of meshes being used for particle rendering.
            /// </summary>
            public int meshCount
            {
                get
                {
                    return renderer.meshCount;
                }
            }

            /// <summary>
            /// Clamp the minimum particle size.
            /// </summary>
            public float minParticleSize;

            /// <summary>
            /// How much are billboard particle normals oriented towards the camera.
            /// </summary>
            public float normalDirection;

            /// <summary>
            /// Modify the pivot point used for rotating particles.
            /// </summary>
            public Vector3 pivot;

            /// <summary>
            /// How particles are drawn.
            /// </summary>
            public ParticleSystemRenderMode renderMode = ParticleSystemRenderMode.Billboard;

            /// <summary>
            /// Biases particle system sorting amongst other transparencies.
            /// </summary>
            public float sortingFudge;

            /// <summary>
            /// Sort particles within a system.
            /// </summary>
            public ParticleSystemSortMode sortMode = ParticleSystemSortMode.None;

            public Material material;

            /// <summary>
            /// Set the material used by the Trail module for attaching trails to particles.
            /// </summary>
            public Material trailMaterial;

            /// <summary>
            /// How much are the particles stretched depending on "how fast they move".
            /// </summary>
            public float velocityScale;

            public RenderModule()
            {
                Initialize();
            }

            public RenderModule(ParticleSystemRenderer renderer)
            {
                this.renderer = renderer;
                this.alignment = renderer.alignment;
                this.cameraVelocityScale = renderer.cameraVelocityScale;
                this.lengthScale = renderer.lengthScale;
                this.maxParticleSize = renderer.maxParticleSize;
                //this.mesh = renderer.mesh;
                this.minParticleSize = renderer.minParticleSize;
                this.normalDirection = renderer.normalDirection;
                this.pivot = renderer.pivot;
                this.renderMode = renderer.renderMode;
                this.sortingFudge = renderer.sortingFudge;
                this.sortMode = renderer.sortMode;
                this.material = renderer.material;
                this.trailMaterial = renderer.trailMaterial;
                this.velocityScale = renderer.velocityScale;
            }


            public void Initialize()
            {
                alignment = ParticleSystemRenderSpace.View;
                cameraVelocityScale = 0;
                lengthScale = 2;
                maxParticleSize = 0.5f;
                minParticleSize = 0;
                normalDirection = 1;
                pivot = Vector3.zero;
                renderMode = ParticleSystemRenderMode.Billboard;
                sortingFudge = 0;
                sortMode = ParticleSystemSortMode.None;
                velocityScale = 0;
            }

            /// <summary>
            /// Query whether the particle system renderer uses a particular set of vertex streams.
            /// </summary>
            /// <param name="streams">Streams to query.</param>
            /// <returns>Whether all the queried streams are enabled or not.</returns>
            public bool AreVertexStreamsEnabled(ParticleSystemVertexStreams streams)
            {
                return renderer.AreVertexStreamsEnabled(streams);
            }

            /// <summary>
            /// Disable a set of vertex shader streams on the particle system renderer. /nThe position stream is always enabled, and any attempts to remove it will be ignored.
            /// </summary>
            /// <param name="streams">Streams to disable.</param>
            public void DisableVertexStreams(ParticleSystemVertexStreams streams)
            {
                renderer.DisableVertexStreams(streams);
            }

            /// <summary>
            /// Enable a set of vertex shader streams on the particle system renderer.
            /// </summary>
            /// <param name="streams">Streams to enable.</param>
            public void EnableVertexStreams(ParticleSystemVertexStreams streams)
            {
                renderer.EnableVertexStreams(streams);
            }

            /// <summary>
            /// Query whether the particle system renderer uses a particular set of vertex streams.
            /// </summary>
            /// <param name="streams">Streams to query.</param>
            /// <returns>Returns the subset of the queried streams that are actually enabled.</returns>
            public ParticleSystemVertexStreams GetEnabledVertexStreams(ParticleSystemVertexStreams streams)
            {
                return renderer.GetEnabledVertexStreams(streams);
            }

            /// <summary>
            /// Set the array of meshes used as particles.
            /// </summary>
            /// <param name="meshes">This array will be populated with the list of meshes being used for particle rendering.</param>
            /// <returns>The number of meshes actually written to the destination array.</returns>
            public int GetMeshes(Mesh[] meshes)
            {
                return renderer.GetMeshes(meshes);
            }

            /// <summary>
            /// Set an array of meshes used as particles instead of a billboarded texture.
            /// </summary>
            /// <param name="meshes">Array of meshes to be used.</param>
            public void SetMeshes(Mesh[] meshes)
            {
                renderer.SetMeshes(meshes);
            }

            /// <summary>
            /// Set an array of meshes used as particles instead of a billboarded texture.
            /// </summary>
            /// <param name="meshes">Array of meshes to be used.</param>
            /// <param name="size">Number of elements from the mesh array to be applied.</param>
            public void SetMeshes(Mesh[] meshes, int size)
            {
                renderer.SetMeshes(meshes, size);
            }

            public void CopyTo(ParticleSystemRenderer renderer)
            {
                renderer.alignment = alignment;
                renderer.cameraVelocityScale = cameraVelocityScale;
                renderer.lengthScale = lengthScale;
                renderer.maxParticleSize = maxParticleSize;
                renderer.mesh = mesh;
                renderer.minParticleSize = minParticleSize;
                renderer.normalDirection = normalDirection;
                renderer.pivot = pivot;
                renderer.renderMode = renderMode;
                renderer.sortingFudge = sortingFudge;
                renderer.sortMode = sortMode;
                renderer.material = material;
                renderer.trailMaterial = trailMaterial;
                renderer.velocityScale = velocityScale;
            }
        }

        [System.Serializable]
        public class MainModule
        {
            /// <summary>
            /// The duration of the particle system in seconds.
            /// </summary>
            public float duration;
            /// <summary>
            /// Is the particle system looping?
            /// </summary>
            public bool loop;
            /// <summary>
            /// When looping is enabled, this controls whether this particle system will look like it has already simulated for one loop when first becoming visible.
            /// </summary>
            public bool prewarm;
            /// <summary>
            /// The maximum number of particles to emit.
            /// </summary>
            public int maxParticles;
            /// <summary>
            /// If set to true, the particle system will automatically start playing on startup.
            /// </summary>
            public bool playOnAwake;

            /// <summary>
            /// Simulate particles relative to a custom transform component.
            /// </summary>
            public Transform customSimulationSpace;

            [Space()]
            /// <summary>
            /// Start delay in seconds
            /// </summary>
            public MinMaxCurve startDelay;
            /// <summary>
            /// Start delay multiplier in seconds.
            /// </summary>
            public float startDelayMultiplier;
            /// <summary>
            /// The total lifetime in seconds that each new particle will have.
            /// </summary>
            public MinMaxCurve startLifetime;
            /// <summary>
            /// Start lifetime multiplier.
            /// </summary>
            public float startLifetimeMultiplier;
            /// <summary>
            /// The initial speed of particles when emitted.
            /// </summary>
            public MinMaxCurve startSpeed;
            /// <summary>
            /// A multiplier of the initial speed of particles when emitted.
            /// </summary>
            public float startSpeedMultiplier;

            [Space()]
            [Header("Size")]
            /// <summary>
            /// The initial size of particles when emitted.
            /// </summary>
            public MinMaxCurve startSize;
            /// <summary>
            /// Start size multiplier.
            /// </summary>
            public float startSizeMultiplier;

            [Space()]
            [Header("3D Size")]
            /// <summary>
            /// A flag to enable specifying particle size individually for each axis.
            /// </summary>
            public bool startSize3D;
            [Space()]
            /// <summary>
            /// The initial size of particles along the X axis when emitted.
            /// </summary>
            public MinMaxCurve startSizeX;
            /// <summary>
            /// Start rotation multiplier along the X axis.
            /// </summary>
            public float startSizeXMultiplier;
            /// <summary>
            /// The initial size of particles along the Y axis when emitted.
            /// </summary>
            public MinMaxCurve startSizeY;
            /// <summary>
            /// Start rotation multiplier along the Y axis.
            /// </summary>
            public float startSizeYMultiplier;
            /// <summary>
            /// The initial size of particles along the Z axis when emitted.
            /// </summary>
            public MinMaxCurve startSizeZ;
            /// <summary>
            /// Start rotation multiplier along the Z axis.
            /// </summary>
            public float startSizeZMultiplier;


            [Space()]
            [Header("Rotation")]
            /// <summary>
            /// The initial rotation of particles when emitted.
            /// </summary>
            public MinMaxCurve startRotation;
            /// <summary>
            /// Start rotation multiplier.
            /// </summary>
            public float startRotationMultiplier;

            [Space()]
            [Header("3D Rotation")]
            /// <summary>
            /// A flag to enable 3D particle rotation.
            /// </summary>
            public bool startRotation3D;
            [Space()]
            /// <summary>
            /// The initial rotation of particles around the X axis when emitted.
            /// </summary>
            public MinMaxCurve startRotationX;
            /// <summary>
            /// Start rotation multiplier around the X axis.
            /// </summary>
            public float startRotationXMultiplier;
            /// <summary>
            /// The initial rotation of particles around the Y axis when emitted.
            /// </summary>
            public MinMaxCurve startRotationY;
            /// <summary>
            /// Start rotation multiplier around the Y axis.
            /// </summary>
            public float startRotationYMultiplier;
            /// <summary>
            /// The initial rotation of particles around the Z axis when emitted.
            /// </summary>
            public MinMaxCurve startRotationZ;
            /// <summary>
            /// Start rotation multiplier around the Z axis.
            /// </summary>
            public float startRotationZMultiplier;

            [Range(0, 1)]
            /// <summary>
            /// Cause some particles to spin in the opposite direction.
            /// </summary>
            public float randomizeRotationDirection;

            /// <summary>
            /// The initial color of particles when emitted.
            /// </summary>
            public MinMaxGradient startColor;
            /// <summary>
            /// Scale applied to the gravity, defined by Physics.gravity.
            /// </summary>
            public MinMaxCurve gravityModifier;
            /// <summary>
            /// Change the gravity mulutiplier.
            /// </summary>
            public float gravityModifierMultiplier;

            /// <summary>
            /// This selects the space in which to simulate particles. It can be either world or local space.
            /// </summary>
            public ParticleSystemSimulationSpace simulationSpace = ParticleSystemSimulationSpace.Local;
            /// <summary>
            /// Override the default playback speed of the Particle System.
            /// </summary>
            public float simulationSpeed;
            /// <summary>
            /// Control how the particle system's Transform Component is applied to the particle system.
            /// </summary>
            public ParticleSystemScalingMode scalingMode = ParticleSystemScalingMode.Local;

            public MainModule()
            {
                Initialize();
            }

            public MainModule(ParticleSystem.MainModule mainModule)
            {
                this.duration = mainModule.duration;
                this.loop = mainModule.loop;
                this.prewarm = mainModule.prewarm;

                this.customSimulationSpace = mainModule.customSimulationSpace;

                this.startDelay = mainModule.startDelay;
                this.startDelayMultiplier = mainModule.startDelayMultiplier;
                this.startLifetime = mainModule.startLifetime;
                this.startLifetimeMultiplier = mainModule.startLifetimeMultiplier;
                this.startSpeed = mainModule.startSpeed;
                this.startSpeedMultiplier = mainModule.startSpeedMultiplier;

                this.startSize3D = mainModule.startSize3D;
                this.startSize = mainModule.startSize;
                this.startSizeMultiplier = mainModule.startSizeMultiplier;

                this.startSizeX = mainModule.startSizeX;
                this.startSizeXMultiplier = mainModule.startSizeXMultiplier;
                this.startSizeY = mainModule.startSizeY;
                this.startSizeYMultiplier = mainModule.startSizeYMultiplier;
                this.startSizeZ = mainModule.startSizeZ;
                this.startSizeZMultiplier = mainModule.startSizeZMultiplier;

                this.startRotation3D = mainModule.startRotation3D;
                this.startRotation = mainModule.startRotation;
                this.startRotationMultiplier = mainModule.startRotationMultiplier;

                this.startRotationX = mainModule.startRotationX;
                this.startRotationXMultiplier = mainModule.startRotationXMultiplier;
                this.startRotationY = mainModule.startRotationY;
                this.startRotationYMultiplier = mainModule.startRotationYMultiplier;
                this.startRotationZ = mainModule.startRotationZ;
                this.startRotationZMultiplier = mainModule.startRotationZMultiplier;

                this.randomizeRotationDirection = mainModule.randomizeRotationDirection;

                this.startColor = mainModule.startColor;

                this.gravityModifier = mainModule.gravityModifier;
                this.gravityModifierMultiplier = mainModule.gravityModifierMultiplier;

                this.simulationSpace = mainModule.simulationSpace;
                this.simulationSpeed = mainModule.simulationSpeed;
                this.scalingMode = mainModule.scalingMode;

                this.maxParticles = mainModule.maxParticles;
                this.playOnAwake = mainModule.playOnAwake;
            }

            public void Initialize()
            {
                duration = 5;
                loop = true;
                prewarm = false;
                maxParticles = 1000;
                playOnAwake = true;

                startDelay = 0;
                startDelayMultiplier = 0;

                startColor = Color.white;
                startLifetime = 5;
                startLifetimeMultiplier = 5;

                startSpeed = 5;
                startSpeedMultiplier = 5;

                startSize = 1;
                startSizeMultiplier = 1;

                startSize3D = false;
                startSizeX = 1;
                startSizeXMultiplier = 1;
                startSizeY = 1;
                startSizeYMultiplier = 1;
                startSizeZ = 1;
                startSizeZMultiplier = 1;

                startRotation = 0;
                startRotationMultiplier = 0;

                startRotation3D = false;
                startRotationX = 0;
                startRotationXMultiplier = 0;
                startRotationY = 0;
                startRotationYMultiplier = 0;
                startRotationZ = 0;
                startRotationZMultiplier = 0;

                randomizeRotationDirection = 0;

                gravityModifier = 0;
                gravityModifierMultiplier = 0;

                simulationSpace = ParticleSystemSimulationSpace.Local;
                simulationSpeed = 1;
                scalingMode = ParticleSystemScalingMode.Local;
            }

            public static implicit operator MainModule(ParticleSystem.MainModule mainModule)
            {
                return new MainModule(mainModule);
            }

            public static implicit operator ParticleSystem.MainModule(MainModule mainModule)
            {
                return new ParticleSystem.MainModule()
                {
                    duration = mainModule.duration,
                    loop = mainModule.loop,
                    prewarm = mainModule.prewarm,

                    customSimulationSpace = mainModule.customSimulationSpace,

                    startDelay = mainModule.startDelay,
                    startDelayMultiplier = mainModule.startDelayMultiplier,
                    startLifetime = mainModule.startLifetime,
                    startLifetimeMultiplier = mainModule.startLifetimeMultiplier,
                    startSpeed = mainModule.startSpeed,
                    startSpeedMultiplier = mainModule.startSpeedMultiplier,

                    startSize3D = mainModule.startSize3D,
                    startSize = mainModule.startSize,
                    startSizeMultiplier = mainModule.startSizeMultiplier,

                    startSizeX = mainModule.startSizeX,
                    startSizeXMultiplier = mainModule.startSizeXMultiplier,
                    startSizeY = mainModule.startSizeY,
                    startSizeYMultiplier = mainModule.startSizeYMultiplier,
                    startSizeZ = mainModule.startSizeZ,
                    startSizeZMultiplier = mainModule.startSizeZMultiplier,

                    startRotation3D = mainModule.startRotation3D,
                    startRotation = mainModule.startRotation,
                    startRotationMultiplier = mainModule.startRotationMultiplier,

                    startRotationX = mainModule.startRotationX,
                    startRotationXMultiplier = mainModule.startRotationXMultiplier,
                    startRotationY = mainModule.startRotationY,
                    startRotationYMultiplier = mainModule.startRotationYMultiplier,
                    startRotationZ = mainModule.startRotationZ,
                    startRotationZMultiplier = mainModule.startRotationZMultiplier,

                    randomizeRotationDirection = mainModule.randomizeRotationDirection,

                    startColor = mainModule.startColor,

                    gravityModifier = mainModule.gravityModifier,
                    gravityModifierMultiplier = mainModule.gravityModifierMultiplier,

                    simulationSpace = mainModule.simulationSpace,
                    simulationSpeed = mainModule.simulationSpeed,
                    scalingMode = mainModule.scalingMode,

                    maxParticles = mainModule.maxParticles,
                    playOnAwake = mainModule.playOnAwake
                };
            }

            public void CopyTo(ParticleSystem.MainModule mainModule)
            {
                mainModule.duration = duration;
                mainModule.loop = loop;
                mainModule.prewarm = prewarm;

                mainModule.customSimulationSpace = customSimulationSpace;

                mainModule.startDelay = startDelay;
                mainModule.startDelayMultiplier = startDelayMultiplier;
                mainModule.startLifetime = startLifetime;
                mainModule.startLifetimeMultiplier = startLifetimeMultiplier;
                mainModule.startSpeed = startSpeed;
                mainModule.startSpeedMultiplier = startSpeedMultiplier;

                mainModule.startSize3D = startSize3D;
                mainModule.startSize = startSize;
                mainModule.startSizeMultiplier = startSizeMultiplier;

                mainModule.startSizeX = startSizeX;
                mainModule.startSizeXMultiplier = startSizeXMultiplier;
                mainModule.startSizeY = startSizeY;
                mainModule.startSizeYMultiplier = startSizeYMultiplier;
                mainModule.startSizeZ = startSizeZ;
                mainModule.startSizeZMultiplier = startSizeZMultiplier;

                mainModule.startRotation3D = startRotation3D;
                mainModule.startRotation = startRotation;
                mainModule.startRotationMultiplier = startRotationMultiplier;

                mainModule.startRotationX = startRotationX;
                mainModule.startRotationXMultiplier = startRotationXMultiplier;
                mainModule.startRotationY = startRotationY;
                mainModule.startRotationYMultiplier = startRotationYMultiplier;
                mainModule.startRotationZ = startRotationZ;
                mainModule.startRotationZMultiplier = startRotationZMultiplier;

                mainModule.randomizeRotationDirection = randomizeRotationDirection;

                mainModule.startColor = startColor;

                mainModule.gravityModifier = gravityModifier;
                mainModule.gravityModifierMultiplier = gravityModifierMultiplier;

                mainModule.simulationSpace = simulationSpace;
                mainModule.simulationSpeed = simulationSpeed;
                mainModule.scalingMode = scalingMode;

                mainModule.maxParticles = maxParticles;
                mainModule.playOnAwake = playOnAwake;
            }


        }

        /// <summary>
        /// Script interface for the Collision module.
        /// </summary>
        [System.Serializable]
        public class CollisionModule
        {

            /// <summary>
            /// How much force is applied to each particle after a collision.
            /// </summary>
            public MinMaxCurve bounce;

            /// <summary>
            /// Change the bounce multiplier.
            /// </summary>
            public float bounceMultiplier;

            /// <summary>
            /// Control which layers this particle system collides with.
            /// </summary>
            public LayerMask collidesWith;

            /// <summary>
            /// How much speed is lost from each particle after a collision.
            /// </summary>
            public MinMaxCurve dampen;

            /// <summary>
            /// Change the dampen multiplier.
            /// </summary>
            public float dampenMultiplier;

            /// <summary>
            /// Enable/disable the Collision module.
            /// </summary>
            public bool enabled;

            /// <summary>
            /// Allow particles to collide with dynamic colliders when using world collision mode.
            /// </summary>
            public bool enableDynamicColliders;

            /// <summary>
            /// Allow particles to collide when inside colliders.
            /// </summary>
            public bool enableInteriorCollisions;

            /// <summary>
            /// How much a particle's lifetime is reduced after a collision.
            /// </summary>
            public MinMaxCurve lifetimeLoss;

            /// <summary>
            /// Change the lifetime loss multiplier.
            /// </summary>
            public float lifetimeLossMultiplier;

            /// <summary>
            /// The maximum number of collision shapes that will be considered for particle collisions. Excess shapes will be ignored. Terrains take priority.
            /// </summary>
            public int maxCollisionShapes;

            /// <summary>
            /// Kill particles whose speed goes above this threshold, after a collision.
            /// </summary>
            public float maxKillSpeed;

            /// <summary>
            /// Kill particles whose speed falls below this threshold, after a collision.
            /// </summary>
            public float minKillSpeed;

            /// <summary>
            /// Choose between 2D and 3D world collisions.
            /// </summary>
            public ParticleSystemCollisionMode mode = ParticleSystemCollisionMode.Collision3D;

            /// <summary>
            /// Specifies the accuracy of particle collisions against colliders in the scene.
            /// </summary>
            public ParticleSystemCollisionQuality quality = ParticleSystemCollisionQuality.High;

            /// <summary>
            /// A multiplier applied to the size of each particle before collisions are processed.
            /// </summary>
            public float radiusScale;


            /// <summary>
            /// Send collision callback messages.
            /// </summary>
            public bool sendCollisionMessages;

            /// <summary>
            /// The type of particle collision to perform.
            /// </summary>
            public ParticleSystemCollisionType type = ParticleSystemCollisionType.World;

            /// <summary>
            /// Size of voxels in the collision cache.
            /// </summary>
            public float voxelSize;

            public CollisionModule()
            {
                Initialize();
            }

            public CollisionModule(ParticleSystem.CollisionModule collisionModule)
            {
                this.bounce = collisionModule.bounce;
                this.bounceMultiplier = collisionModule.bounceMultiplier;
                this.collidesWith = collisionModule.collidesWith;
                this.dampen = collisionModule.dampen;
                this.dampenMultiplier = collisionModule.dampenMultiplier;
                this.enabled = collisionModule.enabled;
                this.enableDynamicColliders = collisionModule.enableDynamicColliders;
                this.enableInteriorCollisions = collisionModule.enableInteriorCollisions;
                this.lifetimeLoss = collisionModule.lifetimeLoss;
                this.lifetimeLossMultiplier = collisionModule.lifetimeLossMultiplier;
                this.maxCollisionShapes = collisionModule.maxCollisionShapes;
                this.maxKillSpeed = collisionModule.maxKillSpeed;
                this.minKillSpeed = collisionModule.minKillSpeed;
                this.mode = collisionModule.mode;
                this.quality = collisionModule.quality;
                this.radiusScale = collisionModule.radiusScale;
                this.sendCollisionMessages = collisionModule.sendCollisionMessages;
                this.type = collisionModule.type;
                this.voxelSize = collisionModule.voxelSize;
            }

            public void Initialize()
            {
                bounce = 1;
                bounceMultiplier = 1;
                collidesWith = -1;

                dampen = 0;
                dampenMultiplier = 0;

                enableDynamicColliders = true;
                enableInteriorCollisions = true;

                lifetimeLoss = 0;
                lifetimeLossMultiplier = 0;

                maxCollisionShapes = 256;
                maxKillSpeed = 10000;
                minKillSpeed = 0;

                mode = ParticleSystemCollisionMode.Collision3D;
                quality = ParticleSystemCollisionQuality.High;
                radiusScale = 1;
                sendCollisionMessages = false;

                type = ParticleSystemCollisionType.Planes;
                voxelSize = 0.5f;
            }

            /// <summary>
            /// Get a collision plane associated with this particle system.
            /// </summary>
            /// <param name="index">Specifies which plane to access.</param>
            /// <returns>The plane.</returns>
            public Transform GetPlane(int index)
            {
                return null;
            }

            /// <summary>
            /// Set a collision plane to be used with this particle system.
            /// </summary>
            /// <param name="index">Specifies which plane to set.</param>
            /// <param name="transform">The plane to set.</param>
            public void SetPlane(int index, Transform transform)
            { }

            public static implicit operator CollisionModule(ParticleSystem.CollisionModule collisionModule)
            {
                return new CollisionModule(collisionModule);
            }

            public static implicit operator ParticleSystem.CollisionModule(CollisionModule collisionModule)
            {
                return new ParticleSystem.CollisionModule()
                {
                    bounce = collisionModule.bounce,
                    bounceMultiplier = collisionModule.bounceMultiplier,
                    collidesWith = collisionModule.collidesWith,
                    dampen = collisionModule.dampen,
                    dampenMultiplier = collisionModule.dampenMultiplier,
                    enabled = collisionModule.enabled,
                    enableDynamicColliders = collisionModule.enableDynamicColliders,
                    enableInteriorCollisions = collisionModule.enableInteriorCollisions,
                    lifetimeLoss = collisionModule.lifetimeLoss,
                    lifetimeLossMultiplier = collisionModule.lifetimeLossMultiplier,
                    maxCollisionShapes = collisionModule.maxCollisionShapes,
                    maxKillSpeed = collisionModule.maxKillSpeed,
                    minKillSpeed = collisionModule.minKillSpeed,
                    mode = collisionModule.mode,
                    quality = collisionModule.quality,
                    radiusScale = collisionModule.radiusScale,
                    sendCollisionMessages = collisionModule.sendCollisionMessages,
                    type = collisionModule.type,
                    voxelSize = collisionModule.voxelSize
                };
            }

            public void CopyTo(ParticleSystem.CollisionModule collisionModule)
            {
                collisionModule.bounce = bounce;
                collisionModule.bounceMultiplier = bounceMultiplier;
                collisionModule.collidesWith = collidesWith;
                collisionModule.dampen = dampen;
                collisionModule.dampenMultiplier = dampenMultiplier;
                collisionModule.enabled = enabled;
                collisionModule.enableDynamicColliders = enableDynamicColliders;
                collisionModule.enableInteriorCollisions = enableInteriorCollisions;
                collisionModule.lifetimeLoss = lifetimeLoss;
                collisionModule.lifetimeLossMultiplier = lifetimeLossMultiplier;
                collisionModule.maxCollisionShapes = maxCollisionShapes;
                collisionModule.maxKillSpeed = maxKillSpeed;
                collisionModule.minKillSpeed = minKillSpeed;
                collisionModule.mode = mode;
                collisionModule.quality = quality;
                collisionModule.radiusScale = radiusScale;
                collisionModule.sendCollisionMessages = sendCollisionMessages;
                collisionModule.type = type;
                collisionModule.voxelSize = voxelSize;
            }
        }

        [System.Serializable]
        public class ColorBySpeedModule
        {
            /// <summary>
            /// The gradient controlling the particle colors.
            /// </summary>
            public MinMaxGradient color;

            /// <summary>
            /// Enable/disable the Color By Speed module.
            /// </summary>
            public bool enabled;

            /// <summary>
            /// Apply the color gradient between these minimum and maximum speeds.
            /// </summary>
            public Vector2 range;

            public ColorBySpeedModule()
            {
                Initialize();
            }

            public ColorBySpeedModule(ParticleSystem.ColorBySpeedModule colorBySpeedModule)
            {
                this.color = colorBySpeedModule.color;
                this.enabled = colorBySpeedModule.enabled;
                this.range = colorBySpeedModule.range;
            }

            public void Initialize()
            {
                var gradient = new Gradient();
                gradient.Set(Color.white);

                color = gradient;
                range = new Vector2(0, 1);
            }

            public static implicit operator ColorBySpeedModule(ParticleSystem.ColorBySpeedModule colorBySpeedModule)
            {
                return new ColorBySpeedModule(colorBySpeedModule);
            }

            public static implicit operator ParticleSystem.ColorBySpeedModule(ColorBySpeedModule colorBySpeedModule)
            {
                return new ParticleSystem.ColorBySpeedModule()
                {
                    color = colorBySpeedModule.color,
                    enabled = colorBySpeedModule.enabled,
                    range = colorBySpeedModule.range
                };
            }

            public void CopyTo(ParticleSystem.ColorBySpeedModule colorBySpeedModule)
            {
                colorBySpeedModule.color = color;
                colorBySpeedModule.enabled = enabled;
                colorBySpeedModule.range = range;
            }

        }

        [System.Serializable]
        public class ColorOverLifetimeModule
        {
            /// <summary>
            /// The gradient controlling the particle colors.
            /// </summary>
            public MinMaxGradient color;

            /// <summary>
            /// Enable/disable the Color Over Lifetime module.
            /// </summary>
            public bool enabled;

            public ColorOverLifetimeModule()
            {
                Initialize();
            }

            public ColorOverLifetimeModule(ParticleSystem.ColorOverLifetimeModule colorOverLifetimeModule)
            {
                this.color = colorOverLifetimeModule.color;
                this.enabled = colorOverLifetimeModule.enabled;
            }

            public void Initialize()
            {
                var gradient = new Gradient();
                gradient.Set(Color.white);

                color = gradient;
            }

            public static implicit operator ColorOverLifetimeModule(ParticleSystem.ColorOverLifetimeModule colorOverLifetimeModule)
            {
                return new ColorOverLifetimeModule(colorOverLifetimeModule);
            }

            public static implicit operator ParticleSystem.ColorOverLifetimeModule(ColorOverLifetimeModule colorOverLifetimeModule)
            {
                return new ParticleSystem.ColorOverLifetimeModule()
                {
                    color = colorOverLifetimeModule.color,
                    enabled = colorOverLifetimeModule.enabled
                };
            }

            public void CopyTo(ParticleSystem.ColorOverLifetimeModule colorOverLifetimeModule)
            {
                colorOverLifetimeModule.color = color;
                colorOverLifetimeModule.enabled = enabled;
            }
        }

        [System.Serializable]
        public class EmissionModule
        {
            /// <summary>
            /// Enable/disable the Emission module.
            /// </summary>
            public bool enabled;

            /// <summary>
            /// The rate at which new particles are spawned, over distance.
            /// </summary>
            public MinMaxCurve rateOverDistance;

            /// <summary>
            /// Change the rate over distance multiplier.
            /// </summary>
            public float rateOverDistanceMultiplier;

            /// <summary>
            /// The rate at which new particles are spawned, over time.
            /// </summary>
            public MinMaxCurve rateOverTime;

            /// <summary>
            /// Change the rate over time multiplier.
            /// </summary>
            public float rateOverTimeMultiplier;

            public List<Burst> bursts;

            public EmissionModule()
            {
                Initialize();
            }

            public EmissionModule(ParticleSystem.EmissionModule emissionModule)
            {
                this.enabled = emissionModule.enabled;
                this.rateOverDistance = emissionModule.rateOverDistance;
                this.rateOverDistanceMultiplier = emissionModule.rateOverDistanceMultiplier;
                this.rateOverTime = emissionModule.rateOverTime;
                this.rateOverTimeMultiplier = emissionModule.rateOverTimeMultiplier;

                var particleSystemBursts = new ParticleSystem.Burst[emissionModule.burstCount];
                emissionModule.GetBursts(particleSystemBursts);
                bursts = new List<Burst>();

                foreach (var burst in particleSystemBursts)
                {
                    bursts.Add(burst);
                }
            }

            public void Initialize()
            {
                enabled = true;
                rateOverDistance = 0;
                rateOverDistanceMultiplier = 0;
                rateOverTime = 10;
                rateOverTimeMultiplier = 10;

                bursts = new List<Burst>();
            }

            public static implicit operator EmissionModule(ParticleSystem.EmissionModule emissionModule)
            {
                return new EmissionModule(emissionModule);
            }

            public static implicit operator ParticleSystem.EmissionModule(EmissionModule emissionModule)
            {
                return new ParticleSystem.EmissionModule()
                {
                    enabled = emissionModule.enabled,
                    rateOverDistance = emissionModule.rateOverDistance,
                    rateOverDistanceMultiplier = emissionModule.rateOverDistanceMultiplier,
                    rateOverTime = emissionModule.rateOverTime,
                    rateOverTimeMultiplier = emissionModule.rateOverTimeMultiplier,
                };
            }

            public void CopyTo(ParticleSystem.EmissionModule emissionModule)
            {
                emissionModule.enabled = enabled;
                emissionModule.rateOverDistance = rateOverDistance;
                emissionModule.rateOverDistanceMultiplier = rateOverDistanceMultiplier;
                emissionModule.rateOverTime = rateOverTime;
                emissionModule.rateOverTimeMultiplier = rateOverTimeMultiplier;

                var particleSystemBursts = new List<ParticleSystem.Burst>();
                bursts.ForEach(b => particleSystemBursts.Add(b));
                emissionModule.SetBursts(particleSystemBursts.ToArray());

            }
        }

        [System.Serializable]
        public class ExternalForcesModule
        {
            /// <summary>
            /// Enable/disable the External Forces module.
            /// </summary>
            public bool enabled;

            /// <summary>
            /// Multiplies the magnitude of applied external forces.
            /// </summary>
            public float multiplier = 1;

            public ExternalForcesModule()
            {
                Initialize();
            }

            public ExternalForcesModule(ParticleSystem.ExternalForcesModule externalForcesModule)
            {
                this.enabled = externalForcesModule.enabled;
                this.multiplier = externalForcesModule.multiplier;
            }

            public void Initialize()
            {
                multiplier = 1;
            }

            public static implicit operator ExternalForcesModule(ParticleSystem.ExternalForcesModule externalForcesModule)
            {
                return new ExternalForcesModule(externalForcesModule);
            }

            public static implicit operator ParticleSystem.ExternalForcesModule(ExternalForcesModule externalForcesModule)
            {
                return new ParticleSystem.ExternalForcesModule()
                {
                    enabled = externalForcesModule.enabled,
                    multiplier = externalForcesModule.multiplier
                };
            }

            public void CopyTo(ParticleSystem.ExternalForcesModule externalForcesModule)
            {
                externalForcesModule.enabled = enabled;
                externalForcesModule.multiplier = multiplier;
            }
        }

        [System.Serializable]
        public class ForceOverLifetimeModule
        {
            /// <summary>
            /// Enable/disable the Force Over Lifetime module.
            /// </summary>
            public bool enabled;

            /// <summary>
            /// When randomly selecting values between two curves or constants, this flag will cause a new random force to be chosen on each frame.
            /// </summary>
            public bool randomized;

            /// <summary>
            /// Are the forces being applied in local or world space?
            /// </summary>
            public ParticleSystemSimulationSpace space;

            /// <summary>
            /// The curve defining particle forces in the X axis.
            /// </summary>
            public MinMaxCurve x;

            /// <summary>
            /// Change the X axis mulutiplier.
            /// </summary>
            public float xMultiplier;

            /// <summary>
            /// The curve defining particle forces in the Y axis.
            /// </summary>
            public MinMaxCurve y;

            /// <summary>
            /// Change the Y axis multiplier.
            /// </summary>
            public float yMultiplier;

            /// <summary>
            /// The curve defining particle forces in the Z axis.
            /// </summary>
            public MinMaxCurve z;

            /// <summary>
            /// Change the Z axis multiplier.
            /// </summary>
            public float zMultiplier;

            public ForceOverLifetimeModule()
            {
                Initialize();
            }

            public ForceOverLifetimeModule(ParticleSystem.ForceOverLifetimeModule forceOverLifetimeModule)
            {
                this.enabled = forceOverLifetimeModule.enabled;
                this.randomized = forceOverLifetimeModule.randomized;
                this.space = forceOverLifetimeModule.space;
                this.x = forceOverLifetimeModule.x;
                this.xMultiplier = forceOverLifetimeModule.xMultiplier;
                this.y = forceOverLifetimeModule.y;
                this.yMultiplier = forceOverLifetimeModule.yMultiplier;
                this.z = forceOverLifetimeModule.z;
                this.zMultiplier = forceOverLifetimeModule.zMultiplier;
            }

            public void Initialize()
            {
                randomized = false;
                space = ParticleSystemSimulationSpace.Local;
                x = 0;
                xMultiplier = 0;
                y = 0;
                yMultiplier = 0;
                z = 0;
                zMultiplier = 0;
            }

            public static implicit operator ForceOverLifetimeModule(ParticleSystem.ForceOverLifetimeModule forceOverLifetimeModule)
            {
                return new ForceOverLifetimeModule(forceOverLifetimeModule);
            }

            public static implicit operator ParticleSystem.ForceOverLifetimeModule(ForceOverLifetimeModule forceOverLifetimeModule)
            {
                return new ParticleSystem.ForceOverLifetimeModule()
                {
                    enabled = forceOverLifetimeModule.enabled,
                    randomized = forceOverLifetimeModule.randomized,
                    space = forceOverLifetimeModule.space,
                    x = forceOverLifetimeModule.x,
                    xMultiplier = forceOverLifetimeModule.xMultiplier,
                    y = forceOverLifetimeModule.y,
                    yMultiplier = forceOverLifetimeModule.yMultiplier,
                    z = forceOverLifetimeModule.z,
                    zMultiplier = forceOverLifetimeModule.zMultiplier
                };
            }

            public void CopyTo(ParticleSystem.ForceOverLifetimeModule forceOverLifetimeModule)
            {
                forceOverLifetimeModule.enabled = enabled;
                forceOverLifetimeModule.randomized = randomized;
                forceOverLifetimeModule.space = space;
                forceOverLifetimeModule.x = x;
                forceOverLifetimeModule.xMultiplier = xMultiplier;
                forceOverLifetimeModule.y = y;
                forceOverLifetimeModule.yMultiplier = yMultiplier;
                forceOverLifetimeModule.z = z;
                forceOverLifetimeModule.zMultiplier = zMultiplier;
            }
        }

        /// <summary>
        /// The Inherit Velocity Module controls how the velocity of the emitter is transferred to the particles as they are emitted.
        /// </summary>
        [System.Serializable]
        public class InheritVelocityModule
        {
            /// <summary>
            /// Curve to define how much emitter velocity is applied during the lifetime of a particle.
            /// </summary>
            public MinMaxCurve curve;

            /// <summary>
            /// Change the curve multiplier.
            /// </summary>
            public float curveMultiplier;

            /// <summary>
            /// Enable/disable the InheritVelocity module.
            /// </summary>
            public bool enabled;

            /// <summary>
            /// How to apply emitter velocity to particles.
            /// </summary>
            public ParticleSystemInheritVelocityMode mode;

            public InheritVelocityModule()
            {
                Initialize();
            }

            public InheritVelocityModule(ParticleSystem.InheritVelocityModule inheritVelocityModule)
            {
                this.curve = inheritVelocityModule.curve;
                this.curveMultiplier = inheritVelocityModule.curveMultiplier;
                this.enabled = inheritVelocityModule.enabled;
                this.mode = inheritVelocityModule.mode;
            }

            public void Initialize()
            {
                curve = 0;
                curveMultiplier = 0;
                mode = ParticleSystemInheritVelocityMode.Initial;
            }

            public static implicit operator InheritVelocityModule(ParticleSystem.InheritVelocityModule inheritVelocityModule)
            {
                return new InheritVelocityModule(inheritVelocityModule);
            }

            public static implicit operator ParticleSystem.InheritVelocityModule(InheritVelocityModule inheritVelocityModule)
            {
                return new ParticleSystem.InheritVelocityModule()
                {
                    curve = inheritVelocityModule.curve,
                    curveMultiplier = inheritVelocityModule.curveMultiplier,
                    enabled = inheritVelocityModule.enabled,
                    mode = inheritVelocityModule.mode
                };
            }

            public void CopyTo(ParticleSystem.InheritVelocityModule inheritVelocityModule)
            {
                inheritVelocityModule.curve = curve;
                inheritVelocityModule.curveMultiplier = curveMultiplier;
                inheritVelocityModule.enabled = enabled;
                inheritVelocityModule.mode = mode;
            }
        }

        [System.Serializable]
        public class LightsModule
        {

            /// <summary>
            /// Toggle whether the particle alpha gets multiplied by the light intensity, when computing the final light intensity.
            /// </summary>
            public bool alphaAffectsIntensity;

            /// <summary>
            /// Enable/disable the Lights module.
            /// </summary>
            public bool enabled;

            /// <summary>
            /// Define a curve to apply custom intensity scaling to particle lights.
            /// </summary>
            public MinMaxCurve intensity;

            /// <summary>
            /// Intensity multiplier.
            /// </summary>
            public float intensityMultiplier;

            //TODO: this needs to be implemented as a Light Widget.
            /// <summary>
            /// Select what Light prefab you want to base your particle lights on.
            /// </summary>
            public Light light;

            /// <summary>
            /// Set a limit on how many lights this Module can create.
            /// </summary>
            public int maxLights;

            /// <summary>
            /// Define a curve to apply custom range scaling to particle lights.
            /// </summary>
            public MinMaxCurve range;

            /// <summary>
            /// Range multiplier.
            /// </summary>
            public float rangeMultiplier;

            /// <summary>
            /// Choose what proportion of particles will receive a dynamic light.
            /// </summary>
            public float ratio;

            /// <summary>
            /// Toggle where the particle size will be multiplied by the light range, to determine the final light range.
            /// </summary>
            public bool sizeAffectsRange;

            /// <summary>
            /// Toggle whether the particle lights will have their color multiplied by the particle color.
            /// </summary>
            public bool useParticleColor;

            /// <summary>
            /// Randomly assign lights to new particles based on ParticleSystem.LightsModule.ratio.
            /// </summary>
            public bool useRandomDistribution;

            public LightsModule()
            {
                Initialize();
            }

            public LightsModule(ParticleSystem.LightsModule lightsModule)
            {
                this.alphaAffectsIntensity = lightsModule.alphaAffectsIntensity;
                this.enabled = lightsModule.enabled;
                this.intensity = lightsModule.intensity;
                this.intensityMultiplier = lightsModule.intensityMultiplier;
                this.light = lightsModule.light;
                this.maxLights = lightsModule.maxLights;
                this.range = lightsModule.range;
                this.rangeMultiplier = lightsModule.rangeMultiplier;
                this.ratio = lightsModule.ratio;
                this.sizeAffectsRange = lightsModule.sizeAffectsRange;
                this.useParticleColor = lightsModule.useParticleColor;
                this.useRandomDistribution = lightsModule.useRandomDistribution;
            }

            public void Initialize()
            {
                alphaAffectsIntensity = true;
                intensity = 1;
                intensityMultiplier = 1;
                maxLights = 20;
                range = 1;
                rangeMultiplier = 1;
                ratio = 0;
                sizeAffectsRange = true;
                useParticleColor = true;
                useRandomDistribution = true;
            }

            public static implicit operator LightsModule(ParticleSystem.LightsModule lightsModule)
            {
                return new LightsModule(lightsModule);
            }

            public static implicit operator ParticleSystem.LightsModule(LightsModule lightsModule)
            {
                return new ParticleSystem.LightsModule()
                {
                    alphaAffectsIntensity = lightsModule.alphaAffectsIntensity,
                    enabled = lightsModule.enabled,
                    intensity = lightsModule.intensity,
                    intensityMultiplier = lightsModule.intensityMultiplier,
                    light = lightsModule.light,
                    maxLights = lightsModule.maxLights,
                    range = lightsModule.range,
                    rangeMultiplier = lightsModule.rangeMultiplier,
                    ratio = lightsModule.ratio,
                    sizeAffectsRange = lightsModule.sizeAffectsRange,
                    useParticleColor = lightsModule.useParticleColor,
                    useRandomDistribution = lightsModule.useRandomDistribution
                };
            }

            public void CopyTo(ParticleSystem.LightsModule lightsModule)
            {
                lightsModule.alphaAffectsIntensity = alphaAffectsIntensity;
                lightsModule.enabled = enabled;
                lightsModule.intensity = intensity;
                lightsModule.intensityMultiplier = intensityMultiplier;
                lightsModule.light = light;
                lightsModule.maxLights = maxLights;
                lightsModule.range = range;
                lightsModule.rangeMultiplier = rangeMultiplier;
                lightsModule.ratio = ratio;
                lightsModule.sizeAffectsRange = sizeAffectsRange;
                lightsModule.useParticleColor = useParticleColor;
                lightsModule.useRandomDistribution = useRandomDistribution;
            }
        }

        [System.Serializable]
        public class LimitVelocityOverLifetimeModule
        {
            /// <summary>
            /// Controls how much the velocity that exceeds the velocity limit should be dampened.
            /// </summary>
            public float dampen;

            /// <summary>
            /// Enable/disable the Limit Force Over Lifetime module.
            /// </summary>
            public bool enabled;

            /// <summary>
            /// Maximum velocity curve, when not using one curve per axis.
            /// </summary>
            public MinMaxCurve limit;

            /// <summary>
            /// Change the limit multiplier.
            /// </summary>
            public float limitMultiplier;

            /// <summary>
            /// Maximum velocity curve for the X axis.
            /// </summary>
            public MinMaxCurve limitX;

            /// <summary>
            /// Change the limit multiplier on the X axis.
            /// </summary>
            public float limitXMultiplier;

            /// <summary>
            /// Maximum velocity curve for the Y axis.
            /// </summary>
            public MinMaxCurve limitY;

            /// <summary>
            /// Change the limit multiplier on the Y axis.
            /// </summary>
            public float limitYMultiplier;

            /// <summary>
            /// Maximum velocity curve for the Z axis.
            /// </summary>
            public MinMaxCurve limitZ;

            /// <summary>
            /// Change the limit multiplier on the Z axis.
            /// </summary>
            public float limitZMultiplier;

            /// <summary>
            /// Set the velocity limit on each axis separately.
            /// </summary>
            public bool separateAxes;

            /// <summary>
            /// Specifies if the velocity limits are in local space (rotated with the transform) or world space.
            /// </summary>
            public ParticleSystemSimulationSpace space;

            public LimitVelocityOverLifetimeModule()
            {
                Initialize();
            }

            public LimitVelocityOverLifetimeModule(ParticleSystem.LimitVelocityOverLifetimeModule limitVelocityOverLifetimeModule)
            {
                this.dampen = limitVelocityOverLifetimeModule.dampen;
                this.enabled = limitVelocityOverLifetimeModule.enabled;
                this.limit = limitVelocityOverLifetimeModule.limit;
                this.limitMultiplier = limitVelocityOverLifetimeModule.limitMultiplier;
                this.limitX = limitVelocityOverLifetimeModule.limitX;
                this.limitXMultiplier = limitVelocityOverLifetimeModule.limitXMultiplier;
                this.limitY = limitVelocityOverLifetimeModule.limitY;
                this.limitYMultiplier = limitVelocityOverLifetimeModule.limitYMultiplier;
                this.limitZ = limitVelocityOverLifetimeModule.limitZ;
                this.limitZMultiplier = limitVelocityOverLifetimeModule.limitZMultiplier;
                this.separateAxes = limitVelocityOverLifetimeModule.separateAxes;
                this.space = limitVelocityOverLifetimeModule.space;
            }

            public void Initialize()
            {
                dampen = 1;
                limit = 1;
                limitMultiplier = 1;
                limitX = 1;
                separateAxes = false;
                limitXMultiplier = 1;
                limitY = 1;
                limitYMultiplier = 1;
                limitZ = 1;
                limitZMultiplier = 1;
                space = ParticleSystemSimulationSpace.Local;
            }


            public static implicit operator LimitVelocityOverLifetimeModule(ParticleSystem.LimitVelocityOverLifetimeModule limitVelocityOverLifetimeModule)
            {
                return new LimitVelocityOverLifetimeModule(limitVelocityOverLifetimeModule);
            }

            public static implicit operator ParticleSystem.LimitVelocityOverLifetimeModule(LimitVelocityOverLifetimeModule limitVelocityOverLifetimeModule)
            {
                return new ParticleSystem.LimitVelocityOverLifetimeModule()
                {
                    dampen = limitVelocityOverLifetimeModule.dampen,
                    enabled = limitVelocityOverLifetimeModule.enabled,
                    limit = limitVelocityOverLifetimeModule.limit,
                    limitMultiplier = limitVelocityOverLifetimeModule.limitMultiplier,
                    limitX = limitVelocityOverLifetimeModule.limitX,
                    limitXMultiplier = limitVelocityOverLifetimeModule.limitXMultiplier,
                    limitY = limitVelocityOverLifetimeModule.limitY,
                    limitYMultiplier = limitVelocityOverLifetimeModule.limitYMultiplier,
                    limitZ = limitVelocityOverLifetimeModule.limitZ,
                    limitZMultiplier = limitVelocityOverLifetimeModule.limitZMultiplier,
                    separateAxes = limitVelocityOverLifetimeModule.separateAxes,
                    space = limitVelocityOverLifetimeModule.space
                };
            }

            public void CopyTo(ParticleSystem.LimitVelocityOverLifetimeModule limitVelocityOverLifetimeModule)
            {
                limitVelocityOverLifetimeModule.dampen = dampen;
                limitVelocityOverLifetimeModule.enabled = enabled;
                limitVelocityOverLifetimeModule.limit = limit;
                limitVelocityOverLifetimeModule.limitMultiplier = limitMultiplier;
                limitVelocityOverLifetimeModule.limitX = limitX;
                limitVelocityOverLifetimeModule.limitXMultiplier = limitXMultiplier;
                limitVelocityOverLifetimeModule.limitY = limitY;
                limitVelocityOverLifetimeModule.limitYMultiplier = limitYMultiplier;
                limitVelocityOverLifetimeModule.limitZ = limitZ;
                limitVelocityOverLifetimeModule.limitZMultiplier = limitZMultiplier;
                limitVelocityOverLifetimeModule.separateAxes = separateAxes;
                limitVelocityOverLifetimeModule.space = space;
            }
        }

        /// <summary>
        /// Noise Module allows you to apply turbulence to the movement of your particles. Use the low quality settings to create computationally efficient Noise, or simulate smoother, richer Noise with the higher quality settings. You can also choose to define the behavior of the Noise individually for each axis.
        /// </summary>
        [System.Serializable]
        public class NoiseModule
        {
            /// <summary>
            /// Higher frequency noise will reduce the strength by a proportional amount, if enabled.
            /// </summary>
            public bool damping = true;

            /// <summary>
            /// Enable/disable the Noise module.
            /// </summary>
            public bool enabled;

            /// <summary>
            /// Low values create soft, smooth noise, and high values create rapidly changing noise.
            /// </summary>
            public float frequency = 0.5f;

            /// <summary>
            /// Layers of noise that combine to produce final noise.
            /// </summary>
            public int octaveCount = 1;

            /// <summary>
            /// When combining each octave, scale the intensity by this amount.
            /// </summary>
            public float octaveMultiplier = 0.5f;

            /// <summary>
            /// When combining each octave, zoom in by this amount.
            /// </summary>
            public float octaveScale = 2;

            /// <summary>
            /// Generate 1D, 2D or 3D noise.
            /// </summary>
            public ParticleSystemNoiseQuality quality = ParticleSystemNoiseQuality.High;

            /// <summary>
            /// Define how the noise values are remapped.
            /// </summary>
            public MinMaxCurve remap;

            /// <summary>
            /// Enable remapping of the final noise values, allowing for noise values to be translated into different values.
            /// </summary>
            public bool remapEnabled;

            /// <summary>
            /// Remap multiplier.
            /// </summary>
            public float remapMultiplier;

            /// <summary>
            /// Define how the noise values are remapped on the X axis, when using the ParticleSystem.NoiseModule.separateAxes option.
            /// </summary>
            public MinMaxCurve remapX;

            /// <summary>
            /// X axis remap multiplier.
            /// </summary>
            public float remapXMultiplier;

            /// <summary>
            /// Define how the noise values are remapped on the Y axis, when using the ParticleSystem.NoiseModule.separateAxes option.
            /// </summary>
            public MinMaxCurve remapY;

            /// <summary>
            /// Y axis remap multiplier.
            /// </summary>
            public float remapYMultiplier;

            /// <summary>
            /// Define how the noise values are remapped on the Z axis, when using the ParticleSystem.NoiseModule.separateAxes option.
            /// </summary>
            public MinMaxCurve remapZ;

            /// <summary>
            /// Z axis remap multiplier.
            /// </summary>
            public float remapZMultiplier;

            /// <summary>
            /// Scroll the noise map over the particle system.
            /// </summary>
            public MinMaxCurve scrollSpeed;

            /// <summary>
            /// Scroll speed multiplier.
            /// </summary>
            public float scrollSpeedMultiplier;

            /// <summary>
            /// Control the noise separately for each axis.
            /// </summary>
            public bool separateAxes;

            /// <summary>
            /// How strong the overall noise effect is.
            /// </summary>
            public MinMaxCurve strength = 1;

            /// <summary>
            /// Strength multiplier.
            /// </summary>
            public float strengthMultiplier;

            /// <summary>
            /// Define the strength of the effect on the X axis, when using the ParticleSystem.NoiseModule.separateAxes option.
            /// </summary>
            public MinMaxCurve strengthX;

            /// <summary>
            /// X axis strength multiplier.
            /// </summary>
            public float strengthXMultiplier;

            /// <summary>
            /// Define the strength of the effect on the Y axis, when using the ParticleSystem.NoiseModule.separateAxes option.
            /// </summary>
            public MinMaxCurve strengthY;

            /// <summary>
            /// Y axis strength multiplier.
            /// </summary>
            public float strengthYMultiplier;

            /// <summary>
            /// Define the strength of the effect on the Z axis, when using the ParticleSystem.NoiseModule.separateAxes option.
            /// </summary>
            public MinMaxCurve strengthZ;

            /// <summary>
            /// Z axis strength multiplier.
            /// </summary>
            public float strengthZMultiplier;

            public NoiseModule()
            {
                Initialize();
            }

            public NoiseModule(ParticleSystem.NoiseModule noiseModule)
            {
                this.damping = noiseModule.damping;
                this.enabled = noiseModule.enabled;
                this.frequency = noiseModule.frequency;
                this.octaveCount = noiseModule.octaveCount;
                this.octaveMultiplier = noiseModule.octaveMultiplier;
                this.octaveScale = noiseModule.octaveScale;
                this.quality = noiseModule.quality;
                this.remap = noiseModule.remap;
                this.remapEnabled = noiseModule.remapEnabled;
                this.remapMultiplier = noiseModule.remapMultiplier;
                this.remapX = noiseModule.remapX;
                this.remapXMultiplier = noiseModule.remapXMultiplier;
                this.remapY = noiseModule.remapY;
                this.remapYMultiplier = noiseModule.remapYMultiplier;
                this.remapZ = noiseModule.remapZ;
                this.remapZMultiplier = noiseModule.remapZMultiplier;
                this.scrollSpeed = noiseModule.scrollSpeed;
                this.scrollSpeedMultiplier = noiseModule.scrollSpeedMultiplier;
                this.separateAxes = noiseModule.separateAxes;
                this.strength = noiseModule.strength;
                this.strengthMultiplier = noiseModule.strengthMultiplier;
                this.strengthX = noiseModule.strengthX;
                this.strengthXMultiplier = noiseModule.strengthXMultiplier;
                this.strengthY = noiseModule.strengthY;
                this.strengthYMultiplier = noiseModule.strengthYMultiplier;
                this.strengthZ = noiseModule.strengthZ;
                this.strengthZMultiplier = noiseModule.strengthZMultiplier;
            }

            public void Initialize()
            {
                damping = true;
                frequency = 0.5f;
                octaveCount = 1;
                octaveMultiplier = 0.5f;
                octaveScale = 2;
                quality = ParticleSystemNoiseQuality.High;

                remapEnabled = false;
                remap = new Core.MinMaxCurve(1, AnimationCurve.Linear(0, -1, 1, 1));
                remapMultiplier = 1;
                remapX = new Core.MinMaxCurve(1, AnimationCurve.Linear(0, -1, 1, 1));
                remapXMultiplier = 1;
                remapY = new Core.MinMaxCurve(1, AnimationCurve.Linear(0, -1, 1, 1));
                remapYMultiplier = 1;
                remapZ = new Core.MinMaxCurve(1, AnimationCurve.Linear(0, -1, 1, 1));
                remapZMultiplier = 1;

                scrollSpeed = 0;

                separateAxes = false;
                strength = 1;
                strengthMultiplier = 1;
                strengthX = 1;
                strengthXMultiplier = 1;
                strengthY = 1;
                strengthYMultiplier = 1;
                strengthZ = 1;
                strengthZMultiplier = 1;
            }

            public static implicit operator NoiseModule(ParticleSystem.NoiseModule noiseModule)
            {
                return new NoiseModule(noiseModule);
            }

            public static implicit operator ParticleSystem.NoiseModule(NoiseModule noiseModule)
            {
                return new ParticleSystem.NoiseModule()
                {
                    damping = noiseModule.damping,
                    enabled = noiseModule.enabled,
                    frequency = noiseModule.frequency,
                    octaveCount = noiseModule.octaveCount,
                    octaveMultiplier = noiseModule.octaveMultiplier,
                    octaveScale = noiseModule.octaveScale,
                    quality = noiseModule.quality,
                    remap = noiseModule.remap,
                    remapEnabled = noiseModule.remapEnabled,
                    remapMultiplier = noiseModule.remapMultiplier,
                    remapX = noiseModule.remapX,
                    remapXMultiplier = noiseModule.remapXMultiplier,
                    remapY = noiseModule.remapY,
                    remapYMultiplier = noiseModule.remapYMultiplier,
                    remapZ = noiseModule.remapZ,
                    remapZMultiplier = noiseModule.remapZMultiplier,
                    scrollSpeed = noiseModule.scrollSpeed,
                    scrollSpeedMultiplier = noiseModule.scrollSpeedMultiplier,
                    separateAxes = noiseModule.separateAxes,
                    strength = noiseModule.strength,
                    strengthMultiplier = noiseModule.strengthMultiplier,
                    strengthX = noiseModule.strengthX,
                    strengthXMultiplier = noiseModule.strengthXMultiplier,
                    strengthY = noiseModule.strengthY,
                    strengthYMultiplier = noiseModule.strengthYMultiplier,
                    strengthZ = noiseModule.strengthZ,
                    strengthZMultiplier = noiseModule.strengthZMultiplier
                };
            }

            public void CopyTo(ParticleSystem.NoiseModule noiseModule)
            {
                noiseModule.damping = damping;
                noiseModule.enabled = enabled;
                noiseModule.frequency = frequency;
                noiseModule.octaveCount = octaveCount;
                noiseModule.octaveMultiplier = octaveMultiplier;
                noiseModule.octaveScale = octaveScale;
                noiseModule.quality = quality;
                noiseModule.remap = remap;
                noiseModule.remapEnabled = remapEnabled;
                noiseModule.remapMultiplier = remapMultiplier;
                noiseModule.remapX = remapX;
                noiseModule.remapXMultiplier = remapXMultiplier;
                noiseModule.remapY = remapY;
                noiseModule.remapYMultiplier = remapYMultiplier;
                noiseModule.remapZ = remapZ;
                noiseModule.remapZMultiplier = remapZMultiplier;
                noiseModule.scrollSpeed = scrollSpeed;
                noiseModule.scrollSpeedMultiplier = scrollSpeedMultiplier;
                noiseModule.separateAxes = separateAxes;
                noiseModule.strength = strength;
                noiseModule.strengthMultiplier = strengthMultiplier;
                noiseModule.strengthX = strengthX;
                noiseModule.strengthXMultiplier = strengthXMultiplier;
                noiseModule.strengthY = strengthY;
                noiseModule.strengthYMultiplier = strengthYMultiplier;
                noiseModule.strengthZ = strengthZ;
                noiseModule.strengthZMultiplier = strengthZMultiplier;
            }
        }

        [System.Serializable]
        public class RotationBySpeedModule
        {
            /// <summary>
            /// Enable/disable the Rotation By Speed module.
            /// </summary>
            public bool enabled;

            /// <summary>
            /// Apply the rotation curve between these minimum and maximum speeds.
            /// </summary>
            public Vector2 range = Vector2.one;

            /// <summary>
            /// Set the rotation by speed on each axis separately.
            /// </summary>
            public bool separateAxes;

            /// <summary>
            /// Rotation by speed curve for the X axis.
            /// </summary>
            public MinMaxCurve x;

            /// <summary>
            /// Speed multiplier along the X axis.
            /// </summary>
            public float xMultiplier;

            /// <summary>
            /// Rotation by speed curve for the Y axis.
            /// </summary>
            public MinMaxCurve y;

            /// <summary>
            /// Speed multiplier along the Y axis.
            /// </summary>
            public float yMultiplier;

            /// <summary>
            /// Rotation by speed curve for the Z axis.
            /// </summary>
            public MinMaxCurve z;

            /// <summary>
            /// Speed multiplier along the Z axis.
            /// </summary>
            public float zMultiplier;

            public RotationBySpeedModule()
            {
                Initialize();
            }

            public RotationBySpeedModule(ParticleSystem.RotationBySpeedModule rotationBySpeedModule)
            {
                rotationBySpeedModule.enabled = enabled;
                rotationBySpeedModule.range = range;
                rotationBySpeedModule.separateAxes = separateAxes;
                rotationBySpeedModule.x = x;
                rotationBySpeedModule.xMultiplier = xMultiplier;
                rotationBySpeedModule.y = y;
                rotationBySpeedModule.yMultiplier = yMultiplier;
                rotationBySpeedModule.z = z;
                rotationBySpeedModule.zMultiplier = zMultiplier;
            }

            public void Initialize()
            {
                range = Vector2.one;
                separateAxes = false;
                x = 0;
                xMultiplier = 0;
                y = 0;
                yMultiplier = 0;
                z = 0.7853981f;
                zMultiplier = 0.7853981f;
            }

            public static implicit operator RotationBySpeedModule(ParticleSystem.RotationBySpeedModule rotationBySpeedModule)
            {
                return new RotationBySpeedModule(rotationBySpeedModule);
            }

            public static implicit operator ParticleSystem.RotationBySpeedModule(RotationBySpeedModule rotationBySpeedModule)
            {
                return new ParticleSystem.RotationBySpeedModule()
                {
                    enabled = rotationBySpeedModule.enabled,
                    range = rotationBySpeedModule.range,
                    separateAxes = rotationBySpeedModule.separateAxes,
                    x = rotationBySpeedModule.x,
                    xMultiplier = rotationBySpeedModule.xMultiplier,
                    y = rotationBySpeedModule.y,
                    yMultiplier = rotationBySpeedModule.yMultiplier,
                    z = rotationBySpeedModule.z,
                    zMultiplier = rotationBySpeedModule.zMultiplier
                };
            }

            public void CopyTo(ParticleSystem.RotationBySpeedModule rotationBySpeedModule)
            {
                this.enabled = rotationBySpeedModule.enabled;
                this.range = rotationBySpeedModule.range;
                this.separateAxes = rotationBySpeedModule.separateAxes;
                this.x = rotationBySpeedModule.x;
                this.xMultiplier = rotationBySpeedModule.xMultiplier;
                this.y = rotationBySpeedModule.y;
                this.yMultiplier = rotationBySpeedModule.yMultiplier;
                this.z = rotationBySpeedModule.z;
                this.zMultiplier = rotationBySpeedModule.zMultiplier;
            }
        }

        [System.Serializable]
        public class RotationOverLifetimeModule
        {
            /// <summary>
            /// Enable/disable the Rotation Over Lifetime module.
            /// </summary>
            public bool enabled;

            /// <summary>
            /// Set the rotation over lifetime on each axis separately.
            /// </summary>
            public bool separateAxes;

            /// <summary>
            /// Rotation over lifetime curve for the X axis.
            /// </summary>
            public MinMaxCurve x;

            /// <summary>
            /// Rotation multiplier around the X axis.
            /// </summary>
            public float xMultiplier;

            /// <summary>
            /// Rotation over lifetime curve for the Y axis.
            /// </summary>
            public MinMaxCurve y;

            /// <summary>
            /// Rotation multiplier around the Y axis.
            /// </summary>
            public float yMultiplier;

            /// <summary>
            /// Rotation over lifetime curve for the Z axis.
            /// </summary>
            public MinMaxCurve z;

            /// <summary>
            /// Rotation multiplier around the Z axis.
            /// </summary>
            public float zMultiplier;

            public RotationOverLifetimeModule()
            {
                Initialize();
            }

            public RotationOverLifetimeModule(ParticleSystem.RotationOverLifetimeModule rotationOverLifetimeModule)
            {
                this.enabled = rotationOverLifetimeModule.enabled;
                this.separateAxes = rotationOverLifetimeModule.separateAxes;
                this.x = rotationOverLifetimeModule.x;
                this.xMultiplier = rotationOverLifetimeModule.xMultiplier;
                this.y = rotationOverLifetimeModule.y;
                this.yMultiplier = rotationOverLifetimeModule.yMultiplier;
                this.z = rotationOverLifetimeModule.z;
                this.zMultiplier = rotationOverLifetimeModule.zMultiplier;
            }

            public void Initialize()
            {
                separateAxes = false;
                x = 0;
                xMultiplier = 0;
                y = 0;
                yMultiplier = 0;
                z = 0.7853981f;
                zMultiplier = 0.7853981f;
            }

            public static implicit operator RotationOverLifetimeModule(ParticleSystem.RotationOverLifetimeModule rotationOverLifetimeModule)
            {
                return new RotationOverLifetimeModule(rotationOverLifetimeModule);
            }

            public static implicit operator ParticleSystem.RotationOverLifetimeModule(RotationOverLifetimeModule rotationOverLifetimeModule)
            {
                return new ParticleSystem.RotationOverLifetimeModule()
                {
                    enabled = rotationOverLifetimeModule.enabled,
                    separateAxes = rotationOverLifetimeModule.separateAxes,
                    x = rotationOverLifetimeModule.x,
                    xMultiplier = rotationOverLifetimeModule.xMultiplier,
                    y = rotationOverLifetimeModule.y,
                    yMultiplier = rotationOverLifetimeModule.yMultiplier,
                    z = rotationOverLifetimeModule.z,
                    zMultiplier = rotationOverLifetimeModule.zMultiplier
                };
            }

            public void CopyTo(ParticleSystem.RotationOverLifetimeModule rotationOverLifetimeModule)
            {
                rotationOverLifetimeModule.enabled = enabled;
                rotationOverLifetimeModule.separateAxes = separateAxes;
                rotationOverLifetimeModule.x = x;
                rotationOverLifetimeModule.xMultiplier = xMultiplier;
                rotationOverLifetimeModule.y = y;
                rotationOverLifetimeModule.yMultiplier = yMultiplier;
                rotationOverLifetimeModule.z = z;
                rotationOverLifetimeModule.zMultiplier = zMultiplier;
            }
        }

        [System.Serializable]
        public class ShapeModule
        {
            internal ShapeModel model { get; set; }

            /// <summary>
            /// Enable/disable the Shape module.
            /// </summary>
            public bool enabled;

            /// <summary>
            /// Align particles based on their initial direction of travel.
            /// </summary>
            public bool alignToDirection;

            /// <summary>
            /// Angle of the cone.
            /// </summary>
            public float angle;

            /// <summary>
            /// Circle arc angle.
            /// </summary>
            public float arc;

            /// <summary>
            /// Scale of the box.
            /// </summary>
            public Vector3 box;

            /// <summary>
            /// Length of the cone.
            /// </summary>
            public float length;

            /// <summary>
            /// Mesh to emit particles from.
            /// </summary>
            public Mesh mesh
            {
                get
                {
                    if (model != null && model.gameObject)
                    {
                        var filter = model.gameObject.GetComponentInChildren<MeshFilter>();
                        return filter ? filter.sharedMesh : null;
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            /// <summary>
            /// MeshRenderer to emit particles from.
            /// </summary>
            public MeshRenderer meshRenderer
            {
                get
                {
                    if (model != null && model.gameObject)
                    {
                        return model.gameObject.GetComponentInChildren<MeshRenderer>();
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            /// <summary>
            /// SkinnedMeshRenderer to emit particles from.
            /// </summary>
            public SkinnedMeshRenderer skinnedMeshRenderer
            {
                get
                {
                    if (model != null && model.gameObject)
                    {
                        return model.gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
                    }
                    else
                    {
                        return null;
                    }
                }
            }



            /// <summary>
            /// Modulate the particle colors with the vertex colors, or the material color if no vertex colors exist.
            /// </summary>
            public bool useMeshColors;

            /// <summary>
            /// Emit from a single material, or the whole mesh.
            /// </summary>
            public bool useMeshMaterialIndex;

            /// <summary>
            /// Emit particles from a single material of a mesh.
            /// </summary>
            public int meshMaterialIndex;

            /// <summary>
            /// Apply a scaling factor to the mesh used for generating source positions.
            /// </summary>
            public float meshScale;

            /// <summary>
            /// Where on the mesh to emit particles from.
            /// </summary>
            public ParticleSystemMeshShapeType meshShapeType = ParticleSystemMeshShapeType.Vertex;

            /// <summary>
            /// Move particles away from the surface of the source mesh.
            /// </summary>
            public float normalOffset;

            /// <summary>
            /// Radius of the shape.
            /// </summary>
            public float radius;

            /// <summary>
            /// Randomizes the starting direction of particles.
            /// </summary>
            public float randomDirectionAmount;

            /// <summary>
            /// Type of shape to emit particles from.
            /// </summary>
            public ParticleSystemShapeType shapeType = ParticleSystemShapeType.Cone;

            /// <summary>
            /// Spherizes the starting direction of particles.
            /// </summary>
            public float sphericalDirectionAmount;

            public ShapeModule()
            {
                Initialize();
            }

            public ShapeModule(ParticleSystem.ShapeModule shapeModule)
            {
                this.alignToDirection = shapeModule.alignToDirection;
                this.angle = shapeModule.angle;
                this.arc = shapeModule.arc;
                this.box = shapeModule.box;
                this.enabled = shapeModule.enabled;
                this.length = shapeModule.length;

                this.meshMaterialIndex = shapeModule.meshMaterialIndex;
                this.meshScale = shapeModule.meshScale;
                this.meshShapeType = shapeModule.meshShapeType;
                this.normalOffset = shapeModule.normalOffset;
                this.radius = shapeModule.radius;
                this.randomDirectionAmount = shapeModule.randomDirectionAmount;
                this.shapeType = shapeModule.shapeType;
                this.sphericalDirectionAmount = shapeModule.sphericalDirectionAmount;
                this.useMeshColors = shapeModule.useMeshColors;
                this.useMeshMaterialIndex = shapeModule.useMeshMaterialIndex;
            }

            public void Initialize()
            {
                alignToDirection = false;
                angle = 25;
                arc = 360;
                box = Vector3.one;
                enabled = true;
                length = 5;
                meshMaterialIndex = 0;
                meshScale = 1;
                meshShapeType = ParticleSystemMeshShapeType.Vertex;
                normalOffset = 0;
                radius = 1;
                randomDirectionAmount = 0;
                shapeType = ParticleSystemShapeType.Cone;
                sphericalDirectionAmount = 0;
                useMeshColors = true;
                useMeshMaterialIndex = false;
            }


            public static implicit operator ShapeModule(ParticleSystem.ShapeModule shapeModule)
            {
                return new ShapeModule(shapeModule);
            }

            public static implicit operator ParticleSystem.ShapeModule(ShapeModule shapeModule)
            {
                return new ParticleSystem.ShapeModule()
                {
                    alignToDirection = shapeModule.alignToDirection,
                    angle = shapeModule.angle,
                    arc = shapeModule.arc,
                    box = shapeModule.box,
                    enabled = shapeModule.enabled,
                    length = shapeModule.length,

                    useMeshColors = shapeModule.useMeshColors,
                    useMeshMaterialIndex = shapeModule.useMeshMaterialIndex,
                    meshMaterialIndex = shapeModule.meshMaterialIndex,
                    meshScale = shapeModule.meshScale,
                    meshShapeType = shapeModule.meshShapeType,
                    normalOffset = shapeModule.normalOffset,
                    radius = shapeModule.radius,
                    randomDirectionAmount = shapeModule.randomDirectionAmount,
                    shapeType = shapeModule.shapeType,
                    sphericalDirectionAmount = shapeModule.sphericalDirectionAmount,
                };
            }

            public void CopyTo(ParticleSystem.ShapeModule shapeModule)
            {
                shapeModule.enabled = enabled;

                shapeModule.alignToDirection = alignToDirection;
                shapeModule.angle = angle;
                shapeModule.arc = arc;
                shapeModule.box = box;
                shapeModule.length = length;

                shapeModule.useMeshColors = useMeshColors;
                shapeModule.useMeshMaterialIndex = useMeshMaterialIndex;
                shapeModule.meshMaterialIndex = meshMaterialIndex;

                shapeModule.shapeType = shapeType;

                if (shapeType == ParticleSystemShapeType.Mesh)
                {
                    shapeModule.mesh = mesh;
                }
                else if (shapeType == ParticleSystemShapeType.MeshRenderer)
                {
                    shapeModule.meshRenderer = meshRenderer;
                }
                else if (shapeType == ParticleSystemShapeType.SkinnedMeshRenderer)
                {
                    shapeModule.skinnedMeshRenderer = skinnedMeshRenderer;
                }

                shapeModule.meshScale = meshScale;
                shapeModule.meshShapeType = meshShapeType;
                shapeModule.normalOffset = normalOffset;
                shapeModule.radius = radius;
                shapeModule.randomDirectionAmount = randomDirectionAmount;
                shapeModule.sphericalDirectionAmount = sphericalDirectionAmount;
            }
        }

        [System.Serializable]
        public class ShapeModel
        {
            internal ParticleWidget particleWidget { get; set; }
            internal Core.Asset asset { get; set; }
            internal Action updateAction;

            public string assetID;
            public int assetIndex;
            public bool useModelPrimitive;

            public ModelWidget.Primitive modelPrimitive;
            public GameObject gameObject;

            public void Initialize(ParticleWidget particleWidget, Action updateAction)
            {
                this.particleWidget = particleWidget;
                this.updateAction = updateAction;
                UpdateGameObject();
            }

            public void UpdateGameObject()
            {
                if (useModelPrimitive)
                {
                    gameObject = ModelWidget.GeneratePrimitive(modelPrimitive, particleWidget.transform);
                    gameObject.AddComponent<InstancedAssetWidget>();
                    gameObject.SetActive(false);
                }
                else if (!string.IsNullOrEmpty(assetID))
                {
                    particleWidget.StartCoroutine(FetchAsset(assetID));
                }
            }

            public IEnumerator FetchAsset(string assetID)
            {
                if (!string.IsNullOrEmpty(assetID))
                {
                    asset = Asset.AssetsManager.AddAsset(assetID);

                    if (asset != null)
                    {
                        gameObject = null;

                        if (asset.isLoaded)
                        {
                            var assets = asset.GetAssetGameObjects();

                            if (assets.Length > assetIndex)
                            {
                                gameObject = assets[assetIndex];
                            }
                            else
                            {
                                gameObject = assets.FirstOrDefault();
                                assetIndex = 0;
                            }
                        }
                        else
                        {
                            asset.onProcessEnd += OnAssetUpdate;
                            yield return particleWidget.StartCoroutine(asset.LoadAssetInBackground(particleWidget));

                            while (!gameObject && !asset.InProcess(Asset.Process.Error))
                                yield return null;
                        }

                        if (gameObject)
                        {
                            updateAction();
                        }
                    }
                }
            }

            protected void OnAssetUpdate(Asset sender, Asset.Process[] currentProcesses, Asset.Process endingProcess)
            {
                if (endingProcess == Asset.Process.Loading)
                {
                    if (sender.bundle)
                    {
                        var assets = asset.GetAssetGameObjects();

                        if (assets.Length > assetIndex)
                        {
                            gameObject = assets[assetIndex];
                        }
                        else
                        {
                            gameObject = assets.FirstOrDefault();
                            assetIndex = 0;
                        }
                    }

                    asset.onProcessEnd -= OnAssetUpdate;
                }
            }
        }

        [System.Serializable]
        public class SizeBySpeedModule
        {
            /// <summary>
            /// Enable/disable the Size By Speed module.
            /// </summary>
            public bool enabled;

            /// <summary>
            /// Apply the size curve between these minimum and maximum speeds.
            /// </summary>
            public Vector2 range = Vector2.one;

            /// <summary>
            /// Set the size by speed on each axis separately.
            /// </summary>
            public bool separateAxes;

            /// <summary>
            /// Curve to control particle size based on speed.
            /// </summary>
            public MinMaxCurve size;

            /// <summary>
            /// Size multiplier.
            /// </summary>
            public float sizeMultiplier;

            /// <summary>
            /// Size by speed curve for the X axis.
            /// </summary>
            public MinMaxCurve x;

            /// <summary>
            /// X axis size multiplier.
            /// </summary>
            public float xMultiplier;

            /// <summary>
            /// Size by speed curve for the Y axis.
            /// </summary>
            public MinMaxCurve y;

            /// <summary>
            /// Y axis size multiplier.
            /// </summary>
            public float yMultiplier;

            /// <summary>
            /// Size by speed curve for the Z axis.
            /// </summary>
            public MinMaxCurve z;

            /// <summary>
            /// Z axis size multiplier.
            /// </summary>
            public float zMultiplier;

            public SizeBySpeedModule()
            {
                Initialize();
            }

            public SizeBySpeedModule(ParticleSystem.SizeBySpeedModule sizeBySpeedModule)
            {
                this.enabled = sizeBySpeedModule.enabled;
                this.range = sizeBySpeedModule.range;
                this.separateAxes = sizeBySpeedModule.separateAxes;
                this.size = sizeBySpeedModule.size;
                this.sizeMultiplier = sizeBySpeedModule.sizeMultiplier;
                this.x = sizeBySpeedModule.x;
                this.xMultiplier = sizeBySpeedModule.xMultiplier;
                this.y = sizeBySpeedModule.y;
                this.yMultiplier = sizeBySpeedModule.yMultiplier;
                this.z = sizeBySpeedModule.z;
                this.zMultiplier = sizeBySpeedModule.zMultiplier;
            }

            public void Initialize()
            {
                range = new Vector2(0, 1);
                size = new MinMaxCurve(1, AnimationCurve.Linear(0, 1, 1, 1));
                sizeMultiplier = 1;
                separateAxes = false;
                x = new MinMaxCurve(1, AnimationCurve.Linear(0, 1, 1, 1));
                xMultiplier = 1;
                y = new MinMaxCurve(1, AnimationCurve.Linear(0, 1, 1, 1));
                yMultiplier = 1;
                z = new MinMaxCurve(1, AnimationCurve.Linear(0, 1, 1, 1));
                zMultiplier = 1;
            }

            public static implicit operator SizeBySpeedModule(ParticleSystem.SizeBySpeedModule sizeBySpeedModule)
            {
                return new SizeBySpeedModule(sizeBySpeedModule);
            }

            public static implicit operator ParticleSystem.SizeBySpeedModule(SizeBySpeedModule sizeBySpeedModule)
            {
                return new ParticleSystem.SizeBySpeedModule()
                {
                    enabled = sizeBySpeedModule.enabled,
                    range = sizeBySpeedModule.range,
                    separateAxes = sizeBySpeedModule.separateAxes,
                    size = sizeBySpeedModule.size,
                    sizeMultiplier = sizeBySpeedModule.sizeMultiplier,
                    x = sizeBySpeedModule.x,
                    xMultiplier = sizeBySpeedModule.xMultiplier,
                    y = sizeBySpeedModule.y,
                    yMultiplier = sizeBySpeedModule.yMultiplier,
                    z = sizeBySpeedModule.z,
                    zMultiplier = sizeBySpeedModule.zMultiplier
                };
            }

            public void CopyTo(ParticleSystem.SizeBySpeedModule sizeBySpeedModule)
            {
                sizeBySpeedModule.enabled = enabled;
                sizeBySpeedModule.range = range;
                sizeBySpeedModule.separateAxes = separateAxes;
                sizeBySpeedModule.size = size;
                sizeBySpeedModule.sizeMultiplier = sizeMultiplier;
                sizeBySpeedModule.x = x;
                sizeBySpeedModule.xMultiplier = xMultiplier;
                sizeBySpeedModule.y = y;
                sizeBySpeedModule.yMultiplier = yMultiplier;
                sizeBySpeedModule.z = z;
                sizeBySpeedModule.zMultiplier = zMultiplier;
            }
        }

        [System.Serializable]
        public class SizeOverLifetimeModule
        {
            /// <summary>
            /// Enable/disable the Size Over Lifetime module.
            /// </summary>
            public bool enabled;

            /// <summary>
            /// Set the size over lifetime on each axis separately.
            /// </summary>
            public bool separateAxes;

            /// <summary>
            /// Curve to control particle size based on lifetime.
            /// </summary>
            public MinMaxCurve size;

            /// <summary>
            /// Size multiplier.
            /// </summary>
            public float sizeMultiplier;

            /// <summary>
            /// Size over lifetime curve for the X axis.
            /// </summary>
            public MinMaxCurve x;

            /// <summary>
            /// X axis size multiplier.
            /// </summary>
            public float xMultiplier;

            /// <summary>
            /// Size over lifetime curve for the Y axis.
            /// </summary>
            public MinMaxCurve y;

            /// <summary>
            /// Y axis size multiplier.
            /// </summary>
            public float yMultiplier;

            /// <summary>
            /// Size over lifetime curve for the Z axis.
            /// </summary>
            public MinMaxCurve z;

            /// <summary>
            /// Z axis size multiplier.
            /// </summary>
            public float zMultiplier;

            public SizeOverLifetimeModule()
            {
                Initialize();
            }

            public SizeOverLifetimeModule(ParticleSystem.SizeOverLifetimeModule sizeOverLifetimeModule)
            {
                this.enabled = sizeOverLifetimeModule.enabled;
                this.separateAxes = sizeOverLifetimeModule.separateAxes;
                this.size = sizeOverLifetimeModule.size;
                this.sizeMultiplier = sizeOverLifetimeModule.sizeMultiplier;
                this.x = sizeOverLifetimeModule.x;
                this.xMultiplier = sizeOverLifetimeModule.xMultiplier;
                this.y = sizeOverLifetimeModule.y;
                this.yMultiplier = sizeOverLifetimeModule.yMultiplier;
                this.z = sizeOverLifetimeModule.z;
                this.zMultiplier = sizeOverLifetimeModule.zMultiplier;
            }

            public void Initialize()
            {
                size = new MinMaxCurve(1, AnimationCurve.Linear(0, 1, 1, 1));
                sizeMultiplier = 1;
                separateAxes = false;
                x = new MinMaxCurve(1, AnimationCurve.Linear(0, 1, 1, 1));
                xMultiplier = 1;
                y = new MinMaxCurve(1, AnimationCurve.Linear(0, 1, 1, 1));
                yMultiplier = 1;
                z = new MinMaxCurve(1, AnimationCurve.Linear(0, 1, 1, 1));
                zMultiplier = 1;
            }

            public static implicit operator SizeOverLifetimeModule(ParticleSystem.SizeOverLifetimeModule sizeOverLifetimeModule)
            {
                return new SizeOverLifetimeModule(sizeOverLifetimeModule);
            }

            public static implicit operator ParticleSystem.SizeOverLifetimeModule(SizeOverLifetimeModule sizeOverLifetimeModule)
            {
                return new ParticleSystem.SizeOverLifetimeModule()
                {
                    enabled = sizeOverLifetimeModule.enabled,
                    separateAxes = sizeOverLifetimeModule.separateAxes,
                    size = sizeOverLifetimeModule.size,
                    sizeMultiplier = sizeOverLifetimeModule.sizeMultiplier,
                    x = sizeOverLifetimeModule.x,
                    xMultiplier = sizeOverLifetimeModule.xMultiplier,
                    y = sizeOverLifetimeModule.y,
                    yMultiplier = sizeOverLifetimeModule.yMultiplier,
                    z = sizeOverLifetimeModule.z,
                    zMultiplier = sizeOverLifetimeModule.zMultiplier
                };

            }

            public void CopyTo(ParticleSystem.SizeOverLifetimeModule sizeOverLifetimeModule)
            {
                sizeOverLifetimeModule.enabled = sizeOverLifetimeModule.enabled;
                sizeOverLifetimeModule.separateAxes = sizeOverLifetimeModule.separateAxes;
                sizeOverLifetimeModule.size = sizeOverLifetimeModule.size;
                sizeOverLifetimeModule.sizeMultiplier = sizeOverLifetimeModule.sizeMultiplier;
                sizeOverLifetimeModule.x = sizeOverLifetimeModule.x;
                sizeOverLifetimeModule.xMultiplier = sizeOverLifetimeModule.xMultiplier;
                sizeOverLifetimeModule.y = sizeOverLifetimeModule.y;
                sizeOverLifetimeModule.yMultiplier = sizeOverLifetimeModule.yMultiplier;
                sizeOverLifetimeModule.z = sizeOverLifetimeModule.z;
                sizeOverLifetimeModule.zMultiplier = sizeOverLifetimeModule.zMultiplier;
            }
    }

        [System.Serializable]
        public class SubEmittersModule
        {
            /// <summary>
            /// Enable/disable the Sub Emitters module.
            /// </summary>
            public bool enabled;

            public List<ParticleSystem> subEmitters;

            public void Initialize()
            {
            }

        }

        [System.Serializable]
        public class TextureSheetAnimationModule
        {
            /// <summary>
            /// Specifies the animation type.
            /// </summary>
            public ParticleSystemAnimationType animation = ParticleSystemAnimationType.WholeSheet;

            /// <summary>
            /// Specifies how many times the animation will loop during the lifetime of the particle.
            /// </summary>
            public int cycleCount = 1;

            /// <summary>
            /// Enable/disable the Texture Sheet Animation module.
            /// </summary>
            public bool enabled;

            /// <summary>
            /// Flip the U coordinate on particles, causing them to appear mirrored horizontally.
            /// </summary>
            public float flipU;

            /// <summary>
            /// Flip the V coordinate on particles, causing them to appear mirrored vertically.
            /// </summary>
            public float flipV;

            /// <summary>
            /// Curve to control which frame of the texture sheet animation to play.
            /// </summary>
            public MinMaxCurve frameOverTime;

            /// <summary>
            /// Frame over time mutiplier.
            /// </summary>
            public float frameOverTimeMultiplier;

            /// <summary>
            /// Defines the tiling of the texture in the X axis.
            /// </summary>
            public int numTilesX = 1;

            /// <summary>
            /// Defines the tiling of the texture in the Y axis.
            /// </summary>
            public int numTilesY = 1;

            /// <summary>
            /// Explicitly select which row of the texture sheet is used, when ParticleSystem.TextureSheetAnimationModule.useRandomRow is set to false.
            /// </summary>
            public int rowIndex;

            /// <summary>
            /// Define a random starting frame for the texture sheet animation.
            /// </summary>
            public MinMaxCurve startFrame;

            /// <summary>
            /// Starting frame multiplier.
            /// </summary>
            public float startFrameMultiplier;

            /// <summary>
            /// Use a random row of the texture sheet for each particle emitted.
            /// </summary>
            public bool useRandomRow;

            /// <summary>
            /// Choose which UV channels will receive texture animation.
            /// </summary>
            //public UnityEngine.Rendering.UVChannelFlags uvChannelMask;
            public int uvChannelMask;

            public TextureSheetAnimationModule()
            {
                Initialize();
            }

            public TextureSheetAnimationModule(ParticleSystem.TextureSheetAnimationModule textureSheetAnimationModule)
            {
                this.animation = textureSheetAnimationModule.animation;
                this.cycleCount = textureSheetAnimationModule.cycleCount;
                this.enabled = textureSheetAnimationModule.enabled;
                this.flipU = textureSheetAnimationModule.flipU;
                this.flipV = textureSheetAnimationModule.flipV;
                this.frameOverTime = textureSheetAnimationModule.frameOverTime;
                this.frameOverTimeMultiplier = textureSheetAnimationModule.frameOverTimeMultiplier;
                this.numTilesX = textureSheetAnimationModule.numTilesX;
                this.numTilesY = textureSheetAnimationModule.numTilesY;
                this.rowIndex = textureSheetAnimationModule.rowIndex;
                this.startFrame = textureSheetAnimationModule.startFrame;
                this.startFrameMultiplier = textureSheetAnimationModule.startFrameMultiplier;
                this.useRandomRow = textureSheetAnimationModule.useRandomRow;
                this.uvChannelMask = (int)textureSheetAnimationModule.uvChannelMask;
            }

            public void Initialize()
            {
                animation = ParticleSystemAnimationType.WholeSheet;
                cycleCount = 1;
                frameOverTime = new MinMaxCurve(1, AnimationCurve.Linear(0, 0, 1, 1));
                startFrame = 0;
                numTilesX = 1;
                numTilesY = 1;
                useRandomRow = true;
                uvChannelMask = -1;
            }

            public static implicit operator TextureSheetAnimationModule(ParticleSystem.TextureSheetAnimationModule textureSheetAnimationModule)
            {
                return new TextureSheetAnimationModule(textureSheetAnimationModule);
            }

            public static implicit operator ParticleSystem.TextureSheetAnimationModule(TextureSheetAnimationModule textureSheetAnimationModule)
            {
                return new ParticleSystem.TextureSheetAnimationModule()
                {
                    animation = textureSheetAnimationModule.animation,
                    cycleCount = textureSheetAnimationModule.cycleCount,
                    enabled = textureSheetAnimationModule.enabled,
                    flipU = textureSheetAnimationModule.flipU,
                    flipV = textureSheetAnimationModule.flipV,
                    frameOverTime = textureSheetAnimationModule.frameOverTime,
                    frameOverTimeMultiplier = textureSheetAnimationModule.frameOverTimeMultiplier,
                    numTilesX = textureSheetAnimationModule.numTilesX,
                    numTilesY = textureSheetAnimationModule.numTilesY,
                    rowIndex = textureSheetAnimationModule.rowIndex,
                    startFrame = textureSheetAnimationModule.startFrame,
                    startFrameMultiplier = textureSheetAnimationModule.startFrameMultiplier,
                    useRandomRow = textureSheetAnimationModule.useRandomRow,
                    uvChannelMask = (UnityEngine.Rendering.UVChannelFlags)textureSheetAnimationModule.uvChannelMask
                };
            }

            public void CopyTo(ParticleSystem.TextureSheetAnimationModule textureSheetAnimationModule)
            {
                textureSheetAnimationModule.animation = animation;
                textureSheetAnimationModule.cycleCount = cycleCount;
                textureSheetAnimationModule.enabled = enabled;
                textureSheetAnimationModule.flipU = flipU;
                textureSheetAnimationModule.flipV = flipV;
                textureSheetAnimationModule.frameOverTime = frameOverTime;
                textureSheetAnimationModule.frameOverTimeMultiplier = frameOverTimeMultiplier;
                textureSheetAnimationModule.numTilesX = numTilesX;
                textureSheetAnimationModule.numTilesY = numTilesY;
                textureSheetAnimationModule.rowIndex = rowIndex;
                textureSheetAnimationModule.startFrame = startFrame;
                textureSheetAnimationModule.startFrameMultiplier = startFrameMultiplier;
                textureSheetAnimationModule.useRandomRow = useRandomRow;
                textureSheetAnimationModule.uvChannelMask = (UnityEngine.Rendering.UVChannelFlags)uvChannelMask;
            }
        }

        [System.Serializable]
        public class TrailModule
        {
            /// <summary>
            /// The gradient controlling the trail colors during the lifetime of the attached particle.
            /// </summary>
            public MinMaxGradient colorOverLifetime;

            /// <summary>
            /// The gradient controlling the trail colors over the length of the trail.
            /// </summary>
            public MinMaxGradient colorOverTrail;

            /// <summary>
            /// If enabled, Trails will disappear immediately when their owning particle dies. Otherwise, the trail will persist until all its points have naturally expired, based on its lifetime.
            /// </summary>
            public bool dieWithParticles;

            /// <summary>
            /// Enable/disable the Trail module.
            /// </summary>
            public bool enabled;

            /// <summary>
            /// Toggle whether the trail will inherit the particle color as its starting color.
            /// </summary>
            public bool inheritParticleColor;

            /// <summary>
            /// The curve describing the trail lifetime, throughout the lifetime of the particle.
            /// </summary>
            public MinMaxCurve lifetime;

            /// <summary>
            /// Change the lifetime multiplier.
            /// </summary>
            public float lifetimeMultiplier;

            /// <summary>
            /// Set the minimum distance each trail can travel before a new vertex is added to it.
            /// </summary>
            public float minVertexDistance;

            /// <summary>
            /// Choose what proportion of particles will receive a trail.
            /// </summary>
            public float ratio;

            /// <summary>
            /// Set whether the particle size will act as a multiplier on top of the trail lifetime.
            /// </summary>
            public bool sizeAffectsLifetime;

            /// <summary>
            /// Set whether the particle size will act as a multiplier on top of the trail width.
            /// </summary>
            public bool sizeAffectsWidth;

            /// <summary>
            /// Choose whether the U coordinate of the trail texture is tiled or stretched.
            /// </summary>
            public ParticleSystemTrailTextureMode textureMode;

            /// <summary>
            /// The curve describing the width, of each trail point.
            /// </summary>
            public MinMaxCurve widthOverTrail;

            /// <summary>
            /// Change the width multiplier.
            /// </summary>
            public float widthOverTrailMultiplier;

            /// <summary>
            /// Drop new trail points in world space, regardless of Particle System Simulation Space.
            /// </summary>
            public bool worldSpace;

            public TrailModule()
            {
                Initialize();
            }

            public TrailModule(ParticleSystem.TrailModule trailModule)
            {
                this.colorOverLifetime = trailModule.colorOverLifetime;
                this.colorOverTrail = trailModule.colorOverTrail;
                this.dieWithParticles = trailModule.dieWithParticles;
                this.enabled = trailModule.enabled;
                this.inheritParticleColor = trailModule.inheritParticleColor;
                this.lifetime = trailModule.lifetime;
                this.lifetimeMultiplier = trailModule.lifetimeMultiplier;
                this.minVertexDistance = trailModule.minVertexDistance;
                this.ratio = trailModule.ratio;
                this.sizeAffectsLifetime = trailModule.sizeAffectsLifetime;
                this.sizeAffectsWidth = trailModule.sizeAffectsWidth;
                this.textureMode = trailModule.textureMode;
                this.widthOverTrail = trailModule.widthOverTrail;
                this.widthOverTrailMultiplier = trailModule.widthOverTrailMultiplier;
                this.worldSpace = trailModule.worldSpace;
            }

            public void Initialize()
            {
                colorOverLifetime = Color.white;
                colorOverTrail = Color.white;
                dieWithParticles = true;
                inheritParticleColor = true;
                lifetime = 1;
                lifetimeMultiplier = 1;

                minVertexDistance = 0.2f;
                ratio = 1;
                sizeAffectsLifetime = false;
                sizeAffectsWidth = true;
                textureMode = ParticleSystemTrailTextureMode.Stretch;
                widthOverTrail = 1;
                widthOverTrailMultiplier = 1;
                worldSpace = false;
            }

            public static implicit operator TrailModule(ParticleSystem.TrailModule trailModule)
            {
                return new TrailModule(trailModule);
            }

            public static implicit operator ParticleSystem.TrailModule(TrailModule trailModule)
            {
                return new ParticleSystem.TrailModule()
                {
                    colorOverLifetime = trailModule.colorOverLifetime,
                    colorOverTrail = trailModule.colorOverTrail,
                    dieWithParticles = trailModule.dieWithParticles,
                    enabled = trailModule.enabled,
                    inheritParticleColor = trailModule.inheritParticleColor,
                    lifetime = trailModule.lifetime,
                    lifetimeMultiplier = trailModule.lifetimeMultiplier,
                    minVertexDistance = trailModule.minVertexDistance,
                    ratio = trailModule.ratio,
                    sizeAffectsLifetime = trailModule.sizeAffectsLifetime,
                    sizeAffectsWidth = trailModule.sizeAffectsWidth,
                    textureMode = trailModule.textureMode,
                    widthOverTrail = trailModule.widthOverTrail,
                    widthOverTrailMultiplier = trailModule.widthOverTrailMultiplier,
                    worldSpace = trailModule.worldSpace
                };
            }

            public void CopyTo(ParticleSystem.TrailModule trailModule)
            {
                trailModule.colorOverLifetime = colorOverLifetime;
                trailModule.colorOverTrail = colorOverTrail;
                trailModule.dieWithParticles = dieWithParticles;
                trailModule.enabled = enabled;
                trailModule.inheritParticleColor = inheritParticleColor;
                trailModule.lifetime = lifetime;
                trailModule.lifetimeMultiplier = lifetimeMultiplier;
                trailModule.minVertexDistance = minVertexDistance;
                trailModule.ratio = ratio;
                trailModule.sizeAffectsLifetime = sizeAffectsLifetime;
                trailModule.sizeAffectsWidth = sizeAffectsWidth;
                trailModule.textureMode = textureMode;
                trailModule.widthOverTrail = widthOverTrail;
                trailModule.widthOverTrailMultiplier = widthOverTrailMultiplier;
                trailModule.worldSpace = worldSpace;
            }
        }

        [System.Serializable]
        public class TriggerModule
        {
            /// <summary>
            /// Enable/disable the Trigger module.
            /// </summary>
            public bool enabled;

            /// <summary>
            /// Choose what action to perform when particles enter the trigger volume.
            /// </summary>
            public ParticleSystemOverlapAction enter;

            /// <summary>
            /// Choose what action to perform when particles leave the trigger volume.
            /// </summary>
            public ParticleSystemOverlapAction exit;

            /// <summary>
            /// Choose what action to perform when particles are inside the trigger volume.
            /// </summary>
            public ParticleSystemOverlapAction inside;

            ///// <summary>
            ///// The maximum number of collision shapes that can be attached to this particle system trigger.
            ///// </summary>
            //public int maxColliderCount { get; }


            /// <summary>
            /// Choose what action to perform when particles are outside the trigger volume.
            /// </summary>
            public ParticleSystemOverlapAction outside;

            /// <summary>
            /// A multiplier applied to the size of each particle before overlaps are processed.
            /// </summary>
            public float radiusScale;

            //
            // Summary:
            //     ///
            //     Get a collision shape associated with this particle system trigger.
            //     ///
            //
            // Parameters:
            //   index:
            //     Which collider to return.
            //
            // Returns:
            //     ///
            //     The collider at the given index.
            //     ///
            //public Component GetCollider(int index);
            //
            // Summary:
            //     ///
            //     Set a collision shape associated with this particle system trigger.
            //     ///
            //
            // Parameters:
            //   index:
            //     Which collider to set.
            //
            //   collider:
            //     The collider to associate with this trigger.
            //public void SetCollider(int index, Component collider);

            public TriggerModule()
            {
                Initialize();
            }

            public TriggerModule(ParticleSystem.TriggerModule triggerModule)
            {
                this.enabled = triggerModule.enabled;
                this.enter = triggerModule.enter;
                this.exit = triggerModule.exit;
                this.inside = triggerModule.inside;
                this.outside = triggerModule.outside;
                this.radiusScale = triggerModule.radiusScale;
            }

            public void Initialize()
            {
                radiusScale = 1;
                enter = ParticleSystemOverlapAction.Ignore;
                exit = ParticleSystemOverlapAction.Ignore;
                inside = ParticleSystemOverlapAction.Kill;
                outside = ParticleSystemOverlapAction.Ignore;
            }

            public static implicit operator TriggerModule(ParticleSystem.TriggerModule triggerModule)
            {
                return new TriggerModule(triggerModule);
            }

            public static implicit operator ParticleSystem.TriggerModule(TriggerModule triggerModule)
            {
                return new ParticleSystem.TriggerModule()
                {
                    enabled = triggerModule.enabled,
                    enter = triggerModule.enter,
                    exit = triggerModule.exit,
                    inside = triggerModule.inside,
                    outside = triggerModule.outside,
                    radiusScale = triggerModule.radiusScale
                };
            }

            public void CopyTo(ParticleSystem.TriggerModule triggerModule)
            {
                triggerModule.enabled = enabled;
                triggerModule.enter = enter;
                triggerModule.exit = exit;
                triggerModule.inside = inside;
                triggerModule.outside = outside;
                triggerModule.radiusScale = radiusScale;
            }

        }

        [System.Serializable]
        public class VelocityOverLifetimeModule
        {
            /// <summary>
            /// Enable/disable the Velocity Over Lifetime module.
            /// </summary>
            public bool enabled;

            /// <summary>
            /// Specifies if the velocities are in local space (rotated with the transform) or world space.
            /// </summary>
            public ParticleSystemSimulationSpace space;

            /// <summary>
            /// Curve to control particle speed based on lifetime, on the X axis.
            /// </summary>
            public MinMaxCurve x;

            /// <summary>
            /// X axis speed multiplier.
            /// </summary>
            public float xMultiplier;

            /// <summary>
            /// Curve to control particle speed based on lifetime, on the Y axis.
            /// </summary>
            public MinMaxCurve y;

            /// <summary>
            /// Y axis speed multiplier.
            /// </summary>
            public float yMultiplier;

            /// <summary>
            /// Curve to control particle speed based on lifetime, on the Z axis.
            /// </summary>
            public MinMaxCurve z;

            /// <summary>
            /// Z axis speed multiplier.
            /// </summary>
            public float zMultiplier;

            public VelocityOverLifetimeModule()
            {
                Initialize();
            }

            public VelocityOverLifetimeModule(ParticleSystem.VelocityOverLifetimeModule velocityOverLifetimeModule)
            {
                this.enabled = velocityOverLifetimeModule.enabled;
                this.space = velocityOverLifetimeModule.space;
                this.x = velocityOverLifetimeModule.x;
                this.xMultiplier = velocityOverLifetimeModule.xMultiplier;
                this.y = velocityOverLifetimeModule.y;
                this.yMultiplier = velocityOverLifetimeModule.yMultiplier;
                this.z = velocityOverLifetimeModule.z;
                this.zMultiplier = velocityOverLifetimeModule.zMultiplier;
            }

            public void Initialize()
            {
                space = ParticleSystemSimulationSpace.Local;
            }

            public static implicit operator VelocityOverLifetimeModule(ParticleSystem.VelocityOverLifetimeModule velocityOverLifetimeModule)
            {
                return new VelocityOverLifetimeModule(velocityOverLifetimeModule);
            }

            public static implicit operator ParticleSystem.VelocityOverLifetimeModule(VelocityOverLifetimeModule velocityOverLifetimeModule)
            {
                return new VelocityOverLifetimeModule()
                {
                    enabled = velocityOverLifetimeModule.enabled,
                    space = velocityOverLifetimeModule.space,
                    x = velocityOverLifetimeModule.x,
                    xMultiplier = velocityOverLifetimeModule.xMultiplier,
                    y = velocityOverLifetimeModule.y,
                    yMultiplier = velocityOverLifetimeModule.yMultiplier,
                    z = velocityOverLifetimeModule.z,
                    zMultiplier = velocityOverLifetimeModule.zMultiplier
                };
            }

            public void CopyTo(ParticleSystem.VelocityOverLifetimeModule velocityOverLifetimeModule)
            {
                velocityOverLifetimeModule.enabled = enabled;
                velocityOverLifetimeModule.space = space;
                velocityOverLifetimeModule.x = x;
                velocityOverLifetimeModule.xMultiplier = xMultiplier;
                velocityOverLifetimeModule.y = y;
                velocityOverLifetimeModule.yMultiplier = yMultiplier;
                velocityOverLifetimeModule.z = z;
                velocityOverLifetimeModule.zMultiplier = zMultiplier;
            }
        }
    }
}