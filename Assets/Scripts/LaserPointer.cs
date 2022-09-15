using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Valve.VR;

public class LaserPointer : MonoBehaviour
{
    public GameObject vrRig;
    public Transform cameraRigTransform, headTransform;
    public SteamVR_Action_Boolean laserClickAction, resetCellPositionAction, swapColoursAction;
    public SteamVR_Action_Vector2 changePointerSize;
    public SteamVR_Input_Sources hand;
    public Collider floorCollider;
    public GameObject cell;
    public CustomInputModule inputModule;
    public Material laserMat;
    public float pointerSizeChangeRate = 0.1f;
    private float defaultLength = 100.0f;
    private Transform laserPointerSphereCell;
    private GameObject laserPointerSphereNonCell;
    private LineRenderer lineRenderer;
    private MeshController cellMesh;

    private void Start()
    {
        // Store components
        cellMesh = cell.GetComponent<MeshController>();
        lineRenderer = GetComponent<LineRenderer>();
        laserPointerSphereCell = transform.GetChild(0);
        laserPointerSphereNonCell = transform.GetChild(1).gameObject;
    }

    // Update is called once per frame
    private void Update()
    {
        // Show the laser pointer and get the raycast
        RaycastHit hit = DisplayLaser();
        // If pointing at floor, teleport
        if (laserClickAction.GetLastStateDown(hand) && hit.collider == floorCollider)
            Teleport();
        // If laser pointer tip is not pink and over the cell adapt the size
        if (!laserPointerSphereNonCell.activeSelf)
            ChangePointerSize();
        // If pointing at the cell
        if (hit.collider == cell.GetComponent<Collider>())
        {
            // If laser has non-zero green (i.e. is grey)
            if (laserMat.color.g != 0)
            {
                // Make scaling tip active
                laserPointerSphereCell.gameObject.SetActive(true);
                laserPointerSphereNonCell.SetActive(false);
                // Clicking laser sets alpha
                if (laserClickAction.GetState(hand) || Input.GetMouseButton(0))
                    SetAlpha(hit);
            }
            // If laser is pink
            else if (laserMat.color.r == laserMat.color.b)
            {
                // Make non-scaling tip active
                laserPointerSphereCell.gameObject.SetActive(false);
                laserPointerSphereNonCell.SetActive(true);
                // Clicking laser toggles marker
                if (laserClickAction.GetLastStateDown(hand) || Input.GetMouseButtonDown(0))
                    AnnotateMarker(hit);
            }
            else
            {
                // Make scaling tip active
                laserPointerSphereCell.gameObject.SetActive(true);
                laserPointerSphereNonCell.SetActive(false);
                // Clicking laser sets colour
                if (laserClickAction.GetState(hand) || Input.GetMouseButton(0))
                    AnnotateColour(hit);
            }
        }
        else
        {
            // Make non-scaling tip active
            laserPointerSphereCell.gameObject.SetActive(false);
            laserPointerSphereNonCell.SetActive(true);
        }
        // Check if laser swap button pressed
        if (swapColoursAction.GetLastStateDown(hand) || Input.GetMouseButtonDown(1))
            SwapLaserColours();
    }

    private RaycastHit DisplayLaser()
    {
        float targetLength = defaultLength;
        if (inputModule)
        {
            // Set length of laser to the distance to the object hit
            PointerEventData data = inputModule.GetData();
            targetLength = data.pointerCurrentRaycast.distance == 0 ? defaultLength : data.pointerCurrentRaycast.distance;
        }
        RaycastHit hit;
        // Send a raycast
        Physics.Raycast(transform.position, transform.forward, out hit, defaultLength);
        Vector3 endPos = transform.position + (transform.forward * targetLength);
        if (hit.collider != null)
            endPos = hit.point;
        // If the desktop camera is being used off set source of laser so not looking directly down laser
        if (!vrRig.activeInHierarchy)
        {
            Vector3 offset = transform.localPosition;
            offset.x += 0.1f;
            offset.y -= 0.1f;
            offset = transform.TransformPoint(offset);
            lineRenderer.SetPosition(0, offset);
        }
        else
        {
            lineRenderer.SetPosition(0, transform.position);
        }
        // Set position of end of laser and tip sphere
        lineRenderer.SetPosition(1, endPos);
        laserPointerSphereCell.position = endPos;
        laserPointerSphereNonCell.transform.position = endPos;
        return hit;
    }

    private void Teleport()
    {
        // Calculate distance between head and centre of VR space
        Vector3 difference = cameraRigTransform.position - headTransform.position;
        difference.y = 0;
        // Move camera rig to the tip of laser with added difference to ensure
        // head is in place pointed to
        cameraRigTransform.position = laserPointerSphereCell.position + difference;
    }

    private void ChangePointerSize()
    {
        // Get change in axis
        var delta = changePointerSize.GetAxis(hand).y * pointerSizeChangeRate;
        delta += Input.GetAxis("Mouse ScrollWheel");
        delta *= pointerSizeChangeRate;
        var scaleChange = new Vector3(delta, delta, delta);
        // Make sure sphere stays above 0
        laserPointerSphereCell.localScale = laserPointerSphereCell.localScale.x  <= -scaleChange.x ? laserPointerSphereCell.localScale : laserPointerSphereCell.localScale + scaleChange;
    }

    private void SetAlpha(RaycastHit hit)
    {
        var rad = laserPointerSphereCell.localScale.x / 2f;
        cellMesh.SetAlpha(hit, rad);
    }

    private void AnnotateColour(RaycastHit hit)
    {
        var rad = laserPointerSphereCell.localScale.x / 2f;
        // Check if laser is red
        if (laserMat.color.b == 0)
            cellMesh.SetColour(hit, Color.red, rad);
        else
            cellMesh.SetColour(hit, Color.blue, rad);
    }

    private void AnnotateMarker(RaycastHit hit)
    {
        cellMesh.ToggleMarker(hit);
    }

    private void SwapLaserColours()
    {
        // Cycle through laser colours
        if (laserMat.color.g != 0)
            laserMat.color = new Color32(180, 0, 180, 150);
        else if (laserMat.color.r == laserMat.color.b)
            laserMat.color = new Color32(0, 0, 180, 150);
        else if (laserMat.color.r == 0)
            laserMat.color = new Color32(180, 0, 0, 150);
        else
            laserMat.color = new Color32(100, 100, 100, 150);
    }
}
