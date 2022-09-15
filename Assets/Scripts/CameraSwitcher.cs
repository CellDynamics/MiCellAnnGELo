using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    public GameObject pcCamera, vrRig, laserPointer;
    private Transform rightController;

    private void Start()
    {
        rightController = vrRig.transform.GetChild(1);
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown("tab"))
        {   
            // Switch active camera
            vrRig.SetActive(!vrRig.activeInHierarchy);
            pcCamera.SetActive(!pcCamera.activeInHierarchy);
            // If put into desktop mode
            if (pcCamera.activeInHierarchy)
            {
                // Set laser pointer's parent to the camera and hide cursor
                laserPointer.transform.SetParent(pcCamera.transform, false);
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                // Set laser pointer's parent to the right controller and reveal cursor
                laserPointer.transform.SetParent(rightController, false);
                Cursor.lockState = CursorLockMode.None;
            }
        }
    }
}
