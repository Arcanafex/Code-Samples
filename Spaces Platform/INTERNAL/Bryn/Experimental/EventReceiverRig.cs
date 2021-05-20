using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;

public class EventReceiverRig : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler,

    ISelectHandler,
    IUpdateSelectedHandler,
    IDeselectHandler,

    IInitializePotentialDragHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IDropHandler,

    IScrollHandler,
    IMoveHandler
{
    public bool PointerEnterReceived;
    public bool PointerExitReceived;
    public bool PointerDownReceived;
    public bool PointerUpReceived;
    [Space]
    public bool SelectReceived;
    public bool UpdateSelectedReceived;
    public bool DeselectReceived;
    [Space]
    public bool BeforeBeginDragReceived;
    public bool BeginDragReceived;
    public bool DragReceived;
    public bool EndDragReceived;
    public bool DropReceived;
    [Space]
    public bool ScrollReceived;
    public bool MoveReceived;

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log(this.name + " [On Pointer Enter]");
        PointerEnterReceived = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log(this.name + " [On Pointer Exit]");
        PointerExitReceived = true;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log(this.name + " [On Pointer Down]");
        PointerDownReceived = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log(this.name + " [On Pointer Up]");
        PointerUpReceived = true;
    }

    public void OnSelect(BaseEventData eventData)
    {
        Debug.Log(this.name + " [On Select]");
        SelectReceived = true;
    }

    public void OnUpdateSelected(BaseEventData eventData)
    {
        Debug.Log(this.name + " [On Update Selected]");
        UpdateSelectedReceived = true;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        Debug.Log(this.name + " [On Deselect]");
        DeselectReceived = true;
    }

    public void OnInitializePotentialDrag(PointerEventData eventData)
    {
        Debug.Log(this.name + " [On Potential Drag]");
        BeforeBeginDragReceived = true;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log(this.name + " [On Begin Drag]");
        BeginDragReceived = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log(this.name + " [On Drag]");
        DragReceived = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log(this.name + " [On End Drag]");
        EndDragReceived = true;
    }

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log(this.name + " [On Drop]");
        DropReceived = true;
    }

    public void OnScroll(PointerEventData eventData)
    {
        Debug.Log(this.name + " [On Scroll]");
        ScrollReceived = true;
    }

    public void OnMove(AxisEventData eventData)
    {
        Debug.Log(this.name + " [On Move] " + eventData.moveVector.ToString() + " (" + eventData.moveDir.ToString() + ")");
        MoveReceived = true;
    }
}
