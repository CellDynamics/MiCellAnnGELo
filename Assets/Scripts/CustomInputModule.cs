using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Valve.VR;

public class CustomInputModule : BaseInputModule
{
    public Camera eventCamera;
    public SteamVR_Input_Sources hand;
    public SteamVR_Action_Boolean clickAction;

    private GameObject currentObject = null;
    private PointerEventData data = null;
    private bool dragging = false;

    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();

        data = new PointerEventData(eventSystem);
    }

    public override void Process()
    {
        data.Reset();
        data.position = new Vector2(eventCamera.pixelWidth / 2, eventCamera.pixelHeight / 2);

        if (!dragging)
        {
            eventSystem.RaycastAll(data, m_RaycastResultCache);
            data.pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);
            currentObject = data.pointerCurrentRaycast.gameObject;

            m_RaycastResultCache.Clear();

            HandlePointerExitAndEnter(data, currentObject);

            if (clickAction.GetStateDown(hand) || Input.GetMouseButtonDown(0))
                ProcessPress(data);
        }
        else
        {
            if (clickAction.GetState(hand) || Input.GetMouseButton(0))
                ProcessDrag(data);
        }

        if (clickAction.GetStateUp(hand) || Input.GetMouseButtonUp(0))
        {
            ProcessRelease(data);
            dragging = false;
        }
    }

    public PointerEventData GetData()
    {
        return data;
    }

    private void ProcessPress(PointerEventData pointerData)
    {
        pointerData.pointerPressRaycast = pointerData.pointerCurrentRaycast;

        GameObject newPointerPress = ExecuteEvents.ExecuteHierarchy(currentObject, pointerData, ExecuteEvents.pointerDownHandler);

        if (newPointerPress == null)
            newPointerPress = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentObject);

        pointerData.pressPosition = pointerData.position;
        pointerData.pointerPress = newPointerPress;
        pointerData.rawPointerPress = currentObject;

        if (newPointerPress != null && newPointerPress.GetComponent<Slider>())
            dragging = true;
    }

    private void ProcessDrag(PointerEventData pointerData)
    {
        ExecuteEvents.ExecuteHierarchy(currentObject, pointerData, ExecuteEvents.dragHandler);
    }

    private void ProcessRelease(PointerEventData pointerData)
    {
        ExecuteEvents.Execute(pointerData.pointerPress, pointerData, ExecuteEvents.pointerUpHandler);

        GameObject pointerUpHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentObject);

        if (pointerData.pointerPress == pointerUpHandler)
            ExecuteEvents.Execute(pointerData.pointerPress, pointerData, ExecuteEvents.pointerClickHandler);

        eventSystem.SetSelectedGameObject(null);

        pointerData.pressPosition = Vector2.zero;
        pointerData.pointerPress = null;
        pointerData.rawPointerPress = null;
    }
}
