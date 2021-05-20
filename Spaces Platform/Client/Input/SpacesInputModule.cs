using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Spaces.UnityClient
{
    [AddComponentMenu("Event/Spaces Input Module")]
    public class SpacesInputModule : BaseInputModule
    {
        public string button0, button1, button2;
        public float doubleClickTimelimit;
        public float scrollThreshold;
        public float triggerThreshold;
        public float moveDeadZone;

        //private Dictionary<SteamVR_Controller.DeviceRelation, int> DeviceRelations = new Dictionary<SteamVR_Controller.DeviceRelation, int>();

        #if UNITY_STANDALONE_WIN
        private Dictionary<SteamVR_Controller.DeviceRelation, int> DeviceRelations = new Dictionary<SteamVR_Controller.DeviceRelation, int>();

        #elif UNITY_ANDROID || UNITY_IOS
        public enum DeviceRelationsEmulated
        {
            First,
            // radially
            Leftmost,
            Rightmost,
            // distance - also see vr.hmd.GetSortedTrackedDeviceIndicesOfClass
            FarthestLeft,
            FarthestRight,
        }

        private Dictionary<DeviceRelationsEmulated, int> DeviceRelations = new Dictionary<DeviceRelationsEmulated, int>();

        #endif

        protected internal static class SpaceCasterManager
        {
            private static readonly List<SpaceCaster> s_Raycasters = new List<SpaceCaster>();

            public static void AddRaycaster(SpaceCaster spaceCaster)
            {
                if (!s_Raycasters.Contains(spaceCaster))
                    s_Raycasters.Add(spaceCaster);
            }

            public static List<SpaceCaster> GetRaycasters()
            {
                return s_Raycasters;
            }

            public static void RemoveRaycasters(SpaceCaster baseRaycaster)
            {
                if (!s_Raycasters.Contains(baseRaycaster))
                    return;
                s_Raycasters.Remove(baseRaycaster);
            }
        }

        protected internal class InputDeviceState
        {
            public enum PressState
            {
                Pressed,
                Released,
                PressedAndReleased,
                NotChanged
            }

            public float lastUpdate = 0;
            public int currentId = -1;

            #if UNITY_STANDALONE_WIN
            public SteamVR_Controller.DeviceRelation position;

            #elif UNITY_ANDROID || UNITY_IOS
            //Emulate SteamVR_Controller.DeviceRelation
            public DeviceRelationsEmulated position;

            #endif

            public bool buttonTrigger = false;
            public bool buttonTriggerDown = false;
            public bool buttonTriggerUp = false;
            //public PressState buttonTriggerDelta = PressState.NotChanged;

            public bool buttonMenu = false;
            public bool buttonMenuDown = false;
            public bool buttonMenuUp = false;
            //public PressState buttonMenuDelta = PressState.NotChanged;

            public bool buttonGrip = false;
            public bool buttonGripDown = false;
            public bool buttonGripUp = false;
            //public PressState buttonGripDelta = PressState.NotChanged;

            public bool buttonTouchpad = false;
            public bool buttonTouchpadDown = false;
            public bool buttonTouchpadUp = false;
            //public PressState buttonTouchpadDelta = PressState.NotChanged;

            public bool touchTouchpad = false;
            public bool touchTouchpadDown = false;
            public bool touchTouchpadUp = false;
            //public PressState touchTouchpadDelta = PressState.NotChanged;

            public Vector2 axisTouchpad = Vector2.zero;
            public Vector2 axisTouchpadDelta = Vector2.zero;

            public float axisTrigger = 0;
            public float axisTriggetDelta = 0;

            public InputDeviceState lastState;
        }

        protected internal class CasterData
        {
            public PointerEventData eventData;

            #if UNITY_STANDALONE_WIN
            public SteamVR_TrackedObject trackedObject;

            #elif UNITY_ANDROID || UNITY_IOS
            public GameObject trackedObject;

            #endif

            public InputDeviceState inputState;
            public Vector3 lastPointPosition;
        }

        public enum InputState
        {
            Default,
            Menu,
            ObjectSelected,
            ObjectEditing,
            Teleport
        }

        public delegate void InputStateChange(InputState lastState, InputState nextState);
        public event InputStateChange OnInputStateChange;

        protected void UpdateState(InputState state)
        {
            if (currentState != state)
            {
                lastState = currentState;
                currentState = state;

                if (OnInputStateChange != null)
                    OnInputStateChange(lastState, currentState);

            }
        }

        protected SpacesInputModule()
        { }

        protected Dictionary<SpaceCaster, CasterData> m_CasterData = new Dictionary<SpaceCaster, CasterData>();

        protected GameObject m_selected;
        protected GameObject m_inGaze;
        protected Canvas m_currentGazeCanvas;
        protected InputState currentState;
        protected InputState lastState;

        #region Unity Lifetime calls
        protected override void OnEnable()
        {
            base.OnEnable();
            Debug.Log("SpacesInputModule [Enabled]");
            UpdateState(InputState.Default);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            Debug.Log("SpacesInputModule [Disabled]");
            ClearSelection();
        }
        #endregion

        // Returns true if event data was created
        protected bool GetSpaceCasterData(SpaceCaster caster, out PointerEventData data, bool create)
        {
            if (!m_CasterData.ContainsKey(caster) && create)
            {
                //Debug.Log("SpacesInputModule [Added CasterData " + caster.id + "] " + caster.name);
                var casterData = new CasterData()
                {
                    eventData = new PointerEventData(eventSystem) { pointerId = caster.id },

                    #if UNITY_STANDALONE_WIN
                    trackedObject = caster.GetComponentInParent<SteamVR_TrackedObject>(),
                    
                    #elif UNITY_ANDROID || UNITY_IOS
                    trackedObject = caster.GetComponentInParent<Transform>().gameObject,
                
                    #endif

                    inputState = new InputDeviceState(),
                    lastPointPosition = caster.Forward
                };

                if (!casterData.trackedObject)
                {
                    #if UNITY_STANDALONE_WIN
                    casterData.trackedObject = caster.gameObject.AddComponent<SteamVR_TrackedObject>();

                    #endif
                }

                m_CasterData.Add(caster, casterData);
                data = casterData.eventData;
                return true;
            }
            else
            {
                data = m_CasterData.ContainsKey(caster) ? m_CasterData[caster].eventData : null;
                return false;
            }
        }

        protected void RemoveSpaceCasterData(PointerEventData data)
        {
            Debug.Log("SpacesInputModule [Removed CasterData " + data.pointerId + "]");

            m_CasterData.Remove(SpaceCasterManager.GetRaycasters().First(s => s.id == data.pointerId));
        }

        protected PointerEventData GetSpaceCasterEventData(SpaceCaster caster)
        {
            PointerEventData casterData;

            bool created = GetSpaceCasterData(caster, out casterData, true);
            casterData.Reset();

            if (created)
            {
                casterData.position = caster.eventCamera.ViewportToScreenPoint(Vector2.one * 0.5f);
                UpdateDeviceRelations();
            }

            return casterData;
        }

        protected void UpdateDeviceRelations()
        {
            if (DeviceRelations == null)
                #if UNITY_STANDALONE_WIN
                DeviceRelations = new Dictionary<SteamVR_Controller.DeviceRelation, int>();
                
                #elif UNITY_ANDROID || UNITY_IOS
                DeviceRelations = new Dictionary<DeviceRelationsEmulated, int>();
                
                #endif
            else
                DeviceRelations.Clear();

            //foreach (var relation in new SteamVR_Controller.DeviceRelation[] { SteamVR_Controller.DeviceRelation.First, SteamVR_Controller.DeviceRelation.Leftmost, SteamVR_Controller.DeviceRelation.Rightmost })
            //    DeviceRelations.Add(relation, SteamVR_Controller.GetDeviceIndex(relation));
            #if UNITY_STANDALONE_WIN
                foreach (var relation in new SteamVR_Controller.DeviceRelation[] { SteamVR_Controller.DeviceRelation.First, SteamVR_Controller.DeviceRelation.Leftmost, SteamVR_Controller.DeviceRelation.Rightmost })
                    DeviceRelations.Add(relation, SteamVR_Controller.GetDeviceIndex(relation));
            
            #elif UNITY_ANDROID || UNITY_IOS
                //For right now, IOS/Android head ray cast becomes "First" and index 0
                DeviceRelations.Add(DeviceRelationsEmulated.First, 0);
            
            #endif
        }

        // Hard coded to Vive for now. Needs to be abstracted later.

        protected bool GetDeviceState(SpaceCaster caster, out InputDeviceState deviceInputState)
        {
            if (m_CasterData.ContainsKey(caster))
            {

            #if UNITY_STANDALONE_WIN
                int index = m_CasterData[caster].trackedObject ? (int)m_CasterData[caster].trackedObject.index : 0;

                var device = SteamVR_Controller.Input(index);
                deviceInputState = m_CasterData[caster].inputState;
                deviceInputState.lastState = m_CasterData[caster].inputState;

                deviceInputState.lastUpdate = Time.time;
                deviceInputState.currentId = (int)device.index;

                // If trigger is responsive enough with GetPress, we can try this.
                deviceInputState.buttonTrigger = device.GetHairTrigger();
                deviceInputState.buttonTriggerDown = device.GetHairTriggerDown();
                deviceInputState.buttonTriggerUp = device.GetHairTriggerUp();

                //deviceInputState.buttonTriggerDelta =
                //    device.GetHairTriggerDown() ? InputDeviceState.PressState.Pressed
                //    : device.GetHairTriggerUp() ? InputDeviceState.PressState.Released
                //    : InputDeviceState.PressState.NotChanged;

                //deviceInputState.buttonTrigger = device.GetPress(SteamVR_Controller.ButtonMask.Trigger);
                //deviceInputState.buttonTriggerDelta =
                //    device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger) ? InputDeviceState.PressState.Pressed
                //    : device.GetPressUp(SteamVR_Controller.ButtonMask.Trigger) ? InputDeviceState.PressState.Released
                //    : InputDeviceState.PressState.NotChanged;

                deviceInputState.buttonMenu = device.GetPress(SteamVR_Controller.ButtonMask.ApplicationMenu);
                deviceInputState.buttonMenuDown = device.GetPressDown(SteamVR_Controller.ButtonMask.ApplicationMenu);
                deviceInputState.buttonMenuUp = device.GetPressUp(SteamVR_Controller.ButtonMask.ApplicationMenu);                //deviceInputState.buttonMenuDelta =
                //    device.GetPressDown(SteamVR_Controller.ButtonMask.ApplicationMenu) ? InputDeviceState.PressState.Pressed
                //    : device.GetPressUp(SteamVR_Controller.ButtonMask.ApplicationMenu) ? InputDeviceState.PressState.Released
                //    : InputDeviceState.PressState.NotChanged;

                deviceInputState.buttonGrip = device.GetPress(SteamVR_Controller.ButtonMask.Grip);
                deviceInputState.buttonGripDown = device.GetPressDown(SteamVR_Controller.ButtonMask.Grip);
                deviceInputState.buttonGripUp = device.GetPressUp(SteamVR_Controller.ButtonMask.Grip);                //deviceInputState.buttonGripDelta =
                //    device.GetPressDown(SteamVR_Controller.ButtonMask.Grip) ? InputDeviceState.PressState.Pressed
                //    : device.GetPressUp(SteamVR_Controller.ButtonMask.Grip) ? InputDeviceState.PressState.Released
                //    : InputDeviceState.PressState.NotChanged;

                deviceInputState.buttonTouchpad = device.GetPress(SteamVR_Controller.ButtonMask.Touchpad);
                deviceInputState.buttonTouchpadDown = device.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad);
                deviceInputState.buttonTouchpadUp = device.GetPressUp(SteamVR_Controller.ButtonMask.Touchpad);
                //deviceInputState.buttonTouchpadDelta =
                //    device.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad) ? InputDeviceState.PressState.Pressed
                //    : device.GetPressUp(SteamVR_Controller.ButtonMask.Touchpad) ? InputDeviceState.PressState.Released
                //    : InputDeviceState.PressState.NotChanged;

                deviceInputState.touchTouchpad = device.GetTouch(SteamVR_Controller.ButtonMask.Touchpad);
                deviceInputState.touchTouchpadDown = device.GetTouchDown(SteamVR_Controller.ButtonMask.Touchpad);
                deviceInputState.touchTouchpadUp = device.GetTouchUp(SteamVR_Controller.ButtonMask.Touchpad);
                //deviceInputState.touchTouchpadDelta =
                //    device.GetTouchDown(SteamVR_Controller.ButtonMask.Touchpad) ? InputDeviceState.PressState.Pressed
                //    : device.GetTouchUp(SteamVR_Controller.ButtonMask.Touchpad) ? InputDeviceState.PressState.Released
                //    : InputDeviceState.PressState.NotChanged;

                deviceInputState.axisTouchpadDelta = device.GetAxis() - deviceInputState.axisTouchpad;
                deviceInputState.axisTouchpad = device.GetAxis();

                deviceInputState.axisTriggetDelta = device.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger).x - deviceInputState.axisTrigger;
                deviceInputState.axisTrigger = device.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger).x;

                return device.valid;

#elif UNITY_ANDROID || UNITY_IOS
                deviceInputState = new InputDeviceState();
                return true;

#endif
            }
            else
            {
                deviceInputState = new InputDeviceState();
                return false;
            }

        }

        protected void CopyFromTo(PointerEventData @from, PointerEventData @to)
        {
            @to.position = @from.position;
            @to.delta = @from.delta;
            @to.scrollDelta = @from.scrollDelta;
            @to.pointerCurrentRaycast = @from.pointerCurrentRaycast;
            @to.pointerEnter = @from.pointerEnter;
        }


        public override void Process()
        {
            List<RaycastResult> raycastResults = new List<RaycastResult>();

            foreach (var caster in SpaceCasterManager.GetRaycasters().Where(c => c != null && c.IsActive()))
            {
                //Debug.Log("SpacesInputModule [Process] (" + caster.id + ") " + caster.name);

                // Get out pointer event data and do raycast for this caster.
                var eventData = GetSpaceCasterEventData(caster);
                caster.Raycast(eventData, raycastResults);

                eventData.pointerCurrentRaycast = FindFirstRaycast(raycastResults);
                ProcessMove(eventData);

                //Set delta based on previous worldspace position of a hit or caster.Forward;
                var refPos = eventData.pointerCurrentRaycast.isValid ? eventData.pointerCurrentRaycast.worldPosition : caster.Forward;

                eventData.delta = caster.eventCamera.WorldToScreenPoint(refPos - m_CasterData[caster].lastPointPosition);
                m_CasterData[caster].lastPointPosition = refPos;

                //If event is tracking an object that received a press, update it's position relative to the motion of the caster.
                if (eventData.pointerPress)
                    eventData.pressPosition = caster.eventCamera.WorldToScreenPoint(eventData.pointerPress.transform.position);

                if (caster.CasterType == SpaceCaster.Type.Gaze)
                    ProcessGaze(eventData);

                InputDeviceState inputState;

                if (GetDeviceState(caster, out inputState) || caster.CasterType == SpaceCaster.Type.Gaze)
                {
                    // Trigger State Handling

                    if (inputState.buttonTriggerDown || Input.GetButtonDown(button0)) //inputState.axisTriggetDelta > triggerThreshold)
                    {
                        // trigger pressed
                        //Debug.Log("SpacesInputModule [Trigger Down] (" + caster.id + ") " + caster.name);

                        eventData.clickTime = Time.time;

                        eventData.eligibleForClick = true;
                        eventData.delta = Vector2.zero;
                        eventData.dragging = false;
                        eventData.useDragThreshold = true;

                        eventData.pressPosition = caster.eventCamera.ViewportToScreenPoint(Vector2.one * 0.5f);
                        eventData.pointerPressRaycast = eventData.pointerCurrentRaycast;

                        // Handle press
                        var newPressed = ExecuteEvents.ExecuteHierarchy(eventData.pointerPressRaycast.gameObject, eventData, ExecuteEvents.pointerDownHandler);

                        eventData.pointerPress = newPressed;
                        eventData.rawPointerPress = eventData.pointerPressRaycast.gameObject;

                        ProcessSelection(eventData);

                        // Handle potential drag
                        eventData.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(eventData.pointerPressRaycast.gameObject);
                        eventData.button = PointerEventData.InputButton.Left;

                        if (eventData.pointerDrag != null)
                            ExecuteEvents.Execute(eventData.pointerDrag, eventData, ExecuteEvents.initializePotentialDrag);
                    }
                    else if (inputState.buttonTriggerUp || Input.GetButtonUp(button0)) //|| inputState.axisTriggetDelta < -triggerThreshold)
                    {
                        // trigger released
                        //Debug.Log("SpacesInputModule [Trigger Up] (" + caster.id + ") " + caster.name);

                        // Handle Pointer Up
                        GameObject upHandler = ExecuteEvents.GetEventHandler<IPointerUpHandler>(eventData.pointerPressRaycast.gameObject);
                        ExecuteEvents.ExecuteHierarchy(upHandler, eventData, ExecuteEvents.pointerUpHandler);

                        if (eventData.pointerPress == eventData.lastPress)
                        {
                            // If release was less than .1 sec from press, it's a click
                            if (Time.time - eventData.clickTime < doubleClickTimelimit)
                            {
                                //Debug.Log("SpacesInputModule [Trigger Doubleclick] (" + caster.id + ") " + caster.name);
                                ++eventData.clickCount;
                            }
                            else
                                eventData.clickCount = 1;

                            eventData.clickTime = Time.time;
                        }


                        // An object is either going to get a click event, or a drop event executed.

                        // Handle Click on pointer up
                        GameObject clickHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(eventData.pointerEnter);

                        // Verify that object receiving pointerDown is the same as the one receiving pointer up
                        if (eventData.pointerPress == clickHandler && eventData.eligibleForClick)
                        {
                            //Debug.Log("SpacesInputModule [Click Handler] (" + caster.id + ") " + caster.name);
                            ExecuteEvents.ExecuteHierarchy(clickHandler, eventData, ExecuteEvents.pointerClickHandler);
                        }
                        else if (eventData.pointerDrag && eventData.dragging)
                        {
                            // Handle End Drag & Drop
                            GameObject endDragHandler = ExecuteEvents.GetEventHandler<IEndDragHandler>(eventData.pointerPressRaycast.gameObject);
                            ExecuteEvents.ExecuteHierarchy(endDragHandler, eventData, ExecuteEvents.endDragHandler);

                            GameObject dropHandler = ExecuteEvents.GetEventHandler<IDropHandler>(eventData.pointerPressRaycast.gameObject);
                            ExecuteEvents.ExecuteHierarchy(dropHandler, eventData, ExecuteEvents.dropHandler);
                        }

                    }
                    else if (inputState.buttonTrigger || Input.GetButton(button0)) //|| m_CasterData[caster].inputState.axisTrigger > 0.5f)
                    {
                        // trigger still down
                        //Debug.Log("SpacesInputModule [Trigger Held] (" + caster.id + ") " + caster.name);

                        ////set event delta for press
                        //var pressPos = caster.eventCamera.WorldToScreenPoint(eventData.pointerPress.transform.position);
                        //eventData.delta = caster.eventCamera.ViewportToScreenPoint(Vector2.one * 0.5f);

                        ProcessDrag(eventData);
                    }
                    else
                    {
                        // Trigger still not pressed
                        // clear everything when trigger is on up state ?
                        //eventData.dragging = false;
                        //eventData.pointerPress = null;
                        //eventData.pointerDrag = null;
                        //eventData.rawPointerPress = null;
                        //eventData.dragging = false;
                    }

                    // Menu State Handling
                    if ((inputState.buttonMenu && inputState.buttonMenuDown))
                    {
                        if (caster.CasterLocation == SpaceCaster.Location.RightHand)
                        {
                            if (!UserSession.Instance.MenuOpen)
                                UserSession.Instance.OpenNavInterface();
                            else
                                UserSession.Instance.CloseNavInterface();
                        }
                        else if (caster.CasterLocation == SpaceCaster.Location.LeftHand)
                        {
                            if (!UserSession.Instance.MenuOpen)
                                UserSession.Instance.OpenAssetInterface();
                            else
                                UserSession.Instance.CloseNavInterface();
                        }
                    }

                    // Grip State Handling
                    if ((inputState.buttonGrip && inputState.buttonGripDown))
                    {
                        if (caster.CasterLocation == SpaceCaster.Location.LeftHand)
                        {
                            if (!UserSession.Instance.MenuOpen)
                                UserSession.Instance.OpenNavInterface();
                            else
                                UserSession.Instance.CloseNavInterface();
                        }
                        else if (caster.CasterLocation == SpaceCaster.Location.RightHand)
                        {
                            if (!UserSession.Instance.MenuOpen)
                                UserSession.Instance.OpenAssetInterface();
                            else
                                UserSession.Instance.CloseNavInterface();
                        }
                    }

                }


                ProcessScroll();
                ProcessMoveInput();

                //Update selected GameObject, if any
                if (eventSystem.currentSelectedGameObject)
                {
                    ExecuteEvents.ExecuteHierarchy(eventSystem.currentSelectedGameObject, GetBaseEventData(), ExecuteEvents.updateSelectedHandler);
                }

                // Processsing complete. Set all events to used.
                m_CasterData.Values.ToList().ForEach(data => data.eventData.Use());

                // Add all raycast results to cache, then clear temp list.
                m_RaycastResultCache.AddRange(raycastResults);
                raycastResults.Clear();
            }

            ProcessKeyboadInput();
        }

        protected virtual void ProcessKeyboadInput()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (!UserSession.Instance.MenuOpen)
                    UserSession.Instance.OpenNavInterface();
                else
                    UserSession.Instance.CloseNavInterface();
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                if (!UserSession.Instance.MenuOpen)
                    UserSession.Instance.OpenAssetInterface();
                else
                    UserSession.Instance.CloseNavInterface();
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                Application.Quit();
            }
        }


        protected virtual void ProcessMove(PointerEventData pointerEvent)
        {
            var targetGO = pointerEvent.pointerCurrentRaycast.gameObject;
            HandlePointerExitAndEnter(pointerEvent, targetGO);
        }

        protected virtual void ProcessGaze(PointerEventData pointerEvent)
        {
            var gazeGO = pointerEvent.pointerCurrentRaycast.gameObject;

            if (!m_inGaze)
            {
                if (gazeGO)
                {
                    ExecuteEvents.ExecuteHierarchy<IGazeEnterHandler>(gazeGO, pointerEvent, ExecuteEventsExtensions.gazeEnterHandler);
                }
            }
            else
            {
                if (m_inGaze == gazeGO)
                {
                    ExecuteEvents.ExecuteHierarchy<IGazeStayHandler>(gazeGO, pointerEvent, ExecuteEventsExtensions.gazeStayHandler);
                }
                else
                {
                    ExecuteEvents.ExecuteHierarchy<IGazeExitHandler>(m_inGaze, pointerEvent, ExecuteEventsExtensions.gazeExitHandler);

                    if (gazeGO)
                        ExecuteEvents.ExecuteHierarchy<IGazeEnterHandler>(gazeGO, pointerEvent, ExecuteEventsExtensions.gazeEnterHandler);
                }

            }

            m_inGaze = gazeGO;
        }

        protected virtual void ProcessScroll()
        {
            // Scrolling only possible when looking at the handler
            if (m_inGaze)
            {
                var scrollHandler = ExecuteEvents.GetEventHandler<IScrollHandler>(m_inGaze);

                if (scrollHandler)
                    foreach (var casterData in m_CasterData.Values)
                    {
                        if (casterData.inputState.touchTouchpad && casterData.inputState.axisTouchpadDelta.sqrMagnitude > (scrollThreshold * scrollThreshold))
                        {
                            casterData.eventData.scrollDelta = casterData.inputState.axisTouchpadDelta;
                            ExecuteEvents.ExecuteHierarchy(m_inGaze, casterData.eventData, ExecuteEvents.scrollHandler);
                        }
                    }


            }
        }

        protected virtual void ProcessMoveInput()
        {
            var moveHandler = ExecuteEvents.GetEventHandler<IMoveHandler>(eventSystem.currentSelectedGameObject);

            if (moveHandler)
            {
                //Debug.Log("SpacesInputModule [Move Possible]");

                foreach (var casterData in m_CasterData.Values)
                {
                    //Debug.Log("SpacesInputModule [Move Input] (" + casterData.eventData.pointerId + ")");

                    if (casterData.inputState.buttonTouchpad)
                    {
                        //Debug.Log("SpacesInputModule [Touchpad Pressed] (" + casterData.inputState.buttonTouchpad + ") " + casterData.inputState.axisTouchpad);

                        ExecuteEvents.ExecuteHierarchy(moveHandler, GetAxisEventData(casterData.inputState.axisTouchpad.x, casterData.inputState.axisTouchpad.y, moveDeadZone), ExecuteEvents.moveHandler);
                    }
                }
            }
        }

        protected virtual void ProcessSelection(PointerEventData pointerEvent)
        {
            GameObject selectHandler = ExecuteEvents.GetEventHandler<ISelectHandler>(pointerEvent.pointerPress);

            if (eventSystem.currentSelectedGameObject != selectHandler)
            {
                eventSystem.SetSelectedGameObject(selectHandler, pointerEvent);
            }
            else
            {
                eventSystem.SetSelectedGameObject(null, pointerEvent);
            }

        }

        private static bool ShouldStartDrag(Vector2 pressPos, Vector2 currentPos, float threshold, bool useDragThreshold)
        {
            //Debug.Log("SpacesInputModule [Should start drag?] " + !useDragThreshold);

            if (!useDragThreshold)
                return true;

            return (pressPos - currentPos).sqrMagnitude >= threshold * threshold;
        }

        protected virtual void ProcessDrag(PointerEventData pointerEvent)
        {
            bool moving = pointerEvent.IsPointerMoving();
            //Debug.Log("SpacesInputModule [Process Drag] drag: " + pointerEvent.pointerDrag.name + " dragging: " + pointerEvent.dragging + " moving: " + moving);

            if (moving && pointerEvent.pointerDrag != null
                && !pointerEvent.dragging
                && ShouldStartDrag(pointerEvent.pressPosition, pointerEvent.position, eventSystem.pixelDragThreshold, pointerEvent.useDragThreshold))
            {
                //Debug.Log("SpacesInputModule [Drag]");

                ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.beginDragHandler);
                pointerEvent.dragging = true;
            }

            // Drag notification
            if (pointerEvent.dragging && moving && pointerEvent.pointerDrag != null)
            {
                // Before doing drag we should cancel any pointer down state
                // And clear selection!
                if (pointerEvent.pointerPress != pointerEvent.pointerDrag)
                {
                    ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);

                    pointerEvent.eligibleForClick = false;
                    pointerEvent.pointerPress = null;
                    pointerEvent.rawPointerPress = null;
                }

                ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.dragHandler);
            }
        }

        protected void ClearSelection()
        {
            var baseEventData = GetBaseEventData();

            foreach (var casterData in m_CasterData.Values)
            {
                // clear all selection
                HandlePointerExitAndEnter(casterData.eventData, null);
            }

            m_CasterData.Clear();

            if (eventSystem && eventSystem.currentSelectedGameObject)
                eventSystem.SetSelectedGameObject(null, baseEventData);
        }

    }
}

        /*
         * 
State
Default
    Menu: open Nav
    Grip: open Asset
    Trigger: Activate/Select
    Thumbpad: scroll focused UI
    Thumbpad click: Teleport mode
Game Object selected
    Menu: edit settings
    Grip:
    Trigger: place object
    Thumbpad: rotate
Game Object Editing
Teleport
    Menu:
    Grip: Straighten pointer
    Trigger: teleport
    Thumbpad: Adjust parabola


Commands:
    OpenNav(sender)
    OpenAssets(sender)
    SelectObject(sender, target)
    ClickInteractible(sender, target)
    Scroll(sender, target)
    EnterTeleportMode(sender)
    PlaceObject(sender, object)
    OpenEditMenu(sender, target)
    RotateObject(sender, target, delta)
    ToggleTeleportPointer(sender, pointer, isParabolic)
    Teleport(sender, destination, avatar)
    AdjustParabola(sender, pointer, delta)         
*/

