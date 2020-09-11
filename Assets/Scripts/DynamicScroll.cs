using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using System.IO;
//Script manages the data and dynamic aspects of the save file scrollview
public class DynamicScroll : MonoBehaviour
{
    public GameObject Prefab;
    public Transform Container;
    public List<GameObject> Buttons = new List<GameObject>();
    string[] files;
    public string file;

    void OnEnable()
    {
        file = "";
        files = Directory.GetFiles(Application.persistentDataPath );

        //For each save file found, creates a button in the scrollview
        for (int i = 0; i < files.Length; i++)
        {
            if (Path.GetExtension(files[i]).Equals(".save"))
            {
                GameObject go = Instantiate(Prefab);
                go.GetComponentInChildren<Text>().text = Path.GetFileNameWithoutExtension(files[i]);
                go.transform.SetParent(Container);
                go.transform.localPosition = Vector3.zero;
                go.transform.localScale = Vector3.one;
                int buttonIndex = i;
                go.GetComponent<Button>().onClick.AddListener(() => OnButtonClick(buttonIndex));
                Buttons.Add(go);
            }
        }
        Canvas.ForceUpdateCanvases();
    }
    //Destroys scrollview buttons when disabled
    private void OnDisable()
    {
        foreach (GameObject button in Buttons)
            Destroy(button);
        Buttons.Clear();
    }

    //When one of the buttons pertaining to a save is clicked
    //updates the file variable with that button's file path
    public void OnButtonClick(int index)
    {
        file = files[index];
    }
}
