using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
//using System.Runtime.Remoting.Channels;
//using System.Runtime.Remoting.Channels;
using UnityEngine;

public class GUIscript : MonoBehaviour
{
    public GameObject Object;
    private int prefabOffset = 9; // Use for math related to spawning objects. Object0 is located out of bounds (look to the right in the scene editor) in order to be usable as a prototype for creating other objects and is not to be controlled/counted with the other objects.

    public gameMechanics controller;
    string buttonSymbol = "▶";
    bool trackReset = false;
    bool objectError = false;
    bool levelMenuToggle = false;

    private objectProperties objProperties;

    // Variables related to retrieving the background music
    private GameObject background;
    private bool hasSound = false;
    private AudioSource audioSource;

    // Types of win
    private bool dontMove = false;
    private bool collectKeys = false;
    private bool goToTarget = false;

    // Dont move variables
    private bool positionsSaved = false;
    private List<Vector3> startingLocation = new List<Vector3>();
    private List<Vector3> goalLocations = new List<Vector3>();

    // Default Locations to return to
    //List<KeyValuePair<string, Vector3>> defaultLocations = new List<KeyValuePair<string, Vector3>>();

    private String instruction = "";

    // Start is called before the first frame update
    void Start()
    {
        // disable physics during edit mode
        Physics2D.autoSimulation = false;

        controller = GetComponent<gameMechanics>();
        background = GameObject.Find("background");

        // Get locations of all objects
        // Saw that resetPositions was already a thing, keeping as a backup
        /*
        object[] obj = GameObject.FindSceneObjectsOfType(typeof(GameObject));
        foreach (object o in obj)
        {
            GameObject g = (GameObject)o;
            defaultLocations.Add(new KeyValuePair<string, Vector3>(g.name, g.transform.position));
        }
        */
    }

    // Update is called once per frame
    void Update()
    {
        if (controller.playingGame == false && buttonSymbol == "| |")
        {
            buttonSymbol = "▶";
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            if (!levelMenuToggle)
                levelMenuToggle = true;

            else
                levelMenuToggle = false;

            print("Toggled");
        }

        if (controller.timeLeft < 0)
        {
            controller.playingGame = false;
            controller.triggerLose();
            Physics2D.autoSimulation = false;
            resetObjects(); // reset all objects
        }

        // Win condition is not to move
        if (controller.playingGame == true && dontMove)
        {
            if (!positionsSaved)
            {
                object[] obj = GameObject.FindSceneObjectsOfType(typeof(GameObject));
                foreach (object o in obj)
                {
                    GameObject g = (GameObject)o;
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
            }

            else if (positionsSaved)
            {
                object[] obj = GameObject.FindSceneObjectsOfType(typeof(GameObject));
                foreach (object o in obj)
                {
                    GameObject g = (GameObject)o;
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
                print("Getting Goal Locations");
                object[] obj = GameObject.FindSceneObjectsOfType(typeof(GameObject));
                foreach (object o in obj)
                {
                    GameObject g = (GameObject)o;
                    string objName = g.name.ToUpper();
                    // If the object is a gameobject, get the position properties
                    if (objName.IndexOf("OBJECT") >= 0)
                    {
                        if ((g.GetComponentsInChildren<objectProperties>())[0].isTarget)
                        {
                            goalLocations.Add(g.transform.position);
                            print("Added Location");
                        }
                    }
                            
                }
            }
            
            else
            {
                object[] obj = GameObject.FindSceneObjectsOfType(typeof(GameObject));
                foreach (object o in obj)
                {
                    GameObject g = (GameObject)o;
                    string objName = g.name.ToUpper();
                    // If the object is a gameobject, get the position properties
                    if (objName.IndexOf("OBJECT") >= 0)
                        if ((g.GetComponentsInChildren<objectProperties>())[0].controllable)
                            if (goalLocations.Contains(g.transform.position))
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
            
        }
    }
    /**
     * Helper method to add an object
     */
    public void createObject()
    {
        // if the canvas can fit more objects
        if (controller.objectList.Count <= ((controller.maxWidth - 1) * (controller.maxHeight - 1)))
        {
            GameObject newObject = (GameObject)Instantiate(Object) as GameObject;
            float x = UnityEngine.Random.Range(0 - 16 + 2 + 1, controller.maxWidth - 16 + 2);
            float y = UnityEngine.Random.Range(0 - 3 + 1, controller.maxHeight - 3);

            // make objects not land on top of each other
            int i = 0;
            while (i < controller.objectList.Count)
            {
                GameObject obj = controller.objectList[i];

                if (obj.transform.position.x == x && obj.transform.position.y == y)
                {
                    // reroll
                    x = UnityEngine.Random.Range(0 - 16 + 2 + 1, controller.maxWidth - 16 + 2);
                    y = UnityEngine.Random.Range(0 - 3 + 1, controller.maxHeight - 3);

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
    // Save the properties of each object before playing the game
    void saveObjects()
    {
        objectProperties objprop;

        foreach (GameObject obj in controller.objectList)
        {
            objprop = obj.transform.GetChild(0).GetComponent<objectProperties>();

            // save the posiitons of all objects
            objprop.oldPosition = obj.transform.position;
            objprop.oldPositionSprite = obj.transform.GetChild(0).transform.position;

            // save the scale of all objects
            objprop.oldScale = obj.transform.GetChild(0).transform.localScale;
        }
    }
    // resets the position of objects as playinggame is set to false
    void resetObjects()
    {
        objectProperties objProp;

        foreach (GameObject obj in controller.objectList)
        {
            objProp = obj.transform.GetChild(0).GetComponent<objectProperties>();

            // move all objects back
            obj.transform.position = objProp.oldPosition;
            obj.transform.GetChild(0).transform.position = objProp.oldPositionSprite;

            // reset rotation
            obj.transform.GetChild(0).transform.rotation = Quaternion.identity;

            // reset velocity to 0
            obj.transform.GetChild(0).GetComponent<Rigidbody2D>().velocity = new Vector3(0, 0, 0);

            // reset the scale of the objects
            obj.transform.GetChild(0).transform.localScale = objProp.oldScale;
        }
    }
    void OnGUI()
    {
        if (!controller.playingGame)
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
                controller.timerStart = float.Parse(GUILayout.TextField((controller.timerStart).ToString(), 3));

                GUILayout.Label("Level Instruction");
                instruction = GUILayout.TextField(instruction, 25);

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
        }

        // Play/Pause Button
        if (GUI.Button(new Rect(Screen.width - 100, 0, 80, 20), buttonSymbol))
        {
            // Change from edit to play
            if (buttonSymbol == "▶")
            {
                buttonSymbol = "| |";
                controller.playingGame = true;
                Physics2D.autoSimulation = true;

                // Refresh the background music
                audioSource = background.GetComponent<AudioSource>();
                hasSound = background.GetComponent<changeBackground>().hasSound;

                saveObjects();

                if (hasSound)
                {
                    audioSource.Play();
                }
            }

            // Change from play to edit
            else if (buttonSymbol == "| |")
            {
                goalLocations.Clear();
                startingLocation.Clear();
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

        // Play instruction
        if (controller.playingGame && (controller.timerStart - controller.timeLeft < 3) && instruction != "")
        {
            int instrWidth = instruction.Length * 8;
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
}
