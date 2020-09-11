using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
//Script is used to check if the mouse is over a specific UI panel
public class PanelUIMouseCheck : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public bool mouseOverPanelUI = false;
    //Sets boolean to true when mouse pointer enters this object
    public void OnPointerEnter(PointerEventData eventData)
    {
        mouseOverPanelUI = true;
    }
    //Sets boolean to false when mouse pointer leaves this object
    public void OnPointerExit(PointerEventData eventData)
    {
        mouseOverPanelUI = false;
    }
}
