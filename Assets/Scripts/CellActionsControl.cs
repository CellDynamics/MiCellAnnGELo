using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class CellActionsControl : MonoBehaviour
{
    public SteamVR_Action_Boolean playPause, skipBack, skipForward, 
                                  speedUp, speedDown, toBegining, 
                                  resetCellPositionAction, swapColourModeAction;
    public SteamVR_Input_Sources source;
    private MeshController meshController;

    private void Start()
    {
        meshController = GetComponent<MeshController>();
    }

    // Process any user inputs during the update
    private void Update()
    {
        if (playPause.GetStateDown(source) || Input.GetKeyDown("p"))
            meshController.PlayPause();

        if (skipBack.GetStateDown(source) || Input.GetKeyDown("i"))
            meshController.FrameSkip(-1);

        if (skipForward.GetStateDown(source) || Input.GetKeyDown("o"))
            meshController.FrameSkip(1);

        if (speedUp.GetStateDown(source) || Input.GetKeyDown("l"))
            meshController.ChangeSpeed(false);

        if (speedDown.GetStateDown(source) || Input.GetKeyDown("k"))
            meshController.ChangeSpeed(true);

        if (toBegining.GetStateDown(source) || Input.GetKeyDown("u"))
            meshController.ToBegining();

        if (swapColourModeAction.GetLastStateDown(source) || Input.GetKeyDown("j"))
            meshController.SwitchColourMode();

        if (resetCellPositionAction.GetState(source) || Input.GetKeyDown("h"))
            meshController.ResetCellPosition();
    }
}
