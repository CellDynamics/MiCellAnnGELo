using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class GrabCellActions : MonoBehaviour
{
    public SteamVR_Action_Boolean grab;
    public SteamVR_Input_Sources hand;
    public GameObject otherController;
    public Transform cell, pointerSphere;
    private bool scaling = false;
    private float originalDist;
    private Vector3 cellOriginalScale, pointerOriginalScale, originalPos;

    // Update is called once per frame
    private void Update()
    {
        if (grab.GetLastStateDown(hand))
            Grab();

        if (grab.GetLastStateUp(hand))
            Release();

        if (scaling)
            Scale();
    }

    private void Grab()
    {
        // Check if the cell isn't currently being held
        if (cell.parent == null)
            // Attach cell to hand
            cell.SetParent(gameObject.transform, true);
        else
        {
            // Cell attached to hand so start scaling
            scaling = true;
            // Store parameters before scaling 
            cellOriginalScale = cell.localScale;
            pointerOriginalScale = pointerSphere.localScale;
            originalDist = Vector3.Distance(gameObject.transform.position, otherController.transform.position);
            originalPos = cell.localPosition;
        }
    }

    private void Release()
    {
        // Check if this hand is attached to the cell
        if (cell.parent == gameObject.transform)
        {
            // Set cell parent to world space
            cell.SetParent(null, true);
            // Stop other controller from scaling
            otherController.GetComponent<GrabCellActions>().StopScaling();
        }
        else
            StopScaling();
    }

    private void Scale()
    {
        // Calculate distance between controllers
        float newDist = Vector3.Distance(gameObject.transform.position, otherController.transform.position);
        // Calculate multiple of original distance
        float scaleChange = newDist / originalDist;
        // Scale all objects
        pointerSphere.localScale = pointerOriginalScale * scaleChange;
        cell.localScale = cellOriginalScale * scaleChange;
        cell.localPosition = originalPos * scaleChange;
    }

    public void StopScaling()
    {
        scaling = false;
    }
}
