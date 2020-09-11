using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIMouseCheck : MonoBehaviour
{
    //Checks if the mouse is currently over UI
    public bool IsMouseOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }
}
