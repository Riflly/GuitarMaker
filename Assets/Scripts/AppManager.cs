using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
//Script manages majority of application operations
public class AppManager : MonoBehaviour
{
    public GuitarAssets[] guitars;
    public PickupAssets[] pickups;
    public Text bodyText;
    public Text headText;
    public Button[] prevPickupButtons;
    public Button[] nextPickupButtons;
    public Button prevBodyButton;
    public Button nextBodyButton;
    public Button prevHeadButton;
    public Button nextHeadButton;
    public Button fretMaple;
    public Button fretRose;
    public Button guardWhite;
    public Button guardBlack;
    public Button exitButton;
    public Button saveButton;
    public Toggle guardTog;
    public GameObject colorPickerObj;
    public FlexibleColorPicker colorPicker;
    public Material bodyMat;
    public Material guardMat;
    public Material fretMat;
    public Texture fretMapleTex;
    public Texture fretRoseTex;
    public InputField nameField;
    public GameObject nameWarning;
    public GameObject saveSuccess;
    public GameObject saveDisplay;
    public GameObject scrollView;
    int body = 0;
    int head = 0;
    int[] pickupTypes = {0, 0, 0};
    bool justSaved = false;
    bool nameValid = false;
    float justSavedCounter = 0f;
    public Text[] pickupNames = new Text[3];

    //OnEnable is called when script is first enabled
    void OnEnable()
    {
        //Adds event listeners for the pickup buttons.
        //By passing the pickup location as an int, I only need one Next and Previous method
        //for the pickup swapping, as opposed to three (0 = Neck, 1 = Mid, 2 = Bridge).
        prevPickupButtons[0].onClick.AddListener(() => PrevPickupCallBack(0));
        nextPickupButtons[0].onClick.AddListener(() => NextPickupCallBack(0));
        prevPickupButtons[1].onClick.AddListener(() => PrevPickupCallBack(1));
        nextPickupButtons[1].onClick.AddListener(() => NextPickupCallBack(1));
        prevPickupButtons[2].onClick.AddListener(() => PrevPickupCallBack(2));
        nextPickupButtons[2].onClick.AddListener(() => NextPickupCallBack(2));
        //Event listeners for pickguard color change, so that only one color change method is needed.
        //I don't simply pass Color values such as Color.white because the switch statement requires constant values
        guardWhite.onClick.AddListener(() => ChangeGuardColor(0));
        guardBlack.onClick.AddListener(() => ChangeGuardColor(1));

        //Event listeners for fret material changing (0 = maple, 1 = rosewood)
        fretMaple.onClick.AddListener(() => ChangeFretMat(0));
        fretRose.onClick.AddListener(() => ChangeFretMat(1));

        //Event listener for name change for name validation
        nameField.onValueChanged.AddListener(delegate { ValidateName(nameField.text); });
    }
    //Start is called before the first frame update
    public void Start()
    {
        //Loads the guitar body assets into an array
        guitars = new GuitarAssets[] 
        { 
            new GuitarAssets("Strat", GameObject.Find("BodyStrat"), GameObject.Find("BodyStratKnobs"), GameObject.Find("GuardStrat"), GameObject.Find("HeadStrat")), 
            new GuitarAssets("Tele", GameObject.Find("BodyTele"), GameObject.Find("BodyTeleKnobs"), GameObject.Find("GuardTele"), GameObject.Find("HeadTele")) 
        };

        //Set the display text for the starting body and headstock
        bodyText.text = guitars[body].Name;
        headText.text = guitars[head].Name;

        //Loads the pickup assets into an array
        pickups = new PickupAssets[]
        {
            new PickupAssets("Single", new GameObject[] {GameObject.Find("PickupSingleNeck"), GameObject.Find("PickupSingleMid"), GameObject.Find("PickupSingleBridge") }),
            new PickupAssets("Humbucker", new GameObject[] {GameObject.Find("PickupHumNeck"), GameObject.Find("PickupHumMid"), GameObject.Find("PickupHumBridge") })
        };
        //Sets the first GuitarAssets objects to be the only ones active
        for(int i = 1; i < guitars.Length; i++)
        {
            guitars[i].Body.SetActive(false);
            guitars[i].Knobs.SetActive(false);
            guitars[i].Guard.SetActive(false);
            guitars[i].Head.SetActive(false);
        }
        //Sets previous body and head buttons to be inactive, since the first
        //body and head are enabled at application start
        prevBodyButton.interactable = false;
        prevHeadButton.interactable = false;

        //Deactivates the humbucker pickups so that the single pickups are the default when the app is first loaded
        //Also sets the display text to "Single" for all pickups initially
        for(int i = 0; i < pickupTypes.Length; i++)
        {
            pickups[1].Positions[i].SetActive(false);
            pickupNames[i].text = pickups[pickupTypes[i]].Name;
            prevPickupButtons[i].interactable = false;
        }

        //Sets the default body color to body material
        Color bodyColor = new Color(1f, 0.9f, 0.56f);
        bodyMat.color = bodyColor;
        colorPicker = colorPickerObj.GetComponent<FlexibleColorPicker>();
        //Sets default body color to color picker
        colorPicker.color = bodyColor;

        //Hides color picker initially
        colorPickerObj.SetActive(false);

        //Hides the save panel initially
        saveDisplay.SetActive(false);

        //Sets guard color to white at startup
        ChangeGuardColor(0);
        
        //Sets fretboard material to maple initially
        ChangeFretMat(0);

        //Initially hide save success message
        Color tempColor = saveSuccess.GetComponent<Text>().color;
        tempColor.a = 0;
        saveSuccess.GetComponent<Text>().color = tempColor;

        //Name field will always start empty, so defaults save button to un-interactable at start
        saveButton.interactable = false;
    }

