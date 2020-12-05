using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine.SceneManagement;
//using System.Runtime.Remoting.Channels;
//using System.Runtime.Remoting.Channels;
using UnityEngine;
using UnityEditor; // use for confirmation prompts

public class GUIscript : MonoBehaviour
{
    public GameObject Object;

    public gameMechanics controller;
    string buttonSymbol = "▶";
    bool trackReset = false;
    bool objectError = false;
    bool levelMenuToggle = false;
    bool settingsReset = false; // if true a confirmation prompt for resetting the settings will appear
    bool levelReset = false; // if true a confirmation prompt for resetting the level will appear 

    private objectProperties objProperties;

    // Variables related to retrieving the background music
    private GameObject background;
    private bool hasSound = false;
    private AudioSource audioSource;

    private float loadInTime = 0;

    // Types of win
    public bool dontMove = true;
    public bool collectKeys = false;
    public bool goToTarget = false;

    // Win condition variables
    private bool positionsSaved = false;
    private bool keysSaved = false;
    private int keyAmount = 0;
    private List<Vector3> startingLocation = new List<Vector3>();
    private List<Vector3> goalLocations = new List<Vector3>();
    private List<GameObject> hostileObjects = new List<GameObject>();
    private List<GameObject> controllableObjects = new List<GameObject>();
    Dictionary<Vector3, GameObject> keyLocations = new Dictionary<Vector3, GameObject>();
    GameObject playObj;

    // Variables related to object spawn positions
    private int paddingx = 1;
    private int paddingy = 1;

    // Default Locations to return to
    //List<KeyValuePair<string, Vector3>> defaultLocations = new List<KeyValuePair<string, Vector3>>();

    public String levelName = "";
    public String instruction = "";
    public String levelID = ""; // unique identifier for the level
    public int levelIDlength = 25;

    public int winConditions = 3; // edit if more win conditions are added
    public int maxInstructionLength = 25;
    public int maxNameLength = 25;

    bool deleteObjects = false; // boolean controlling prompt asking if the user is sure about deleting objects

    // Start is called before the first frame update
    void Start()
    {
        // disable physics during edit mode
        Physics2D.autoSimulation = false;

        controller = GetComponent<gameMechanics>();
        background = GameObject.Find("background");

        playObj = GameObject.Find("PlayModeObj");

        // generate levelID
        generateID();


    }

