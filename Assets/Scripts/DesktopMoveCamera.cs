using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesktopMoveCamera : MonoBehaviour
{
    // Speed camera moves at in m/s
    public float baseMoveSpeed = 1f;

    // Update is called once per frame
    private void Update()
    {
        // Rotate camera
        float x = Input.GetAxis("Mouse X");
        float y = -Input.GetAxis("Mouse Y");
        // Rotate camera vertically using local x axis
        transform.Rotate(y, 0, 0);
        // Rotate camera horizontally using world y axis
        transform.Rotate(0, x, 0, Space.World);

        // Move camera
        float moveSpeed = baseMoveSpeed;
        if (Input.GetKey("left shift"))
            moveSpeed *= 2;
        else if (Input.GetKey("left ctrl"))
            moveSpeed *= 0.5f;

        // Get current position
        var pos = transform.position;

        // Move forward/back
        if (Input.GetKey("w"))
            pos += transform.forward * (moveSpeed * Time.deltaTime);
        else if (Input.GetKey("s"))
            pos -= transform.forward * (moveSpeed * Time.deltaTime);

        // Move sideways
        if (Input.GetKey("d"))
            pos += transform.right * (moveSpeed * Time.deltaTime);
        else if (Input.GetKey("a"))
            pos -= transform.right * (moveSpeed * Time.deltaTime);

        // Move up/down
        if (Input.GetKey("e"))
            pos += transform.up * (moveSpeed * Time.deltaTime);
        else if (Input.GetKey("q"))
            pos -= transform.up * (moveSpeed * Time.deltaTime);

        // Reset position
        if (Input.GetKeyDown("r"))
            pos = new Vector3(0f, 1.6f, -1f);

        // Don't allow camera to below -3 z or below 0 y
        pos.z = pos.z < -3 ? -3 : pos.z;
        pos.y = pos.y < 0 ? 0 : pos.y;

        // Update camera position
        transform.position = pos;
    }
}