    // Update is called once per frame
    void Update()
    {
        //Every frame, if the body color differs from the picker color,
        //sets the guitar body color to the picker color
        //If I had more time, I would try to implement this with an
        //event listener
        if (bodyMat.color != colorPicker.color)
            bodyMat.color = colorPicker.color;

        //If just saved, slowly fades the successful save message away
        if(justSaved == true)
        {
            justSavedCounter += 100f * Time.deltaTime;
            if(justSavedCounter >= 60f)
            {
                Color tempColor = saveSuccess.GetComponent<Text>().color;
                if (tempColor.a - Time.deltaTime >= 0)
                {
                    tempColor.a -= Time.deltaTime;
                    saveSuccess.GetComponent<Text>().color = tempColor;
                }
                else
                {
                    tempColor.a = 0f;
                    justSavedCounter = 0f;
                    justSaved = false;
                }
            }
        }

    }
    //Deactivates the current pickup and activates the previous one
    //on button click, changing text to match
    private void PrevPickupCallBack(int pickupPos)
    {
        //pickupPos 0 = Neck, 1 = Mid, 2 = Bridge
        if (pickupTypes[pickupPos] > 0)
        {
            UpdatePickup(pickupTypes[pickupPos] - 1, pickupPos);
        }
    }
    //Deactivates the current pickup and activates the next one
    //on button click, changing text to match
    private void NextPickupCallBack(int pickupPos)
    {
        //pickupPos 0 = Neck, 1 = Mid, 2 = Bridge
        if(pickupTypes[pickupPos] < pickups.Length)
        {
            UpdatePickup(pickupTypes[pickupPos] + 1, pickupPos);
        }


    }

    //Deactivates the current guitar body and activates the next one
    //on button click, changing text to match
    public void NextBody()
    {
        //If not on last guitar body option
        if (body < guitars.Length - 1)
        {
            UpdateBody(body + 1);
        }
    }
    //Deactivates the current guitar body and activates the previous one
    //on button click, changing text to match
    public void PrevBody()
    {
        //If not on first guitar body option
        if (body > 0)
        {
            UpdateBody(body - 1);
        }
    }
    
    //Takes toggle value, and sets the pickguard to be active or inactive
    //depending on boolean value
    public void GuardToggle(bool input)
    {
        //On UI toggle, activates or deactivates current pickguard
        guitars[body].Guard.SetActive(input);
        //Sets the toggle in case of call from randomization method
        guardTog.isOn = input;

    }
    //Deactivates the current guitar head and activates the next one
    //on button click, changing text to match
    public void NextHead()
    {
        //If not on last guitar head option
        if (head < guitars.Length - 1)
        {
            UpdateHead(head + 1);
        }
    }
    //Deactivates the current guitar head and activates the previous one
    //on button click, changing text to match
    public void PrevHead()
    {
        //If not on first guitar head option
        if (head > 0)
        {
            UpdateHead(head - 1);
        }
    }