    // Update is called once per frame
    void Update()
    {
        
        if (!controller.playingGame && playObj.GetComponent<playTrack>().levelsLeft() > 0 && (playObj.GetComponent<playTrack>().getPlay3() || playObj.GetComponent<playTrack>().getPlayLevel())) {
            

            if (loadInTime == 0) {
                string nextLevel = playObj.GetComponent<playTrack>().nextLevel();
                print(nextLevel);
                GameObject.Find("background").GetComponent<returnToMenu>().loadLevel(nextLevel);
                loadInTime = Time.time;
            }

            else if (Time.time - loadInTime > 3) {
                controller.playingGame = true;
                loadInTime = 0;
            }

        }

        // Case if done in play mode
        else if (!controller.playingGame && playObj.GetComponent<playTrack>().levelsLeft() == 0 && (playObj.GetComponent<playTrack>().getPlay3() || playObj.GetComponent<playTrack>().getPlayLevel())) {
            SceneManager.LoadScene("TitleScreen");
        }

        if (controller.playingGame == false && buttonSymbol == "| |" && !(playObj.GetComponent<playTrack>().getPlay3() || playObj.GetComponent<playTrack>().getPlayLevel()))
        {
            buttonSymbol = "▶";
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            if (!levelMenuToggle)
                levelMenuToggle = true;

            else
                levelMenuToggle = false;

        }

        if (controller.timeLeft < 0)
        {
            controller.playingGame = false;
            controller.triggerLose();
            keyLocations.Clear();
            goalLocations.Clear();
            startingLocation.Clear();
            Physics2D.autoSimulation = false; // turn off physics in edit mode
            resetObjects(); // reset all objects
        }

        // Win condition is not to move
        if (controller.playingGame == true && dontMove)
        {
            if (!positionsSaved)
            {
                foreach (GameObject g in controller.objectList)
                {
                    startingLocation.Add(g.transform.position);
                }

                positionsSaved = true;
            }

            if (positionsSaved && controller.timeLeft < 1)
            {
                controller.triggerWin();
                resetObjects();
                positionsSaved = false;
                startingLocation.Clear();

                if (playObj.GetComponent<playTrack>().getPlay3() || playObj.GetComponent<playTrack>().getPlayLevel())
                    playObj.GetComponent<playTrack>().incrementWins();
            }

            else if (positionsSaved)
            {
                foreach (GameObject g in controller.objectList)
                {
                    if (!startingLocation.Contains(g.transform.position))
                    {
                        controller.triggerLose();
                        resetObjects();
                        positionsSaved = false;
                        startingLocation.Clear();
                        break;
                    }
                        
                }
               
            }

        }
        

        // Win condition is to go to target
        if (controller.playingGame == true && goToTarget)
        {
            // Bulid a collection of all of the goal positions
            if (goalLocations.Count == 0)
            {
                foreach (GameObject g in controller.objectList)
                {
                    string objName = g.name.ToUpper();
                    // If the object is a gameobject, get the position properties
                    if (objName.IndexOf("OBJECT") >= 0)
                    {
                        if ((g.GetComponentsInChildren<objectProperties>())[0].isTarget)
                        {
                            goalLocations.Add(new Vector3(Mathf.RoundToInt(g.transform.position.x), Mathf.RoundToInt(g.transform.position.y), Mathf.RoundToInt(g.transform.position.z)));
                        }
                    }
                            
                }
            }
            
            else
            {
                foreach (GameObject g in controller.objectList)
                {
                    string objName = g.name.ToUpper();
                    // If the object is a gameobject, get the position properties
                    if (objName.IndexOf("OBJECT") >= 0)
                        if ((g.GetComponentsInChildren<objectProperties>())[0].controllable)
                            if (goalLocations.Contains(new Vector3(Mathf.RoundToInt(g.transform.position.x), Mathf.RoundToInt(g.transform.position.y), Mathf.RoundToInt(g.transform.position.z))))
                            {
                                controller.triggerWin();
                                goalLocations.Clear();
                                resetObjects();
                            }
                }
            }


            // Case if there are no goals
            if (goalLocations.Count == 0)
            {
                controller.triggerWin();
                goalLocations.Clear();
                resetObjects();
            }
        }

        // Win condition is to collect keys
        if (controller.playingGame == true && collectKeys)
        {
            // Bulid a collection of all of the goal positions
            if (keyAmount == 0 && !keysSaved)
            {
                keyLocations.Clear();
                foreach (GameObject g in controller.objectList)
                {
                    string objName = g.name.ToUpper();
                    // If the object is a gameobject, get the position properties
                    if (objName.IndexOf("OBJECT") >= 0)
                    {
                        if ((g.GetComponentsInChildren<objectProperties>())[0].isKey)
                        {
                            keyLocations.Add(new Vector3(Mathf.RoundToInt(g.transform.position.x), Mathf.RoundToInt(g.transform.position.y), Mathf.RoundToInt(g.transform.position.z)), g);
                            keyAmount = keyAmount + 1;
                        }
                    }

                }

                keysSaved = true;
            }

            else
            {
                foreach (GameObject g in controller.objectList)
                {
                    string objName = g.name.ToUpper();
                    // If the object is a gameobject, get the position properties
                    if (objName.IndexOf("OBJECT") >= 0)
                        if ((g.GetComponentsInChildren<objectProperties>())[0].controllable)
                            if (keyLocations.ContainsKey(new Vector3(Mathf.RoundToInt(g.transform.position.x), Mathf.RoundToInt(g.transform.position.y), Mathf.RoundToInt(g.transform.position.z))))
                            {
                                (keyLocations[new Vector3(Mathf.RoundToInt(g.transform.position.x), Mathf.RoundToInt(g.transform.position.y), Mathf.RoundToInt(g.transform.position.z))]).transform.position = new Vector3(-10, -10, 0);
                                keyLocations.Remove(new Vector3(Mathf.RoundToInt(g.transform.position.x), Mathf.RoundToInt(g.transform.position.y), Mathf.RoundToInt(g.transform.position.z)));
                                keyAmount = keyAmount - 1;
                            }
                }
            }

            if (keyAmount == 0)
            {
                controller.triggerWin();
                keyLocations.Clear();
                resetObjects();
                keysSaved = false;
            }
        }

        
        // Check if a controllable object ever touches a hostile object if it does, lose
        if (controller.playingGame == true && hostileObjects.Count != 0)
        {
            foreach (GameObject g in controllableObjects)
            {
                foreach (GameObject h in hostileObjects)
                {
                    Vector3 gPosition = new Vector3(Mathf.RoundToInt(g.transform.position.x), g.transform.position.y, Mathf.RoundToInt(g.transform.position.z));
                    Vector3 hPosition = new Vector3(Mathf.RoundToInt(h.transform.position.x), h.transform.position.y, Mathf.RoundToInt(h.transform.position.z));

                    // Compare rounded vectors for exact
                    if (gPosition.x == hPosition.x && gPosition.y == hPosition.y)
                    {
                        controller.triggerLose();
                        resetObjects();
                    }
                }
            }
        }
        
    }
    /**
     * Helper method to create the level identifier
     */
    private void generateID()
    {
        StringBuilder result;
        int type;
        int current;

        // create an ID only if the string is empty
        if (String.IsNullOrEmpty(levelID))
        {
            result = new StringBuilder("", levelIDlength);

            for (int i = 0; i < levelIDlength; i++)
            {
                // get a random number out of 0, 1, 2
                type = UnityEngine.Random.Range(0, 3);

                switch (type)
                {
                    case 0:
                        // generate a random number and append it as a char
                        current = UnityEngine.Random.Range(0, 10); // number between 0 and 9
                        result.Append((char) ('0' + current));
                        break;
                    case 1:
                        // generate a random uppercase letter and append it as a char
                        current = UnityEngine.Random.Range(1, 26) - 1; // letter between A and Z
                        result.Append((char) ('A' + current));
                        break;
                    case 2:
                        // generate a random lowercase letter and append it as a char
                        current = UnityEngine.Random.Range(1, 26) - 1; // letter between a and z
                        result.Append((char) ('a' + current));
                        break;
                }
            }
            levelID = result.ToString();
        }
    }
    /**
     * Helper method of create object to determine what the maximum number of objects is
     */
    public int objectCapacity()
    {
        return ((controller.maxWidth + 1) * (controller.maxHeight + 1));
    }
    /**
     * Helper method to add an object
     */
    public void createObject()
    {
        // if the canvas can fit more objects
        if (controller.objectList.Count <= objectCapacity())
        {
            GameObject newObject = (GameObject)Instantiate(Object) as GameObject;
            float x = UnityEngine.Random.Range(0 + 1 + paddingx, controller.maxWidth + 3);
            float y = UnityEngine.Random.Range(0 + paddingy - 1, controller.maxHeight + 1);

            // make objects not land on top of each other
            int i = 0;
            while (i < controller.objectList.Count)
            {
                GameObject obj = controller.objectList[i];

                if (obj.transform.position.x == x && obj.transform.position.y == y)
                {
                    // reroll
                    x = UnityEngine.Random.Range(0 + 1 + paddingx, controller.maxWidth + 3);
                    y = UnityEngine.Random.Range(0 + paddingy - 1, controller.maxHeight + 1);

                    // start over
                    i = 0;
                }

                i++;
            }

            controller.objectTotal++;
            newObject.name = "Object" + controller.objectTotal;
            newObject.transform.position = new Vector2(x, y);
            controller.objectList.Add(newObject);
        }
        else
        {
            // create UI to tell the user they cannot create more objects
            objectError = true;
        }

    }
    public void deleteAllObjects()
    {
        objectProperties obj;

        while (controller.objectList.Count > 0)
        {
            // delete the first object
            obj = (controller.objectList[0]).transform.GetChild(0).GetComponent<objectProperties>();
            obj.deleteObject();
        }

        controller.objectTotal = 0; // reset total
    }
    public Vector3 roundVector3(Vector3 vector)
    {
        vector.x = (float) System.Math.Round((double)vector.x, 0);
        vector.y = (float)System.Math.Round((double)vector.y, 0);
        vector.z = (float)System.Math.Round((double)vector.z, 0);

        return vector;
    }
    // Save the properties of each object before playing the game
    public void saveObjects()
    {

        objectProperties objprop;

        foreach (GameObject obj in controller.objectList)
        {
            objprop = obj.transform.GetChild(0).GetComponent<objectProperties>();

            // save the posiitons of all objects
            objprop.oldPosition = roundVector3(obj.transform.position);
            objprop.oldPositionSprite = roundVector3(obj.transform.GetChild(0).transform.position);

            // save the scale of all objects
            objprop.oldScale = roundVector3(obj.transform.GetChild(0).transform.localScale);
        }
    }
    // resets the position of objects as playinggame is set to false
    public void resetObjects()
    {
        objectProperties objProp;

        foreach (GameObject obj in controller.objectList)
        {
            objProp = obj.transform.GetChild(0).GetComponent<objectProperties>();

            // move all objects back
            obj.transform.position = objProp.oldPosition;
            obj.transform.GetChild(0).transform.position = objProp.oldPositionSprite;

            // reset rotation
            obj.transform.rotation = Quaternion.identity;
            obj.transform.GetChild(0).transform.rotation = Quaternion.identity;

            // reset velocity to 0
            obj.transform.GetChild(0).GetComponent<Rigidbody2D>().velocity = new Vector3(0, 0, 0);

            // reset the scale of the objects
            obj.transform.GetChild(0).transform.localScale = objProp.oldScale;
        }
    }

