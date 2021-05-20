using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Spaces.LBE
{
    public class RallyPoint : MonoBehaviour
    {
        public enum State
        {
            ready,
            rallying,
            rallied,
            returning,
            returned
        }


        public Transform Marker;
        public List<Renderer> DirectionIndicator;
        public List<Renderer> ReturnZoneIndicator;
        public Collider RallyZone;

        //public float FadeInDuration;
        //public float FadeOutOutDuration;
        public float DelayAfterRallied;

        public string RallyTag;
        public UnityEngine.Events.UnityEvent OnRallied;
        public UnityEngine.Events.UnityEvent OnReturned;

        public GameObject[] rallyObjects;
        public List<GameObject> objectsRallied;
        private Dictionary<Renderer, bool> markerRenderers;

        public State currentState;
        public bool rallyOnStart = true;
        private Animator markerAnimator;

        private bool initialized;

        private void Awake()
        {
            initialized = false;
        }

        private void Start()
        {
            if (!initialized)
                Initialize();

            StartCoroutine(UpdateCurrentPlayers());

            if (rallyOnStart)
            {
                Rally();
            }
            else
            {
                Hide();
            }
        }

        private void Update()
        {
            // This check may only need to be only performed every 0.1 sec or so...
            // But, doing this in Update instead of a coroutine so it will always be running for going back and forth
            switch (currentState)
            {
                case State.ready:
                    break;
                case State.rallying:
                    {
                        if (rallyObjects.Length > 0 && rallyObjects.All(obj => objectsRallied.Contains(obj)))
                        {
                            Rallied();
                        }
                        else
                        {
                            if (RallyZone)
                            {
                                // Check if remaining unrallied objects are inside Rally Zone bounds.

                                foreach (var obj in rallyObjects.Where(o => !objectsRallied.Contains(o)))
                                {
                                    if (obj && RallyZone.bounds.Contains(obj.transform.position))
                                    {
                                        if (!objectsRallied.Contains(obj))
                                        {
                                            Debug.Log(this.name + " [" + obj.name + " Reached Rally Zone]");
                                            objectsRallied.Add(obj);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Debug.LogWarning(this.name + " [No Rally Zone Defined]");
                            }
                        }
                    }
                    break;
                case State.rallied:
                    {
                        if (rallyObjects.Length > 0 && objectsRallied.Count == 0)
                        {
                            ChangeState(State.returning);

                            foreach(var renderer in ReturnZoneIndicator)
                            {
                                renderer.enabled = true;
                            }
                        }
                        else
                        {
                            if (RallyZone)
                            {
                                // Check if remaining rallied objects are still inside Rally Zone bounds.

                                foreach (var obj in rallyObjects.Where(o => objectsRallied.Contains(o)))
                                {
                                    if (obj && !RallyZone.bounds.Contains(obj.transform.position))
                                    {
                                        if (objectsRallied.Remove(obj))
                                        {
                                            Debug.Log(this.name + " [" + obj.name + " Exited Rally Zone]");
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Debug.LogWarning(this.name + " [No Rally Zone Defined]");
                            }
                        }
                    }
                    break;
                case State.returning:
                    {
                        if (rallyObjects.Length > 0 && rallyObjects.All(obj => objectsRallied.Contains(obj)))
                        {
                            Returned();
                        }
                        else
                        {
                            if (RallyZone)
                            {
                                // Check if remaining unrallied objects are inside Rally Zone bounds.

                                foreach (var obj in rallyObjects.Where(o => !objectsRallied.Contains(o)))
                                {
                                    if (obj && RallyZone.bounds.Contains(obj.transform.position))
                                    {
                                        if (!objectsRallied.Contains(obj))
                                        {
                                            Debug.Log(this.name + " [" + obj.name + " Returned Rally Zone]");
                                            objectsRallied.Add(obj);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Debug.LogWarning(this.name + " [No Rally Zone Defined]");
                            }
                        }
                    }
                    break;
            }
        }



        public void Initialize()
        {
            currentState = State.ready;
            rallyObjects = new GameObject[0];
            objectsRallied = new List<GameObject>();
            markerRenderers = new Dictionary<Renderer, bool>();

            if (!RallyZone)
            {
                RallyZone = GetComponent<Collider>();
            }

            if (Marker)
            {
                markerAnimator = Marker.GetComponent<Animator>();

                foreach (Renderer r in Marker.GetComponentsInChildren<Renderer>())
                {
                    markerRenderers.Add(r, r.enabled);
                }
            }

            initialized = true;
        }

        public void Rally()
        {
            ChangeState(State.rallying);
            Debug.Log(this.name + " [Rally Started]");
            FadeIn();
        }

        private void Rallied()
        {
            if (ChangeState(State.rallied))
            {
                Debug.Log(this.name + " [All Objects Rallied]");
                StartCoroutine(OnRalliedDelayDone());
                FadeOut();
            }
            else
            {
                Debug.LogWarning(this.name + " [Already Rallied]");
            }
        }

        private void Returned()
        {
            if (ChangeState(State.returned))
            {
                Debug.Log(this.name + " [All Objects Returned]");
                OnReturned.Invoke();
            }
        }

        public void Hide()
        {
            if (!initialized)
                Initialize();

            if (markerAnimator)
            {
                markerAnimator.enabled = false;
            }

            foreach (var r in markerRenderers)
            {
                if (r.Value)
                {
                    Debug.Log(this.name + " [Hiding Renderer] " + r.Key.name);
                    r.Key.enabled = false;
                }
            }
        }

        public void Show()
        {
            if (!initialized)
                Initialize();

            if (markerAnimator)
            {
                markerAnimator.enabled = true;
            }

            foreach (var r in markerRenderers)
            {
                if (r.Value)
                {
                    Debug.Log(this.name + " [Showing Renderer] " + r.Key.name);
                    r.Key.enabled = true;
                }
            }
        }

        //TODO: Set up Fading to look nice
        private void FadeIn()
        {
            if (!initialized)
                Initialize();

            if (markerAnimator)
            {
                markerAnimator.enabled = true;
            }

            foreach (var r in markerRenderers)
            {
                if (r.Value)
                {
                    Debug.Log(this.name + " [Showing Renderer] " + r.Key.name);
                    r.Key.enabled = true;
                }
            }
        }

        private void FadeOut()
        {
            if (!initialized)
                return;

            if (markerAnimator)
            {
                markerAnimator.enabled = false;
            }

            foreach (var r in markerRenderers)
            {
                if (r.Value)
                {
                    if (currentState == State.rallied && DirectionIndicator.Contains(r.Key))
                    {
                        continue;
                    }

                    Debug.Log(this.name + " [Hiding Renderer] " + r.Key.name);
                    r.Key.enabled = false;
                }
            }
        }

        //private IEnumerator FadingIn()
        //{

        //}

        //private IEnumerator FadingOut()
        //{

        //}

        private IEnumerator OnRalliedDelayDone()
        {
            yield return new WaitForSeconds(DelayAfterRallied);
            Debug.Log(this.name + " [Rally Delay Done]");

            OnRallied.Invoke();
            Hide();
        }

        private IEnumerator UpdateCurrentPlayers()
        {
            while(true)
            {
                rallyObjects = GameObject.FindGameObjectsWithTag(RallyTag); //FindObjectsOfType<GameObject>().Where(obj => obj.tag == RallyTag).ToArray();
                yield return new WaitForSeconds(0.5f);
            }
        }

        private IEnumerator RallyingUpdate()
        {
            if (currentState == State.rallying)
            {
                yield return null;
            }
            else
            {
                currentState = State.rallying;
            }

            //Debug.Log(this.name + " [Starting Rally Update Coroutine]");

            while (currentState == State.rallying)
            {
                if (RallyZone)
                {
                    // Check if remaining unrallied objects are inside Rally Zone bounds.

                    foreach (var obj in rallyObjects.Where(o => !objectsRallied.Contains(o)))
                    {
                        if (obj && RallyZone.bounds.Contains(obj.transform.position))
                        {
                            if (!objectsRallied.Contains(obj))
                            {
                                Debug.Log(this.name + " [" + obj.name + " Reached Rally Zone]");
                                objectsRallied.Add(obj);
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogWarning(this.name + " [No Rally Zone Defined]");
                    continue;
                }


                yield return new WaitForSeconds(0.1f);
            }

            //Debug.Log(this.name + " [Exiting Rally Update Coroutine]");
        }

        private bool ChangeState(State state)
        {
            if (currentState != state)
            {
                // this is boilerplate for state change events we might want later
                var lastState = currentState;
                currentState = state;

                Debug.Log(this.name + " [State Changed] " + lastState + " -> " + currentState);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}