    //Randomized all aspects of current guitar besides name
    public void Randomize()
    {
        UpdateBody(Random.Range(0, 2));
        GuardToggle(Random.Range(0, 2) != 0);
        UpdateHead(Random.Range(0, 2));
        UpdatePickup(Random.Range(0, 3), 0);
        UpdatePickup(Random.Range(0, 3), 1);
        UpdatePickup(Random.Range(0, 3), 2);
        ChangeGuardColor(Random.Range(0, 2));
        ChangeFretMat(Random.Range(0, 2));
        Color tempColor = Random.ColorHSV();
        //Checks for dark color, and rerolls color if so,
        //as darker body colors are somewhat less common and desirable
        if(tempColor.r + tempColor.g + tempColor.b < 1.5)
        {
            tempColor = Random.ColorHSV();
        }
        colorPicker.color = tempColor;

    }

    //Toggles display of color picker
    public void TogglePicker()
    {
        //Sets color picker active state to opposite of current
        if (colorPicker.color != bodyMat.color)
            colorPicker.color = bodyMat.color;
        colorPickerObj.SetActive(!colorPickerObj.activeInHierarchy);

    }

    //Updates body to target body
    public void UpdateBody(int targetBody)
    {
        if (targetBody == guitars.Length - 1)
        {
            nextBodyButton.interactable = false;
            prevBodyButton.interactable = true;
        }
        else if(targetBody == 0)
        {
            nextBodyButton.interactable = true;
            prevBodyButton.interactable = false;
        }
        guitars[body].Body.SetActive(false);
        guitars[body].Knobs.SetActive(false);
        if (guitars[body].Guard.activeInHierarchy)
        {
            guitars[body].Guard.SetActive(false);
            guitars[targetBody].Guard.SetActive(true);
        }
        body = targetBody;
        guitars[body].Body.SetActive(true);
        guitars[body].Knobs.SetActive(true);
        bodyText.text = guitars[body].Name;
    }

    //Updates headstock to target headstock
    public void UpdateHead(int targetHead)
    {
        if (targetHead == guitars.Length - 1)
        {
            nextHeadButton.interactable = false;
            prevHeadButton.interactable = true;
        }
        else if (targetHead == 0)
        {
            nextHeadButton.interactable = true;
            prevHeadButton.interactable = false;
        }
        //Deactivates current guitar body and activates the prev one
        guitars[head].Head.SetActive(false);
        head = targetHead;
        guitars[head].Head.SetActive(true);
        //Changes display text to match new guitar head
        headText.text = guitars[head].Name;
    }

    //Updates pickup of a certain position (Neck, Mid, or Bridge) to the target pickup type
    public void UpdatePickup(int targetPickup, int pickupPos)
    {
        if (pickupTypes[pickupPos] < pickups.Length)
            pickups[pickupTypes[pickupPos]].Positions[pickupPos].SetActive(false);
        pickupTypes[pickupPos] = targetPickup;
        if (targetPickup < pickups.Length)
        {
            if (targetPickup == 0)
            {
                prevPickupButtons[pickupPos].interactable = false;
                if (!nextPickupButtons[pickupPos].interactable)
                    nextPickupButtons[pickupPos].interactable = true;
            }
            else
            {
                prevPickupButtons[pickupPos].interactable = true;
                nextPickupButtons[pickupPos].interactable = true;
            }
            pickups[pickupTypes[pickupPos]].Positions[pickupPos].SetActive(true);
            pickupNames[pickupPos].text = pickups[pickupTypes[pickupPos]].Name;
        }
        else
        {
            pickupNames[pickupPos].text = "None";
            nextPickupButtons[pickupPos].interactable = false;
            if (!prevPickupButtons[pickupPos].interactable)
                prevPickupButtons[pickupPos].interactable = true;
        }
    }
    //Method takes an integer input from a button event (currently either 0 for white or 1 for black)
    //and changes the guard color depending on that input
    public void ChangeGuardColor(int color)
    {
        switch (color)
        {
            case 0:
                guardMat.color = Color.white;
                guardWhite.interactable = false;
                guardBlack.interactable = true;
                break;
            case 1:
                guardMat.color = Color.black;
                guardWhite.interactable = true;
                guardBlack.interactable = false;
                break;
            default:
                break;
        }
    }

    //Method takes an integer input from a button event (currently either 0 for maple or 1 for rosewood)
    //and changes the fretboard material texture depending on that input
    public void ChangeFretMat(int wood)
    {
        switch (wood)
        {
            case 0:
                fretMat.SetTexture("_MainTex", fretMapleTex);
                fretMaple.interactable = false;
                fretRose.interactable = true;
                break;
            case 1:
                fretMat.SetTexture("_MainTex", fretRoseTex);
                fretMaple.interactable = true;
                fretRose.interactable = false;
                break;
            default:
                break;
        }
    }