    // Play specified level
    // @level: path to level
    /*
     * Will be more polished
    void startLevelPlay(string levelPath) {
        // Load the level


    }
    */

    void OnGUI()
    {
        /*
        // TODO: delete these two buttons, exists only to test disabling gui
        if (controller.showGUI && GUILayout.Button("Hide GUI"))
        {
            controller.showGUI = false;
        }
        if(!controller.showGUI && GUILayout.Button("Show GUI"))
        {
            controller.showGUI = true;
        }
        // TODO: delete these buttons
        */

        if (controller.showGUI && !controller.playingGame && !(playObj.GetComponent<playTrack>().getPlay3() || playObj.GetComponent<playTrack>().getPlayLevel()))
        {

            // Create the level Menu
            if (levelMenuToggle)
            {
                int levelMenuWidth = 200;
                int levelMenuHeight = 300;
                GUILayout.BeginArea(new Rect((Screen.width / 2) - (levelMenuWidth / 2), (Screen.height / 2) - (levelMenuHeight / 2), levelMenuWidth, levelMenuHeight), GUI.skin.box);

                GUILayout.BeginHorizontal("box");
                GUILayout.Label("Level Settings");
                if (GUILayout.Button("X"))
                {
                    levelMenuToggle = false;
                }
                GUILayout.EndHorizontal();

                GUILayout.Label("Level Name");
                levelName = GUILayout.TextField(levelName, maxNameLength);

                GUILayout.Label("Win Conditions");
                dontMove = GUILayout.Toggle(dontMove, "Don't Move");
                // Toggle the others
                if (dontMove)
                {
                    collectKeys = false;
                    goToTarget = false;
                }

                collectKeys = GUILayout.Toggle(collectKeys, "Collect Keys");
                // Toggle the others
                if (collectKeys)
                {
                    dontMove = false;
                    goToTarget = false;
                }

                goToTarget = GUILayout.Toggle(goToTarget, "Move to Target");
                // Toggle the others
                if (goToTarget)
                {
                    dontMove = false;
                    collectKeys = false;
                }

                else if (!dontMove && !collectKeys && !goToTarget)
                {
                    dontMove = true;
                }

                GUILayout.Label("Level Time");
                String fromText = GUILayout.TextField((controller.timerStart).ToString(), 3);
                float placeHolder = 0;

                // Check to see if a number before setting
                if (float.TryParse(fromText, out placeHolder)) 
                    controller.timerStart = float.Parse(fromText);

                else if (fromText == "")
                    controller.timerStart = 0;

                GUILayout.Label("Level Instruction");
                instruction = GUILayout.TextField(instruction, maxInstructionLength);

                if (GUILayout.Button("Reset Settings"))
                {
                    if (settingsChanged())
                    {
                        settingsReset = true;
                    }
                }

                GUILayout.EndArea();
            }
            if (settingsReset)
            {
                GUILayout.BeginArea(new Rect((Screen.width / 2) - (210 / 2), (Screen.height / 2) - (60 / 2), 230, 80), GUI.skin.box);

                GUILayout.Label("Are you sure you would like to reset the settings?");
                GUILayout.BeginHorizontal("box");
                if (GUILayout.Button("Confirm"))
                {
                    resetSettings();
                    settingsReset = false; // hide menu
                }
                if (GUILayout.Button("Cancel"))
                {
                    settingsReset = false; // hide menu
                }
                GUILayout.EndHorizontal();
                GUILayout.EndArea();
            }

            if (objectError)
            {
                GUILayout.BeginArea(new Rect((Screen.width / 2) - (210 / 2), (Screen.height / 2) - (60 / 2), 210, 60), GUI.skin.box);
                GUILayout.Label("ERROR: Object capacity reached");
                if (GUILayout.Button("Close"))
                {
                    objectError = false;
                }
                GUILayout.EndArea();

            }
            if (GUI.Button(new Rect(10, Screen.height - 30, 80, 20), "Add Object"))
            {
                createObject();
            }
            if (GUI.Button(new Rect(100, Screen.height - 30, 120, 20), "Delete All Objects"))
            {
                // open the delete objects confirmation box if there are objects to delete
                if (controller.objectList.Count > 0)
                {
                    deleteObjects = true;
                }
            }
            if (deleteObjects)
            {
                GUILayout.BeginArea(new Rect((Screen.width / 2) - (210 / 2), (Screen.height / 2) - (60 / 2), 230, 80), GUI.skin.box);
                
                GUILayout.Label("Are you sure you would like to delete all objects?");
                GUILayout.BeginHorizontal("box");
                if (GUILayout.Button("Confirm"))
                {
                    deleteAllObjects();
                    deleteObjects = false; // hide menu
                }
                if (GUILayout.Button("Cancel"))
                {
                    deleteObjects = false; // hide menu
                }
                GUILayout.EndHorizontal();
                GUILayout.EndArea();
            }
       
            // Button to reset the level
            if (GUI.Button(new Rect(Screen.width - 200, 0, 90, 20), "Reset Level"))
            {
                // if the background or settings properties are changed or are if there are any objects
                if (levelChanged())
                {
                    levelReset = true;
                }
            }
            if (levelReset)
            {
                GUILayout.BeginArea(new Rect((Screen.width / 2) - (210 / 2), (Screen.height / 2) - (60 / 2), 230, 80), GUI.skin.box);

                GUILayout.Label("Are you sure you would like to reset the level?");
                GUILayout.BeginHorizontal("box");
                if (GUILayout.Button("Confirm"))
                {
                    resetLevel();
                    levelReset = false; // hide menu
                }
                if (GUILayout.Button("Cancel"))
                {
                    levelReset = false; // hide menu
                }
                GUILayout.EndHorizontal();
                GUILayout.EndArea();
            }
        }
        if (controller.showGUI && !(playObj.GetComponent<playTrack>().getPlay3() || playObj.GetComponent<playTrack>().getPlayLevel()))
        {
            // Play/Pause Button
            if (GUI.Button(new Rect(Screen.width - 100, 0, 80, 20), buttonSymbol))
            {
                // Change from edit to play
                if (buttonSymbol == "▶")
                {
                    buttonSymbol = "| |";

                    // Make a list of all hostile objects
                    hostileObjects.Clear();
                    foreach (GameObject g in controller.objectList)
                    {
                        if ((g.GetComponentsInChildren<objectProperties>())[0].isHostile)
                        {
                            hostileObjects.Add(g);
                        }

                    }

                    controllableObjects.Clear();
                    foreach (GameObject g in controller.objectList)
                    {
                        if ((g.GetComponentsInChildren<objectProperties>())[0].controllable)
                        {
                            controllableObjects.Add(g);
                        }

                    }

                    saveObjects();

                    // Set game variables
                    keyAmount = 0;
                    keysSaved = false;
                    positionsSaved = false;
                    goalLocations.Clear();
                    startingLocation.Clear();
                    keyLocations.Clear();


                    controller.playingGame = true;
                    Physics2D.autoSimulation = true;

                    // Refresh the background music
                    audioSource = background.GetComponent<AudioSource>();
                    hasSound = background.GetComponent<changeBackground>().hasSound;

                    if (hasSound)
                    {
                        audioSource.Play();
                    }
                }

                // Change from play to edit
                else if (buttonSymbol == "| |")
                {
                    buttonSymbol = "▶";
                    controller.playingGame = false;
                    Physics2D.autoSimulation = false;

                    // reset the position of all objects
                    resetObjects();

                    if (hasSound)
                    {
                        audioSource.Pause();
                    }
                }

            }
        }
       

        // Play instruction
        if (controller.playingGame && (controller.timerStart - controller.timeLeft < 3) && instruction != "")
        {
            int instrWidth = instruction.Length * 9;
            int instrHeight = 25;
            var setCentered = GUI.skin.GetStyle("Label");


            GUILayout.BeginArea(new Rect((Screen.width / 2) - (instrWidth / 2), (Screen.height / 2) - (instrHeight / 2), instrWidth, instrHeight), GUI.skin.box);
            GUILayout.Label(instruction);
            GUILayout.EndArea();
        }

        // Create Timer
        if (controller.hasTimer && controller.playingGame)
        {
            GUILayout.BeginArea(new Rect(Screen.width - 100, 25, 80, 20), GUI.skin.box);
            GUILayout.Label((controller.timeLeft).ToString());
            GUILayout.EndArea();
            trackReset = true;
        }

        // Reset Timer
        if (!(controller.hasTimer && controller.playingGame) && trackReset)
        {
            controller.resetTimer();
            trackReset = false;
        }
    }
    public void resetLevel()
    {
        // reset settings
        resetSettings();

        // clear objects
        deleteAllObjects();

        // reset background
        background.GetComponent<changeBackground>().resetBackground();
    }
    public void resetSettings()
    {
        // reset level name
        levelName = "";

        // reset win condition
        dontMove = true;
        collectKeys = false;
        goToTarget = false;

        // reset level time
        controller.timerStart = controller.initialStartTime;

        // reset level instruction
        instruction = "";
    }
    public bool levelChanged()
    {
        bool result = false;

        if (settingsChanged())
        {
            result = true;
        }else if (background.GetComponent<changeBackground>().changed())
        {
            result = true;
        }else if (controller.objectList.Count > 0)
        {
            result = true;
        }

        return result;
    }
    public bool settingsChanged()
    {
        bool result = false;

        // check if the name changed
        if (levelName != "")
        {
            result = true;
        }
        // check if the win condition changed
        else if (!dontMove)
        {
            result = true;
        }
        // check if the level time changed
        else if (controller.timerStart != controller.initialStartTime)
        {
            result = true;   
        }
        // check if the level instruction changed
        else if (instruction != "")
        {
            result = true;
        }

        return result;
    }

}
