using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Spaces.LBE
{
    public class TrackingVolumeZone : MonoBehaviour
    {
        // Static stuff

        protected static HashSet<TrackingVolumeZone> s_ZoneList;
        public static HashSet<TrackingVolumeZone> Zones
        {
            get
            {
                if (s_ZoneList == null)
                    s_ZoneList = new HashSet<TrackingVolumeZone>();

                return s_ZoneList;
            }
        }

        protected static bool Add(TrackingVolumeZone zone)
        {
            if (s_ZoneList == null)
                s_ZoneList = new HashSet<TrackingVolumeZone>();

            if (!s_ZoneList.Any(z => z && z.name == zone.name))
            {
                return s_ZoneList.Add(zone);
            }

            return false;
        }

        protected static bool Remove(TrackingVolumeZone zone)
        {
            if (s_ZoneList != null)
            {
                return s_ZoneList.Remove(zone);
            }

            return false;
        }

        public static event System.Action<TrackingVolumeZone, TrackedObject> OnZoneEntered;
        public static event System.Action<TrackingVolumeZone, TrackedObject> OnZoneExited;

        // Instance stuff
        public Vector3 ZoneOffset = Vector3.zero;
        public Vector3 ZoneSize = Vector3.one;

        [SerializeField]
        protected Color ZoneColor = Color.green;

        [HideInInspector]
        public Bounds Zone;

        public TrackingVolumeZone[] m_VisibleZones;
        public TrackingVolumeZone[] m_ConnectedZones;

        protected HashSet<TrackedObject> m_priorContents;
        protected HashSet<TrackedObject> priorContents
        {
            get
            {
                if (m_priorContents == null)
                    m_priorContents = new HashSet<TrackedObject>();

                return m_priorContents;
            }
        }

        protected HashSet<TrackedObject> m_Contents;
        public HashSet<TrackedObject> Contents
        {
            get
            {
                if (m_Contents == null)
                    m_Contents = new HashSet<TrackedObject>();

                return m_Contents;
            }
        }

        public bool Contains(TrackedObject trackedObject)
        {
            return Contents.Contains(trackedObject);
        }

        public bool IsVisible(TrackedObject trackedObject)
        {
            return Contents.Contains(trackedObject) || m_VisibleZones.Any(zone => zone && zone.Contents.Contains(trackedObject));
        }

        public float updateFreq = 0.1f;
        protected float elapsedTime;

        public bool overrideActive;
        public bool disposeOnSceneUnload;

        public bool isActiveZone
        {
            get
            {
                if (overrideActive)
                    return true;

                if (TrackedPlayer.LocalPlayer)
                {
                    return Contents.Contains(TrackedPlayer.LocalPlayer);
                }
                else
                {
                    // TODO: add Observer Cameras to list of things that triggers activation of Zone ?

                    if (!Application.isPlaying)
                        return FindObjectsOfType<Player>().Any(player => player.m_TrackedPlayer && Contents.Contains(player.m_TrackedPlayer));
                    else
                        return Spaces.LBE.SpacesNetworkManager.Instance.ConnectedPlayerList.Any(player => player.m_TrackedPlayer && Contents.Contains(player.m_TrackedPlayer));
                }
            }
        }

        protected virtual void Awake()
        {
            if (TrackingVolumeZone.Add(this))
            {
                TrackingVolume.Add(transform, true);
            }
            else
            {
                gameObject.SetActive(false);
            }

            elapsedTime = 0;

            if (disposeOnSceneUnload)
                DisposalProxy.Dispose(gameObject);
        }

        protected virtual void Update()
        {
            Zone.center = transform.position + ZoneOffset;
            Zone.size = ZoneSize;

            elapsedTime += Time.deltaTime;

            if (elapsedTime > updateFreq)
            {
                elapsedTime = 0;
                UpdateContents();
            }
        }

        protected virtual void OnDestroy()
        {
            TrackingVolumeZone.Remove(this);
        }

        protected virtual void UpdateContents()
        {
            foreach (var trackedObject in TrackedObject.TrackedObjects)
            {
                if (trackedObject == TrackedPlayer.LocalPlayer || trackedObject.Trackers.Any(t => t != null && t.trackerContinuity && t.trackerContinuity.IsTracking))
                {
                    if (trackedObject.Trackers.Any(t => t != null && t.trackerContinuity && Zone.Contains(t.optitrackRigidBody.transform.position)))
                    {
                        // Some of the Tracked Object's RBs are inside this zone

                        if (Contents.Contains(trackedObject))
                        {
                            // Object already present
                            //Debug.Log(trackedObject + " is in zone " + this.name);
                        }
                        else if (priorContents.Remove(trackedObject))
                        {
                            // Object was last in this zone
                            if (Contents.Add(trackedObject))
                            {
                                Spaces.LBE.DebugLog.Log("tracker", trackedObject.name + " [Returned to " + this.name + "]");

                                if (OnZoneEntered != null)
                                {
                                    OnZoneEntered(this, trackedObject);
                                }
                            }
                        }
                        else
                        {
                            // Object has not been in this zone recently

                            if (m_ConnectedZones.Length == 0
                                || m_ConnectedZones.Any(z => z && (z.Contents.Contains(trackedObject) || z.priorContents.Contains(trackedObject)))
                                || !Zones.Any(z => z && (z.Contents.Contains(trackedObject) || z.priorContents.Contains(trackedObject)))
                                )
                            {
                                if (Contents.Add(trackedObject))
                                {
                                    // Object has just entered zone!
                                    Spaces.LBE.DebugLog.Log("tracker", trackedObject.name + " [Entered " + this.name + "]");

                                    if (OnZoneEntered != null)
                                    {
                                        OnZoneEntered(this, trackedObject);
                                    }
                                }
                            }
                            else
                            {
                                // Object isn't entering via a valid edge
                                Spaces.LBE.DebugLog.Log("tracker", trackedObject.name + " [Entered " + this.name + " from an invalid edge]");
                            }
                        }
                    }
                    else if (TrackedPlayer.LocalPlayer && trackedObject == TrackedPlayer.LocalPlayer && Zone.Contains(TrackedPlayer.LocalPlayer.HmdTracker.transform.position))
                    {
                        // The current TrackedObject is the Local Player and its the Hmd is inside this zone

                        if (Contents.Contains(TrackedPlayer.LocalPlayer))
                        {
                            // Local Player is already in this zone
                        }
                        else if (m_ConnectedZones.Length == 0
                            || m_ConnectedZones.Any(z => z && (z.Contents.Contains(TrackedPlayer.LocalPlayer) || z.priorContents.Contains(TrackedPlayer.LocalPlayer)))
                            || !Zones.Any(z => z && (z.Contents.Contains(TrackedPlayer.LocalPlayer) || z.priorContents.Contains(TrackedPlayer.LocalPlayer)))
                            )
                        {
                            if (Contents.Add(TrackedPlayer.LocalPlayer))
                            {
                                // LocalPlayer has just entered this zone! Zone now active.
                                Spaces.LBE.DebugLog.Log("tracker", trackedObject.name + " [Entered " + this.name + "]");

                                if (OnZoneEntered != null)
                                {
                                    OnZoneEntered(this, trackedObject);
                                }
                            }
                        }
                        else
                        {
                            // Player isn't entering via a valid edge
                            Spaces.LBE.DebugLog.Log("tracker", TrackedPlayer.LocalPlayer.name + " [Entered " + this.name + " from an invalid edge]");
                        }
                    }
                    else
                    {
                        // None of this object's trackers are inside this zone currently

                        // TODO: Decide if player can leave if not entering a valid zone to do so. Do we raise an alarm?
                        //if (m_ConnectedZones.Any(z => z.Contents.Contains(trackedObject) || z.priorContents.Contains(trackedObject)))

                        if (Contents.Remove(trackedObject))
                        {
                            // Object has just left zone
                            priorContents.Add(trackedObject);
                            Spaces.LBE.DebugLog.Log("tracker", trackedObject.name + " [Exited " + this.name + "]");

                            if (OnZoneExited != null)
                            {
                                OnZoneExited(this, trackedObject);
                            }
                        }
                        else if (priorContents.Contains(trackedObject))
                        {
                            // TODO: determine better criteria for removing objects from priors

                            if (Zones.Any(z => z != this && z.Contents.Contains(trackedObject)))
                            {
                                // Object is still contained in some other zone, so we will go ahead and remove it from prior contents

                                if (priorContents.Remove(trackedObject))
                                {
                                    Spaces.LBE.DebugLog.Log("tracker", trackedObject.name + " [No longer in Prior Contents of " + this.name + "]");
                                }
                            }
                            else
                            {
                                // Object has been out of zone for an entire update cycle and is in no other zone, therefore we will keep it in this one's priors.
                                // This will allow it to re-enter this zone rather than only be able to enter via a start zone.
                            }
                        }
                        else
                        {
                            // Object is not in any zone...
                        }
                    }
                }
            }
        }

        public void InitializeZone()
        {
            Zone = new Bounds(transform.position + ZoneOffset, ZoneSize);
        }

        protected virtual void OnDrawGizmos()
        {
            Zone.center = transform.position + ZoneOffset;
            Zone.size = ZoneSize;

            Gizmos.color = ZoneColor;

            if (isActiveZone)
            {
                Gizmos.DrawSphere(Zone.center, 0.1f);
            }

            Gizmos.DrawWireCube(Zone.center, Zone.size);
        }
    }
}