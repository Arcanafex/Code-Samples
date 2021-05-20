using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class SpaceCasterEventData : BaseEventData
{
    public enum FramePressState
    {
        Pressed,
        Released,
        PressedAndReleased,
        NotChanged
    }

    // The object that received OnPointerEnter
    public GameObject pointerEnter { get; set; }

    // The object that received OnPointerDown
    private GameObject m_PointerPress;

    // The object last received OnPointerDown
    public GameObject lastPress { get; private set; }

    // The object that the press happened on even if it can not handle the press event
    public GameObject rawPointerPress { get; set; }

    // The object that received OnDrag
    public GameObject pointerDrag { get; set; }

    public RaycastResult pointerCurrentRaycast { get; set; }
    public RaycastResult pointerPressRaycast { get; set; }

    // Stack of GameObjects intersected by the raycast
    public List<GameObject> hovered = new List<GameObject>();

    public bool eligibleForClick { get; set; }

    public int casterId { get; set; }

    // Current position of the beamPoint & Delta of beamPoint since last update
    public Vector3 positionPoint { get; set; }
    public Vector3 deltaPoint { get; set; }

    // Ray that was cast by the caster
    public Ray ray { get; set; }


    public Vector3 positionCaster { get { return ray.origin; } }
    public Vector3 deltaCaster { get; private set; }

    // Position of the press event
    public Vector3 pressPosition { get; set; }

    // The last time a click event was sent out (used for double-clicks)
    public float clickTime { get; set; }

    // Number of clicks in a row. 2 for a double-click for example.
    public int clickCount { get; set; }

    public Vector2 scrollDelta { get; set; }
    public bool useDragThreshold { get; set; }
    public bool dragging { get; set; }

    public SpaceCasterEventData(EventSystem eventSystem) : base(eventSystem)
    {
        eligibleForClick = false;

        casterId = -1;
        positionPoint = Vector3.zero;
        deltaPoint = Vector3.zero;
        pressPosition = Vector3.zero;
        clickTime = 0.0f;
        clickCount = 0;

        scrollDelta = Vector2.zero;
        useDragThreshold = true;
        dragging = false;
    }

    public bool IsPointMoving()
    {
        return deltaPoint.sqrMagnitude > 0.0f;
    }

    public bool IsScrolling()
    {
        return scrollDelta.sqrMagnitude > 0.0f;
    }

    public GameObject pointerPress
    {
        get { return m_PointerPress; }
        set
        {
            if (m_PointerPress == value)
                return;

            lastPress = m_PointerPress;
            m_PointerPress = value;
        }
    }

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("<b>Position</b>: " + positionPoint);
        sb.AppendLine("<b>delta</b>: " + deltaPoint);
        sb.AppendLine("<b>ray</b>: " + ray.ToString());
        sb.AppendLine("<b>eligibleForClick</b>: " + eligibleForClick);
        sb.AppendLine("<b>pointerEnter</b>: " + pointerEnter);
        sb.AppendLine("<b>pointerPress</b>: " + pointerPress);
        sb.AppendLine("<b>lastPointerPress</b>: " + lastPress);
        sb.AppendLine("<b>pointerDrag</b>: " + pointerDrag);
        sb.AppendLine("<b>Use Drag Threshold</b>: " + useDragThreshold);
        sb.AppendLine("<b>Current Rayast:</b>");
        sb.AppendLine(pointerCurrentRaycast.ToString());
        sb.AppendLine("<b>Press Rayast:</b>");
        sb.AppendLine(pointerPressRaycast.ToString());
        return sb.ToString();
    }
}