    //Method saves guitar
    public void SaveGuitar()
    {
        //Validation
        if (nameValid)
        {
            int guardColor = 0;
            if (guardMat.color == Color.black)
                guardColor = 1;
            int fretTex = 0;
            if (fretMat.GetTexture("_MainTex") == fretRoseTex)
                fretTex = 1;
            //Breaks body color down into a float array of the RGB components for serialization
            float[] bodyColor =
                {
                bodyMat.color.r,
                bodyMat.color.g,
                bodyMat.color.b
                };
            //Creates Save object with guitar data
            Save save = new Save(nameField.text, body, head, pickupTypes, bodyColor, guardColor, fretTex, guardTog.isOn);
            BinaryFormatter bf = new BinaryFormatter();
            //Saves guitar data to file with binary serialization
            FileStream file = File.Create(Application.persistentDataPath + "/" + save.guitarName + ".save");
            bf.Serialize(file, save);
            file.Close();
            //Displays a save successful message
            Color tempColor = saveSuccess.GetComponent<Text>().color;
            tempColor.a = 1;
            saveSuccess.GetComponent<Text>().color = tempColor;
            justSaved = true;
        }
    }
    
    //Toggles the display of the load panel
    public void ToggleLoad()
    {
        saveDisplay.SetActive(!saveDisplay.activeInHierarchy);
    }

    //Method loads guitar selected from scrollview
    public void LoadGuitar()
    {
        string filePath = scrollView.GetComponent<DynamicScroll>().file;
        if (!filePath.Equals(""))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.OpenRead(filePath);
            Save save = (Save) bf.Deserialize(file);
            nameField.text = save.guitarName;
            UpdateBody(save.bodyIndex);
            UpdateHead(save.headIndex);
            for(int i = 0; i < 3; i++)
            {
                UpdatePickup(save.pickups[i], i);
            }
            GuardToggle(save.guardTog);
            ChangeGuardColor(save.guardColor);
            ChangeFretMat(save.fretTex);
            colorPicker.color = new Color(save.bodyColor[0], save.bodyColor[1], save.bodyColor[2]);
        }
    }

    //Quits application
    public void ExitApp()
    {
        Application.Quit();
    }

    //Validates name input
    public void ValidateName(string name)
    {
        //Regex for valid Windows filename
        Regex rg = new Regex(@"^[\w\-. ]+$");
        if (!name.Equals("") && rg.IsMatch(name))
        {
            if (!nameValid)
            {
                nameValid = true;
                nameWarning.SetActive(false);
                saveButton.interactable = true;
            }
        }
        else
        {
            if (nameValid)
            {
                nameValid = false;
                nameWarning.SetActive(true);
                saveButton.interactable = false;
            }

        }

            
    }

}
//Custom class to store assets for specific guitar model (E.g. Strat, Tele)
public class GuitarAssets
{
    public string Name { get;}
    public GameObject Body { get;}
    public GameObject Knobs { get; }
    public GameObject Guard { get;}
    public GameObject Head { get; }
    public GuitarAssets(string name, GameObject body, GameObject knobs, GameObject guard, GameObject head)
    {
        Name = name;
        Body = body;
        Knobs = knobs;
        Guard = guard;
        Head = head;
    }
}
//Custom class to store pickup assets
public class PickupAssets
{
    public string Name { get; }
    public GameObject[] Positions { get; }
    public PickupAssets(string name, GameObject[] positions)
    {
        Name = name;
        Positions = positions;
    }
}

//class for saving guitar data to file
[System.Serializable]
public class Save
{
    public string guitarName;
    public int bodyIndex;
    public int headIndex;
    public int[] pickups;
    public float[] bodyColor;
    public int guardColor;
    public int fretTex;
    public bool guardTog;

    public Save(string guitarName, int bodyIndex, int headIndex, int[] pickups, float[] bodyColor, int guardColor, int fretTex, bool guardTog)
    {
        this.guitarName = guitarName;
        this.bodyIndex = bodyIndex;
        this.headIndex = headIndex;
        this.pickups = pickups;
        this.bodyColor = bodyColor;
        this.guardColor = guardColor;
        this.fretTex = fretTex;
        this.guardTog = guardTog;
    }
}