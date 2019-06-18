using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//for moving the player camera using the mouse and keyboard
public class testCameraScript : MonoBehaviour
{
    public float speedH = 2.0f;
    public float speedV = 2.0f;

    private float yaw = 0.0f;
    private float pitch = 0.0f;
    private bool useMouse = true;

    void Update()
    {
        //trigger mouse input to look around w/o VR headset
        if(Input.GetKeyDown(KeyCode.Mouse1))
        {
            useMouse = !useMouse;
        }

        if (useMouse)
        {
            yaw += speedH * Input.GetAxis("Mouse X");
            pitch -= speedV * Input.GetAxis("Mouse Y");

            transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
        }




        //move player in test mode (without VR headset)
        //move player forward
        if (Input.GetKey(KeyCode.W))
        {
            transform.position += transform.forward * 10 * Time.deltaTime;
        }

        //move player backward
        if(Input.GetKey(KeyCode.S))
        {
            transform.position -= transform.forward * 10 * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.D))
        {
            transform.position += transform.right * 10 * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.A))
        {
            transform.position -= transform.right * 10 * Time.deltaTime;
        }

    }
}
