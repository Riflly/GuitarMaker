using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

//Script manages the user rotation of the guitar
public class RotateScript : MonoBehaviour
{
    public float speed;
    public Transform guitar;
    public Canvas canvas;
    public bool canRotate = false;

    void OnEnable()
    {
        //Gets the main UI canvas object
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
    }
    // Update is called once per frame
    void Update()
    {
        //Checks if the left mouse is being pressed on this frame and whether the mouse is over UI
        //If pressed and not over UI, allows the user to rotate guitar on subsequent frames
        //This canRotate variable is necessary to allow the user to drag the mouse over UI elements during rotation 
        //after the initial press, as simply checking for the mouse over UI every frame would stop the
        //rotation on the frame where the user first hovered over UI. I instead only want to prevent guitar rotation when the
        //initial press is over UI.
        if (Input.GetMouseButtonDown(0) && !canvas.GetComponent<UIMouseCheck>().IsMouseOverUI() && canRotate == false)
        {
            canRotate = true;
        }
        if (Input.GetMouseButtonUp(0) && canRotate == true)
        {
            canRotate = false;
        }
        if (Input.GetMouseButton(0) && canRotate)
        {
            //Performs guitar rotation using guitar position, the rotational axis (Y), and the mouse movement times a set speed
            transform.RotateAround(guitar.position, transform.up, -Input.GetAxis("Mouse X") * speed);
        }
    }

}
