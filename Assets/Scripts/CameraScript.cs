using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Script manages Camera animations
public class CameraScript : MonoBehaviour
{
    public Transform mainCamPos;
    public Transform bodyCamPos;
    public Transform headCamPos;
    public Transform fretCamPos;
    public GameObject bodyHover;
    public GameObject headHover;
    public GameObject fretHover;
    public GameObject bodyColorHover;
    public float speed;

    // Update is called once per frame
    void Update()
    {
        //Checks if mouse is over any of the headstock specific UI, and moves camera for a closeup of the headstock if so
        if (headHover.GetComponent<PanelUIMouseCheck>().mouseOverPanelUI)
        {
            MoveCamera(headCamPos.position);
        }
        //Checks if mouse is over any of the fretboard specific UI, and moves camera for a closeup of the fretboard if so
        else if (fretHover.GetComponent<PanelUIMouseCheck>().mouseOverPanelUI)
        {
            MoveCamera(fretCamPos.position);
        }
        //Checks if mouse is over any of the body specific UI, and moves camera for a closeup of the body if so
        else if (bodyHover.GetComponent<PanelUIMouseCheck>().mouseOverPanelUI || bodyColorHover.GetComponent<PanelUIMouseCheck>().mouseOverPanelUI)
        {
            MoveCamera(bodyCamPos.position);
        }
        //Moves camera to default position if mouse isn't over any area specific UI
        else
        {
            MoveCamera(mainCamPos.position);
        }
    }
    //Method moves progressively from initial positon to destiniation.
    //When called from Update, creates a smooth transition across many frames
    void MoveCamera(Vector3 destination)
    {
        transform.position = Vector3.Lerp(transform.position, destination, Time.deltaTime * speed);
    }
}
