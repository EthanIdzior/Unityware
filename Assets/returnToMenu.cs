using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions; // used to parse text before saving to files
using UnityEditor; // used for the import/export file window
using UnityEngine;
using UnityEngine.SceneManagement; // used to switch scenes
using System.Runtime.InteropServices;
using System.Linq.Expressions;
using System.ComponentModel.Design;

public class returnToMenu : MonoBehaviour
{
    public GameObject Object;

    // Variables used for saving to help compatiblity later on
    int maxProperties = 14; // the number of properties that can be set, used for saving. Change manually as more properties are added

    // Variables related to the menu itself
    int height = 150;
    int width = 200;

    //needed to return to menu
    public string menu;

    private bool menuOpen = true;

    public GUIscript playGUI; // required for playing mode and saving
    public gameMechanics controller; // required to retrieve variables for saving
    public changeBackground background; // required to retrieve background variables for saving
    public objectProperties objZeroProperties;
    public GameObject objZero;

    private bool error = false;
    private String errorMessage = "";

    private bool fileChanged = false;
    private String change = "";
    private String lastFile = "";

    private bool oldLevelChanged = false;

    // Start is called before the first frame update
    private void Start()
    {
        // Retrieve variables from other scripts
        GameObject mainCamera = GameObject.Find("Main Camera");
        playGUI = mainCamera.GetComponent<GUIscript>();
        controller = mainCamera.GetComponent<gameMechanics>();
        background = (GameObject.Find("background")).GetComponent<changeBackground>();
        objZeroProperties = ((GameObject.Find("Object0")).transform.GetChild(0)).GetComponent<objectProperties>();

        // retrieve the prototype gameobject
        Object = GameObject.Find("Object0");
    }

    private void OnGUI()
    {
        if (controller.showGUI && !controller.playingGame)
        {
            if (menuOpen)
            {
                GUILayout.BeginArea(new Rect(Screen.width - width-width, Screen.height - height, width, height), GUI.skin.box);

                GUILayout.BeginHorizontal("box");
                GUILayout.Label("Menu Controls");
                if (GUILayout.Button("_"))
                {
                    menuOpen = false;
                }
                GUILayout.EndHorizontal();

                // Save/Load locally
                GUILayout.BeginHorizontal("box");
                if (GUILayout.Button("Save Level"))
                {
                    change = "saved";
                    saveLevel();
                
                }
                if (GUILayout.Button("Load Level"))
                {
                    change = "loaded";

                    // if level changed, ask the user if they are sure they would like to continue
                    if (playGUI.levelChanged())
                    {
                        oldLevelChanged = true;
                    }
                    else
                    {
                        loadRandomLevel(); // TODO: change to loadLevel once a way to select is implemented
                    }
                }
                GUILayout.EndHorizontal();

                // Save/Load to/from a user defined location
                GUILayout.BeginHorizontal("box");
                if (GUILayout.Button("Export Level"))
                {
                    change = "exported";
                    if (!String.IsNullOrEmpty(playGUI.levelName))
                    {
                        String path;

                        path = EditorUtility.SaveFilePanel(
                            "Export level",
                            "",
                            nameToFileName(playGUI.levelName) + ".txt",
                            "txt");
                        if (path.Length != 0)
                        {
                            saveLevel(path);
                        }
                    }
                    else
                    {
                        error = true;
                        errorMessage = "Level must be named before exporting";
                    }

                }
                if (GUILayout.Button("Import Level"))
                {
                    change = "imported";
                    String path;

                    // get the path of the level
                    path = EditorUtility.OpenFilePanel(
                        "Import a level",
                        "",
                        "txt");

                    if (path.Length != 0)
                    {
                        // verify imported level
                        if (validLevel(path))
                        {
                            // if file does not exist
                            if (!File.Exists("Assets/Saves/" + Path.GetFileName(path)))
                            {
                                // copy file to local saves
                                File.Copy(path, "Assets/Saves/" + Path.GetFileName(path));

                                fileChanged = true;
                                change = "imported";
                                lastFile = path;
                            }
                            // if file exists
                            else
                            {
                                // compare level files to see if the id matches

                                String localSecondLine = "";
                                String importSecondLine = "";
                                bool localHasSecond = true;
                                bool importHasSecond = true;

                                try
                                {
                                    // get the second line for the first file
                                    using (StreamReader localFile = new StreamReader("Assets/Saves/" + Path.GetFileName(path)))
                                    {
                                        localFile.ReadLine(); // Skip first line
                                        localSecondLine = localFile.ReadLine();
                                        // if something goes wrong reading these lines
                                    }
                                }
                                catch (IOException)
                                {
                                    localHasSecond = false;
                                }

                                // get the second line for the second file if the first was successful
                                if (localHasSecond)
                                {
                                    try
                                    {
                                        using (StreamReader importFile = new StreamReader(path))
                                        {
                                            importFile.ReadLine(); // Skip first line
                                            importSecondLine = importFile.ReadLine();
                                        }
                                    }
                                    catch (IOException)
                                    {
                                        importHasSecond = false;
                                    }

                                }

                                if (localHasSecond && importHasSecond)
                                {
                                    // if the id of both levels is the same
                                    if (String.Equals(localSecondLine, importSecondLine))
                                    {
                                        error = true;
                                        errorMessage = "This level already exists";
                                    }
                                }
                                else
                                {
                                    error = true;
                                    errorMessage = "A level already exists with the same name";
                                }
                            }
                        }
                        else
                        {
                            error = true;
                        }
                    }
                }
                GUILayout.EndHorizontal();

                if (GUILayout.Button("Return To Menu"))
                {
                    SceneManager.LoadScene(menu);
                }


                GUILayout.EndArea();
            }
            else if (!playGUI.controller.playingGame)
            {
                GUILayout.BeginArea(new Rect(Screen.width - 40-width, Screen.height - 30, 40, 30), GUI.skin.box);
                if (GUILayout.Button("_"))
                {
                    menuOpen = true;
                }
                GUILayout.EndArea();
            }
            if (error)
            {
                // display error messages on the user end. ex: level needs to be named, etc
                GUILayout.BeginArea(new Rect((Screen.width / 2) - (230 / 2), (Screen.height / 2) - (120 / 2), 230, 120), GUI.skin.box);
                GUILayout.Label("ERROR: " + errorMessage);
                if (GUILayout.Button("Close"))
                {
                    error = false;
                }
                GUILayout.EndArea();
            }
            if (fileChanged)
            {
                // Display a message saying that the level was saved successfully
                GUILayout.BeginArea(new Rect((Screen.width / 2) - (400 / 2), (Screen.height / 2) - (60 / 2), 400, 60), GUI.skin.box);
                GUILayout.Label("Level at path " + lastFile + " " + change + " successfully");
                if (GUILayout.Button("Close"))
                {
                    fileChanged = false;
                }
                GUILayout.EndArea();
            }
            if (oldLevelChanged)
            {
                GUILayout.BeginArea(new Rect((Screen.width / 2) - (400 / 2), (Screen.height / 2) - (60 / 2), 400, 80), GUI.skin.box);

                GUILayout.Label("Would you like to save the current level before loading another?");
                GUILayout.BeginHorizontal("box");
                if (GUILayout.Button("Save and Load"))
                {
                    saveAndLoad();
                    oldLevelChanged = false; // hide menu
                }
                if (GUILayout.Button("Load without Saving"))
                {
                    loadRandomLevel(); // TODO: change to loadLevel when a way to select a level is implemented
                    oldLevelChanged = false; // hide menu
                }
                if (GUILayout.Button("Cancel"))
                {
                    oldLevelChanged = false; // hide menu
                }
                GUILayout.EndHorizontal();
                GUILayout.EndArea();
            }
        }
    }
    /**
     * Helper method to ensure the name is valid
     */
    public bool validName(String currentLine)
    {
        String key = "name";
        // verify first half of line
        if (hasKey(currentLine, key))
        {
            // strip key
            currentLine = currentLine.Substring(key.Length + 1);

            // check if name is empty or null
            if (String.IsNullOrEmpty(currentLine))
                return false;

            if (currentLine.Length > playGUI.maxNameLength)
                return false;

            return true;
        }
        else
            return false;
    }
    public bool validID(String currentLine)
    {
        String key = "id";
        // verify second line has key
        if (hasKey(currentLine, key))
        {
            // strip key
            currentLine = currentLine.Substring(key.Length + 1);

            // check if key is null, does not have the correct length, or contains non letters and numbers
            if (String.IsNullOrEmpty(currentLine) || currentLine.Length != playGUI.levelIDlength || !Regex.IsMatch(currentLine, @"^[a-zA-Z0-9]+$"))
                return false;

            return true;
        }
        else
            return false;
    }
    public bool validWin(String currentLine)
    {
        String key = "win";
        if (hasKey(currentLine, key))
        {
            currentLine = currentLine.Substring(key.Length + 1);

            if (!isPositiveBelowEnd(currentLine, key, playGUI.winConditions - 1))
                return false;

            return true;
        }
        else
            return false;
    }
    public bool validTime(String currentLine)
    {
        String key = "time";
        if (hasKey(currentLine, key))
        {
            currentLine = currentLine.Substring(key.Length + 1);
            if (!isPositiveInt(currentLine, key))
                return false;

            return true;
        }
        else
            return false;
    }
    public bool validInstruction(String currentLine)
    {
        String key = "instruction";
        if (hasKey(currentLine, key))
        {
            currentLine = currentLine.Substring(key.Length + 1);
            if (currentLine.Length > playGUI.maxInstructionLength)
                return false;

            return true;
        }
        else
            return false;
    }
    public bool validObjectTotal(String currentLine, ref int objTotal)
    {
        String key = "objectTotal";
        if (hasKey(currentLine, key))
        {
            currentLine = currentLine.Substring(key.Length + 1);
            if (!isPositiveBelowEnd(currentLine, key, playGUI.objectCapacity()))
                return false;

            objTotal = int.Parse(currentLine); // store total for further verification later
            return true;
        }
        else
            return false;
    }
    public bool validObjectNum(String currentLine, int objTotal, ref int objNum)
    {
        String key = "objectNum";
        if (hasKey(currentLine, key))
        {
            currentLine = currentLine.Substring(key.Length + 1);
            if (!isPositiveBelowOrEqualToEnd(currentLine, key, objTotal))
                return false;

            objNum = int.Parse(currentLine); // store number of objects for further verification later
            return true;
        }
        else
            return false;
    }
    public bool validMaxProperties(String currentLine, ref int maxProperties)
    {
        String key = "maxProperties";
        if (hasKey(currentLine, key))
        {
            currentLine = currentLine.Substring(key.Length + 1);
            if (!isPositiveInt(currentLine, key))
                return false;

            maxProperties = int.Parse(currentLine); // store the number of properties an object can have for further verification later
            return true;
        }
        else
            return false;
    }
    public bool validMaxWidth(String currentLine, ref int maxWidth)
    {
        String key = "maxWidth";
        if (hasKey(currentLine, key))
        {
            currentLine = currentLine.Substring(key.Length + 1);

            if (!isPositiveInt(currentLine, key))
                return false;

            maxWidth = int.Parse(currentLine);
            return true;
        }
        else
            return false;
    }
    public bool validMaxHeight(String currentLine, ref int maxHeight)
    {
        String key = "maxHeight";
        if (hasKey(currentLine, key))
        {
            currentLine = currentLine.Substring(key.Length + 1);

            if (!isPositiveInt(currentLine, key))
                return false;

            maxHeight = int.Parse(currentLine);
            return true;
        }
        else
            return false;
    }
    public bool validBgSpriteIndex(String currentLine)
    {
        String key = "bgSpriteIndex";
        if (hasKey(currentLine, key))
        {
            currentLine = currentLine.Substring(key.Length + 1);

            // ensure that it is below the array size
            if (!isPositiveBelowEnd(currentLine, key, background.backgroundSprites.Length))
                return false;

            return true;
        }
        else
            return false;
    }
    public bool validBgColorIndex(String currentLine)
    {
        String key = "bgColorIndex";
        if (hasKey(currentLine, key))
        {
            currentLine = currentLine.Substring(key.Length + 1);

            // ensure that it is below the array size
            if (!isPositiveBelowEnd(currentLine, key, background.colors.Length))
                return false;

            return true;
        }
        else
            return false;
    }
    public bool validBgHasMusic(String currentLine)
    {
        String key = "bgHasMusic";
        if (hasKey(currentLine, key))
        {
            currentLine = currentLine.Substring(key.Length + 1);

            // ensure that it is a valid boolean
            if (!isIntBoolean(currentLine, key))
                return false;

            return true;
        }
        else
            return false;
    }
    public bool validBgMusicIndex(String currentLine)
    {
        String key = "bgMusicIndex";
        if (hasKey(currentLine, key))
        {
            currentLine = currentLine.Substring(key.Length + 1);

            // ensure that it is below the array size
            if (!isPositiveBelowEnd(currentLine, key, background.backgroundMusic.Length))
                return false;

            return true;
        }
        else
            return false;
    }
    public bool validObjName(String currentLine, int i)
    {
        String key = "objName";
        if (hasKey(currentLine, key))
        {
            currentLine = currentLine.Substring(key.Length + 1);

            String name = "Object" + (i + 1);

            // ensure that the name matches the format
            if (!String.Equals(currentLine, name))
                return false;

            return true;
        }
        else
            return false;
    }
    public bool validObjPositionX(String currentLine, int maxWidth)
    {
        String key = "objPositionX";
        if (hasKey(currentLine, key))
        {
            currentLine = currentLine.Substring(key.Length + 1);

            // ensure that it is an integer
            if (!isIntWithinRange(currentLine, key, 2, maxWidth + 2))
                return false;

            return true;
        }
        else
            return false;
    }
    public bool validObjPositionY(String currentLine, int maxHeight)
    {
        String key = "objPositionY";
        if (hasKey(currentLine, key))
        {
            currentLine = currentLine.Substring(key.Length + 1);

            // ensure that it is an integer
            if (!isIntWithinRange(currentLine, key, 0, maxHeight))
                return false;

            return true;
        }
        else
            return false;
    }
    public bool validObjPositionZ(String currentLine)
    {
        String key = "objPositionZ";
        if (hasKey(currentLine, key))
        {
            currentLine = currentLine.Substring(key.Length + 1);

            int value;
            // if null/empty or not integer
            if (String.IsNullOrEmpty(currentLine) || !int.TryParse(currentLine, out value))
                return false;

            return true;
        }
        else
            return false;
    }
    public bool validObjRotation(String currentLine, String letter)
    {
        String key = "objRotation" + letter;
        if (hasKey(currentLine, key))
        {
            currentLine = currentLine.Substring(key.Length + 1);

            int value;
            // check if it is not zero, as the rotation should only change in play mode
            if (String.IsNullOrEmpty(currentLine) || (!int.TryParse(currentLine, out value) || value != 0))
                return false;

            return true;
        }
        else
        {
            return false;
        }
    }
    public bool validObjScale(String currentLine, String letter)
    {
        String key = "objScale" + letter;
        if (hasKey(currentLine, key))
        {
            currentLine = currentLine.Substring(key.Length + 1);

            int value;
            // check if it is not one, as the rotation should only change in play mode
            if (String.IsNullOrEmpty(currentLine) || (!int.TryParse(currentLine, out value) || value != 1))
                return false;

            return true;
        }
        else
        {
            return false;
        }
    }
    public bool validObjDraggable(String currentLine)
    {
        String key = "objDraggable";
        if (hasKey(currentLine, key))
        {
            currentLine = currentLine.Substring(key.Length + 1);

            if (!isIntBoolean(currentLine, key))
                return false;

            return true;
        } else
            return false;
    }
    public bool validObjClickable (String currentLine)
    {
        String key = "objClickable";
        if (hasKey(currentLine, key))
        {
            currentLine = currentLine.Substring(key.Length + 1);

            if (!isIntBoolean(currentLine, key))
                return false;

            return true;
        }
        else
            return false;
    }
    public bool validObjSpace(String currentLine)
    {
        String key = "objSpace";
        if (hasKey(currentLine, key))
        {
            currentLine = currentLine.Substring(key.Length + 1);

            if (!isIntBoolean(currentLine, key))
                return false;

            return true;
        }
        else
            return false;
    }
    public bool validObjGravity(String currentLine, ref bool gravity)
    {
        String key = "objGravity";
        if (hasKey(currentLine, key))
        {
            currentLine = currentLine.Substring(key.Length + 1);

            if (!isIntBoolean(currentLine, key))
                return false;

            gravity = stringToBool(currentLine);

            return true;
        }
        else
            return false;
    }
    public bool validObjImmobile(String currentLine, bool gravity, ref bool immobile)
    {
        String key = "objImmobile";
        if (hasKey(currentLine, key))
        {
            currentLine = currentLine.Substring(key.Length + 1);

            if (!isIntBoolean(currentLine, key))
                return false;

            immobile = stringToBool(currentLine);

            // both cannot be active at the same time
            if (immobile && gravity)
                return false;
            
            return true;
        }
        else
            return false;
    }
    public bool validObjSolid(String currentLine, ref bool solid)
    {
        String key = "objSolid";
        if (hasKey(currentLine, key))
        {
            currentLine = currentLine.Substring(key.Length + 1);

            if (!isIntBoolean(currentLine, key))
                return false;

            solid = stringToBool(currentLine);
            return true;
        }
        else
            return false;
    }
    public bool validObjGoal(String currentLine, bool immobile, ref bool goal)
    {
        String key = "objGoal";
        if (hasKey(currentLine, key))
        {
            currentLine = currentLine.Substring(key.Length + 1);

            if (!isIntBoolean(currentLine, key))
                return false;

            goal = stringToBool(currentLine);

            // for goal to be true immobile must also be true
            if (goal)
            {
                if (!immobile)
                {
                    return false;
                }
            }

            return true;
        }
        else
            return false;
    }
    public bool validObjKey(String currentLine, bool immobile, ref bool objKey)
    {
        String key = "objKey";
        if (hasKey(currentLine, key))
        {
            currentLine = currentLine.Substring(key.Length + 1);

            if (!isIntBoolean(currentLine, key))
                return false;

            objKey = stringToBool(currentLine);

            // both cannot be active at the same time
            if (immobile && objKey)
                return false;

            return true;
        }
        else
            return false;
    }
    public bool validObjControllable(String currentLine, bool goal, bool objKey, ref bool controllable)
    {
        String key = "objControllable";
        if (hasKey(currentLine, key))
        {
            currentLine = currentLine.Substring(key.Length + 1);

            if (!isIntBoolean(currentLine, key))
                return false;

            controllable = stringToBool(currentLine);

            // controllable cannot be active at the same time as goal or key
            if ((goal && controllable) || (objKey && controllable))
                return false;

            return true;
        }
        else
            return false;
    }
    public bool validObjSpriteIndex(String currentLine)
    {
        String key = "objSpriteIndex";
        if (hasKey(currentLine, key))
        {
            currentLine = currentLine.Substring(key.Length + 1);

            // ensure that it is below the array size
            if (!isPositiveBelowEnd(currentLine, key, objZeroProperties.objectSprites.Length))
                return false;

            return true;
        }
        else
            return false;
    }
    public bool validObjColorIndex(String currentLine)
    {
        String key = "objColorIndex";
        if (hasKey(currentLine, key))
        {
            currentLine = currentLine.Substring(key.Length + 1);

            // ensure that it is below the array size
            if (!isPositiveBelowEnd(currentLine, key, objZeroProperties.colors.Length))
                return false;

            return true;
        }
        else
            return false;
    }
    public bool validObjHasSound(String currentLine)
    {
        String key = "objHasSound";
        if (hasKey(currentLine, key))
        {
            currentLine = currentLine.Substring(key.Length + 1);

            if (!isIntBoolean(currentLine, key))
                return false;

            return true;
        }
        else
            return false;
    }
    public bool validobjSoundIndex(String currentLine)
    {
        String key = "objSoundIndex";

        if (hasKey(currentLine, key))
        {
            currentLine = currentLine.Substring(key.Length + 1);

            // ensure that it is below the array size
            if (!isPositiveBelowEnd(currentLine, key, objZeroProperties.audioClips.Length))
                return false;

            return true;
        }
        else
            return false;
           
    }
    public bool validObjSoundIndex(String currentLine)
    {
        String key = "objSoundIndex";
        if (hasKey(currentLine, key))
        {
            currentLine = currentLine.Substring(key.Length + 1);

            if (!isIntBoolean(currentLine, key))
                return false;

            return true;
        }
        else
            return false;
    }
    public bool validObjHostile(String currentLine, bool controllable, bool immobile, bool solid, bool objKey, bool goal)
    {
        String key = "objHostile";
        if (hasKey(currentLine, key))
        {
            currentLine = currentLine.Substring(key.Length + 1);

            if (!isIntBoolean(currentLine, key))
                return false;

            bool hostile = stringToBool(currentLine);

            // check if when hostile is true controllable, immobile, solid, key, and goal should be false
            if (hostile)
            {
                if (controllable || immobile || solid || objKey || goal)
                {
                    return false;
                }
            }

            return true;
        }
        else
            return false;
    }
    /**
     * Method to ensure that levels are valid
     */
    public bool validLevel(String path)
    {
        bool result = true;
        String currentLine = "";

        int objTotal = 0;
        int objNum = 0;
        int fileProperties = 0;
        int maxWidth = 0;
        int maxHeight = 0;
        bool gravity = false;
        bool immobile = false;
        bool controllable = false;
        bool solid = false;
        bool goal = false;
        bool key = false;

        try
        {
            // check if level is valid
            using (StreamReader file = new StreamReader(path))
            {
                // verify level properties

                // verify name
                currentLine = file.ReadLine();
                if (!validName(currentLine))
                {
                    levelError("name");
                    return false;
                }
                    

                // verify id
                currentLine = file.ReadLine();
                if (!validID(currentLine))
                {
                    levelError("id");
                    return false;
                }
                
                // verify win condition
                currentLine = file.ReadLine();
                if (!validWin(currentLine))
                {
                    levelError("win condition");
                    return false;
                }

                // verify time
                currentLine = file.ReadLine();
                if (!validTime(currentLine))
                {
                    levelError("time");
                    return false;
                }

                // verify instruction
                currentLine = file.ReadLine();
                if (!validInstruction(currentLine))
                {
                    levelError("instruction");
                    return false;
                }

                // verify objectTotal
                currentLine = file.ReadLine();
                if (!validObjectTotal(currentLine, ref objTotal))
                {
                    levelError("object total");
                    return false;
                }

                // verify objectNum
                currentLine = file.ReadLine();
                if (!validObjectNum(currentLine, objTotal, ref objNum))
                {
                    levelError("object number");
                    return false;
                }

                // verify maxProperties
                currentLine = file.ReadLine();
                if (!validMaxProperties(currentLine, ref fileProperties))
                {
                    levelError("max properties");
                    return false;
                }

                // verify maxWidth
                currentLine = file.ReadLine();
                if (!validMaxWidth(currentLine, ref maxWidth))
                {
                    levelError("max width");
                    return false;
                }

                // verify maxHeight
                currentLine = file.ReadLine();
                if (!validMaxHeight(currentLine, ref maxHeight))
                {
                    levelError("max height");
                    return false;
                }

                // verify background properties
                // verify bgSpriteIndex
                currentLine = file.ReadLine();
                if (!validBgSpriteIndex(currentLine))
                {
                    backgroundError("sprite index");
                    return false;
                }

                // verify bgColorIndex
                currentLine = file.ReadLine();
                if (!validBgColorIndex(currentLine))
                {
                    backgroundError("color index");
                    return false;
                }

                // verify bgHasMusic
                currentLine = file.ReadLine();
                if (!validBgHasMusic(currentLine))
                {
                    backgroundError("has music");
                    return false;
                }

                // verify bgMusicIndex
                currentLine = file.ReadLine();
                if (!validBgMusicIndex(currentLine))
                {
                    backgroundError("music index");
                    return false;
                }

                // verify object properties
                // add for loop once it works for one object
                for (int i = 0; i < objNum; i++)
                {
                    if (i == 0) // read first run, otherwise will read at the end of the loop
                    {
                        currentLine = file.ReadLine();
                    }

                    // verify objName
                    if (!validObjName(currentLine, i))
                    {
                        objectError("name", i);
                        return false;
                    }

                    // verify objPositionX
                    currentLine = file.ReadLine();
                    if (!validObjPositionX(currentLine, maxWidth))
                    {
                        objectError("position x", i);
                        return false;
                    }

                    // verify objPositionY
                    currentLine = file.ReadLine();
                    if (!validObjPositionY(currentLine, maxHeight))
                    {
                        objectError("position y", i);
                        return false;
                    }

                    // verify objPositionZ
                    currentLine = file.ReadLine();
                    if (!validObjPositionZ(currentLine))
                    {
                        objectError("position z", i);
                        return false;
                    }

                    // verify objRotationX
                    currentLine = file.ReadLine();
                    if (!validObjRotation(currentLine, "X"))
                    {
                        objectError("rotation x", i);
                        return false;
                    }

                    // verify objRotationY
                    currentLine = file.ReadLine();
                    if (!validObjRotation(currentLine, "Y"))
                    {
                        objectError("rotation y", i);
                        return false;
                    }

                    // verify objRotationZ
                    currentLine = file.ReadLine();
                    if (!validObjRotation(currentLine, "Z"))
                    {
                        objectError("rotation z", i);
                        return false;
                    }

                    // verify objScaleX
                    currentLine = file.ReadLine();
                    if (!validObjScale(currentLine, "X"))
                    {
                        objectError("scale x", i);
                        return false;
                    }

                    // verify objScaleY
                    currentLine = file.ReadLine();
                    if (!validObjScale(currentLine, "Y"))
                    {
                        objectError("scale y", i);
                        return false;
                    }

                    // verify objScaleZ
                    currentLine = file.ReadLine();
                    if (!validObjScale(currentLine, "Z"))
                    {
                        objectError("scale z", i);
                        return false;
                    }

                    // verify objDraggable
                    currentLine = file.ReadLine();
                    if (!validObjDraggable(currentLine))
                    {
                        objectError("draggable", i);
                        return false;
                    }

                    // verify objClickable
                    currentLine = file.ReadLine();
                    if (!validObjClickable(currentLine))
                    {
                        objectError("clickable", i);
                        return false;
                    }

                    // verify objSpace
                    currentLine = file.ReadLine();
                    if (!validObjSpace(currentLine))
                    {
                        objectError("space", i);
                        return false;
                    }

                    // verify objGravity
                    currentLine = file.ReadLine();
                    if (!validObjGravity(currentLine, ref gravity))
                    {
                        objectError("gravity", i);
                        return false;
                    }

                    // verify objImmobile
                    currentLine = file.ReadLine();
                    if (!validObjImmobile(currentLine, gravity, ref immobile))
                    {
                        objectError("immobile", i);
                        return false;
                    }

                    // verify objSolid
                    currentLine = file.ReadLine();
                    if (!validObjSolid(currentLine, ref solid))
                    {
                        objectError("solid", i);
                        return false;
                    }

                    // verify objGoal
                    currentLine = file.ReadLine();
                    if (!validObjGoal(currentLine, immobile, ref goal))
                    {
                        objectError("goal", i);
                        return false;
                    }

                    // verify objKey
                    currentLine = file.ReadLine();
                    if (!validObjKey(currentLine, immobile, ref key))
                    {
                        objectError("key", i);
                        return false;
                    }

                    // verify objControllable
                    currentLine = file.ReadLine();
                    if (!validObjControllable(currentLine, goal, key, ref controllable))
                    {
                        objectError("controllable", i);
                        return false;
                    }

                    // verify objSpriteIndex
                    currentLine = file.ReadLine();
                    if (!validObjSpriteIndex(currentLine))
                    {
                        objectError("sprite index", i);
                        return false;
                    }

                    // verify objColorIndex
                    currentLine = file.ReadLine();
                    if (!validObjColorIndex(currentLine))
                    {
                        objectError("color index", i);
                        return false;
                    }

                    // verify objHasSound
                    currentLine = file.ReadLine();
                    if (!validObjHasSound(currentLine))
                    {
                        objectError("has sound", i);
                        return false;
                    }

                    // verify objSoundIndex
                    currentLine = file.ReadLine();
                    if (!validObjSoundIndex(currentLine))
                    {
                        objectError("sound index", i);
                        return false;
                    }

                    if (fileProperties >= 14)
                    {
                        // verify objHostile
                        currentLine = file.ReadLine();
                        if (!validObjHostile(currentLine, controllable, immobile, solid, key, goal))
                        {
                            objectError("hostile", i);
                            return false;
                        }
                    }

                    String nameKey = "objName";
                    // while the next line is not a new object
                    while (i + 1 != objNum && !hasKey(currentLine, nameKey))
                    {
                        // if there are no more lines
                        if (file.Peek() == -1)
                        {
                            objectError("number of lines", i);
                            return false;
                        }

                        // read in the next line
                        currentLine = file.ReadLine();
                    }
                }
            }
        }
        catch (IOException)
        {
            return false;
        }

        return result;
    }
    private void levelError(String variable)
    {
        error = true;
        errorMessage = "Error when level was " + change + ". " + "Level " + variable + " is not valid.";
    }
    private void backgroundError(String variable)
    {
        error = true;
        errorMessage = "Error when level was " + change + ". " + "Level background " + variable + " is not valid.";
    }
    private void objectError(String variable, int i)
    {
        error = true;
        errorMessage = "Error when level was " + change + ". " + "Object " + variable + " for object" + (i + 1) + " is not valid.";
    }
    /**
     * Helper method for validLevel, checks if the key is 0 or 1
     */
    private bool isIntBoolean(String currentLine, String key)
    {
        return isPositiveBelowOrEqualToEnd(currentLine, key, 1);
    }
    /**
     * Helper method for validLevel, checks if key is an integer between 0 and end
     */
    private bool isPositiveBelowEnd(String currentLine, String key, int end)
    {
        return isIntWithinRange(currentLine, key, 0, end - 1);
    }
    private bool isPositiveBelowOrEqualToEnd(String currentLine, String key, int end)
    {
        return isIntWithinRange(currentLine, key, 0, end);
    }
    /**
     * Helper method for validLevel, checks if the key is an integer within the range of start and end
     */
    private bool isIntWithinRange(String currentLine, String key, int start, int end)
    {
        int value;
        // if null/empty or not integer
        if (String.IsNullOrEmpty(currentLine) || !int.TryParse(currentLine, out value))
        {
            return false;
        }
        else
        {
            // if value is outside of the range
            if (value < start || value > end)
            {
                return false;
            }
        }
        return true;
    }
    /**
     * Helper method for validLevel, checks if the key is a positive int
     */
    private bool isPositiveInt(String currentLine, String key)
    {
        int value;
        // if null/empty or not integer
        if (String.IsNullOrEmpty(currentLine) || !int.TryParse(currentLine, out value))
        {
            return false;
        }
        else
        {
            // if value is outside of the range
            if (value < 0)
            {
                return false;
            }
        }
        return true;
    }
    /**
     * Helper method for validLevel, checks if the string is in the format of [key]:[value]
     */
    private bool hasKey(String line, String key)
    {
        String[] parts = new String[] { "", "" };

        parts = line.Split(new[] { ':' }, 2);
        if (String.Equals(parts[0], key))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /**
     * Method to replace unwanted characters for filenames
     */
    private String nameToFileName(String name)
    {
        // get the desired file path, which is the level name with certain letters stripped or replaced
        name = Regex.Replace(name, " ", "-"); // replace space with dashes
        name = Regex.Replace(name, "[\\\\/:*\"<>|.]", ""); // strip \, /, :, *, ", <, >, |, and .

        return name;
    }
    /**
     * Helper method to write a boolean as a string
     */
    private String boolToString(bool boolean)
    {
        if (boolean)
        {
            return "1";
        }
        else
        {
            return "0";
        }
    }
    /**
     * Helper method to write a string to a boolean
     */
    private bool stringToBool(String str)
    {
        if (str == "1")
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    /**
    * Method to save the levels locally, gets a filename from the level name before passing it to the overloaded method
    */
    private bool saveLevel()
    {
        String fileName = playGUI.levelName;

        fileName = nameToFileName(fileName);
        fileName += ".txt";
        fileName = "Assets/Saves/" + fileName; // add subfolder the saves go to for the full path

        return saveLevel(fileName);
    }
    /**
     * Method to save levels given the path
     */
    private bool saveLevel(String path)
    {
        if (String.IsNullOrEmpty(playGUI.levelName))
        {
            error = true;
            errorMessage = "Level must be named before saving";
            return false;
        }

        // check if the file exists
        if (File.Exists(path))
        {
            // TODO: optional, implement a way to save over
            // If so check if it has the same level id
                // if no, throw an error stating that the level already exists and return
        }
        // set up the file to save to
        StreamWriter save = File.CreateText(path);

        // save.WriteLine("Test");

        // save values in the form name:value, ex solid:0. This is to allow older saves to be compatible as we can just set newer properties to default values if the solid descriptor is not found

        // Begin writing to the file line by line


        // Save level settings
            // Level name
            save.WriteLine("name:" + playGUI.levelName); 
            // level id
            save.WriteLine("id:" + playGUI.levelID); 
            // Win condition (as int)
            save.Write("win:");
            if (playGUI.dontMove)
            {
                save.Write("0");
            }else if (playGUI.collectKeys)
            {
                save.Write("1");
            }else if (playGUI.goToTarget)
            {
                save.Write("2");
            }
            save.Write("\n");
            // level time
            save.WriteLine("time:" + controller.timerStart);
            // level instruction
            save.WriteLine("instruction:" + playGUI.instruction);


        // Save general background/object properties
            // object Total (next int index for new objects, total objects created)
            save.WriteLine("objectTotal:" + controller.objectTotal);
            // current number of objects
            save.WriteLine("objectNum:" + controller.objectList.Count);
            // number of properties per object
            save.WriteLine("maxProperties:" + maxProperties);
            // max width
            save.WriteLine("maxWidth:" + controller.maxWidth);
            // max height
            save.WriteLine("maxHeight:" + controller.maxHeight);


        // background properties
        // sprite index
        save.WriteLine("bgSpriteIndex:" + background.backgroundSpriteIndex);
        // color index
        save.WriteLine("bgColorIndex:" + background.colorIndex);
        // music bool (0 or 1)
        save.WriteLine("bgHasMusic:" + boolToString(background.hasSound));
        // music index
        save.WriteLine("bgMusicIndex:" + background.backgroundMusicIndex);

        objectProperties objprop;

        // object properties (per object, wrap in for loop)
        foreach (GameObject obj in controller.objectList)
        {
            objprop = obj.transform.GetChild(0).GetComponent<objectProperties>();

            // object name
            save.WriteLine("objName:" + obj.name);
            // save position
            save.WriteLine("objPositionX:" + obj.transform.position.x);
            save.WriteLine("objPositionY:" + obj.transform.position.y);
            save.WriteLine("objPositionZ:" + obj.transform.position.z);
            // save rotation
            save.WriteLine("objRotationX:" + obj.transform.rotation.x);
            save.WriteLine("objRotationY:" + obj.transform.rotation.y);
            save.WriteLine("objRotationZ:" + obj.transform.rotation.z);
            // save scale
            save.WriteLine("objScaleX:" + obj.transform.localScale.x);
            save.WriteLine("objScaleY:" + obj.transform.localScale.y);
            save.WriteLine("objScaleZ:" + obj.transform.localScale.z);
            // draggable bool (0 or 1)
            save.WriteLine("objDraggable:" + boolToString(objprop.isDraggable));
            // clickable bool
            save.WriteLine("objClickable:" + boolToString(objprop.isClickable));
            // space bool
            save.WriteLine("objSpace:" + boolToString(objprop.kbInputOn));
            // gravity bool
            save.WriteLine("objGravity:" + boolToString(objprop.hasGravity));
            // immobile bool
            save.WriteLine("objImmobile:" + boolToString(objprop.isImmobile));
            // solid bool
            save.WriteLine("objSolid:" + boolToString(objprop.collisions));
            // goal bool
            save.WriteLine("objGoal:" + boolToString(objprop.isTarget));
            // key bool
            save.WriteLine("objKey:" + boolToString(objprop.isKey));
            // controllable bool
            save.WriteLine("objControllable:" + boolToString(objprop.controllable));
            // sprite index
            save.WriteLine("objSpriteIndex:" + objprop.objectSpriteIndex);
            // color index
            save.WriteLine("objColorIndex:" + objprop.colorIndex);
            // sound bool
            save.WriteLine("objHasSound:" + boolToString(objprop.hasSound));
            // sound index
            save.WriteLine("objSoundIndex:" + objprop.audioClipIndex);
            // hostility
            save.WriteLine("objHostile:" + boolToString(objprop.isHostile));
        }

        save.Close();

        lastFile = path;
        fileChanged = true;
        return true;
    }
    private void loadLevel()
    {
        // TODO: allow a way to choose levels later
    }
    private void loadRandomLevel()
    {
        string[] filePaths = Directory.GetFiles("Assets/Saves/", "*.txt", SearchOption.AllDirectories);

        if (filePaths.Length == 0)
        {
            error = true;
            errorMessage = "No save files found";
            return;
        }
        
        int randomIndex = UnityEngine.Random.Range(0, filePaths.Length);
        loadLevel(filePaths[randomIndex]);
    }
    private void loadLevel(String path)
    {
        // verify level
        if (!validLevel(path))
        {
            errorMessage = "Error loading level from " + path + ".\n" + errorMessage;
            return;
        }
        int fileProperties = 0;

        // clear old level stuff
        playGUI.resetLevel();

        String key = "";
        String currentLine = "";
        int objNum = 0;

        // load level in editor
        try
        {
            // read in level
            using (StreamReader file = new StreamReader(path))
            {
                // read in level properties
                // turn off physics
                Physics2D.autoSimulation = true;

                // read in name
                key = "name";
                currentLine = file.ReadLine();
                currentLine = currentLine.Substring(key.Length + 1);
                playGUI.levelName = currentLine;

                // read in id
                key = "id";
                currentLine = file.ReadLine();
                currentLine = currentLine.Substring(key.Length + 1);
                playGUI.levelID = currentLine;

                // read in win condition
                key = "win";
                currentLine = file.ReadLine();
                currentLine = currentLine.Substring(key.Length + 1);
                switch (currentLine)
                {
                    case "0":
                        playGUI.dontMove = true;
                        playGUI.collectKeys = false;
                        playGUI.goToTarget = false;
                        break;
                    case "1":
                        playGUI.dontMove = false;
                        playGUI.collectKeys = true;
                        playGUI.goToTarget = false;
                        break;
                    case "2":
                        playGUI.dontMove = false;
                        playGUI.collectKeys = false;
                        playGUI.goToTarget = true;
                        break;
                }

                // read in time
                key = "time";
                currentLine = file.ReadLine();
                currentLine = currentLine.Substring(key.Length + 1);
                controller.timerStart = int.Parse(currentLine);

                // read in instruction
                key = "instruction";
                currentLine = file.ReadLine();
                currentLine = currentLine.Substring(key.Length + 1);
                playGUI.instruction = currentLine;

                // read in objectTotal
                key = "objectTotal";
                currentLine = file.ReadLine();
                currentLine = currentLine.Substring(key.Length + 1);
                controller.objectTotal = int.Parse(currentLine);

                // read in objectNum
                key = "objectNum";
                currentLine = file.ReadLine();
                currentLine = currentLine.Substring(key.Length + 1);
                objNum = int.Parse(currentLine); // store for later

                // read in maxProperties for the time when this save file was made
                key = "maxProperties";
                currentLine = file.ReadLine();
                currentLine = currentLine.Substring(key.Length + 1);
                fileProperties = int.Parse(currentLine);

                // read in maxWidth
                key = "maxWidth";
                currentLine = file.ReadLine();
                currentLine = currentLine.Substring(key.Length + 1);
                controller.maxWidth = int.Parse(currentLine);

                // read in maxHeight
                key = "maxHeight";
                currentLine = file.ReadLine();
                currentLine = currentLine.Substring(key.Length + 1);
                controller.maxHeight = int.Parse(currentLine);

                // read in background properties
                // read in bgSpriteIndex
                key = "bgSpriteIndex";
                currentLine = file.ReadLine();
                currentLine = currentLine.Substring(key.Length + 1);
                background.backgroundSpriteIndex = int.Parse(currentLine);
                background.spriteRenderer.sprite = background.backgroundSprites[background.backgroundSpriteIndex]; // render new background

                // read in bgColorIndex
                key = "bgColorIndex";
                currentLine = file.ReadLine();
                currentLine = currentLine.Substring(key.Length + 1);
                background.colorIndex = int.Parse(currentLine);
                background.spriteRenderer.color = background.colors[background.colorIndex]; // render new color

                // read in bgHasMusic
                key = "bgHasMusic";
                currentLine = file.ReadLine();
                currentLine = currentLine.Substring(key.Length + 1);
                background.hasSound = stringToBool(currentLine);

                // read in bgMusicIndex
                key = "bgMusicIndex";
                currentLine = file.ReadLine();
                currentLine = currentLine.Substring(key.Length + 1);
                background.backgroundMusicIndex = int.Parse(currentLine);
                background.audioSource.clip = background.backgroundMusic[background.backgroundMusicIndex];

                // play music if it has it
                if (background.hasSound)
                {
                    background.audioSource.Play();
                }

                GameObject newObject;
                objectProperties objProp;
                playerMoveScript objMovement;
                Rigidbody2D body;
                float x = 0;
                float y = 0;
                float z = 0;

                // read in object properties
                // add for loop once it works for one object
                for (int i = 0; i < objNum; i++)
                {
                    if (i == 0) // read first run, otherwise will read at the end of the loop
                    {
                        currentLine = file.ReadLine();
                    }

                    // create new object
                    newObject = (GameObject)Instantiate(Object) as GameObject;

                    // get object properties
                    objProp = newObject.transform.GetChild(0).GetComponent<objectProperties>();
                    objMovement = newObject.GetComponent<playerMoveScript>();
                    body = newObject.transform.GetChild(0).GetComponent<Rigidbody2D>();

                    // read in objName
                    key = "objName";
                    currentLine = currentLine.Substring(key.Length + 1);
                    newObject.name = currentLine;

                    // read in objPositionX
                    key = "objPositionX";
                    currentLine = file.ReadLine();
                    currentLine = currentLine.Substring(key.Length + 1);
                    x = int.Parse(currentLine);

                    // read in objPositionY
                    key = "objPositionY";
                    currentLine = file.ReadLine();
                    currentLine = currentLine.Substring(key.Length + 1);
                    y = int.Parse(currentLine);

                    // read inobjPositionZ
                    key = "objPositionZ";
                    currentLine = file.ReadLine();
                    currentLine = currentLine.Substring(key.Length + 1);
                    z = int.Parse(currentLine);

                    // transform object position
                    newObject.transform.position = new Vector3(x, y, z);

                    // read in objRotationX
                    key = "objRotationX";
                    currentLine = file.ReadLine();
                    currentLine = currentLine.Substring(key.Length + 1);
                    x = int.Parse(currentLine);

                    // read in objRotationY
                    key = "objRotationY";
                    currentLine = file.ReadLine();
                    currentLine = currentLine.Substring(key.Length + 1);
                    y = int.Parse(currentLine);

                    // read inobjRotationZ
                    key = "objRotationZ";
                    currentLine = file.ReadLine();
                    currentLine = currentLine.Substring(key.Length + 1);
                    z = int.Parse(currentLine);

                    // transform object rotation, set it to 0,0,0
                    newObject.transform.GetChild(0).transform.rotation = Quaternion.identity;

                    // read in objScaleX
                    key = "objScaleX";
                    currentLine = file.ReadLine();
                    currentLine = currentLine.Substring(key.Length + 1);
                    x = int.Parse(currentLine);

                    // read in objScaleY
                    key = "objScaleY";
                    currentLine = file.ReadLine();
                    currentLine = currentLine.Substring(key.Length + 1);
                    y = int.Parse(currentLine);

                    // read in objScaleZ
                    key = "objScaleZ";
                    currentLine = file.ReadLine();
                    currentLine = currentLine.Substring(key.Length + 1);
                    z = int.Parse(currentLine);

                    // transform object scale
                    newObject.transform.GetChild(0).transform.localScale = new Vector3(x, y, z);

                    // read in objDraggable
                    key = "objDraggable";
                    currentLine = file.ReadLine();
                    currentLine = currentLine.Substring(key.Length + 1);
                    objProp.isDraggable = stringToBool(currentLine);

                    // read in objClickable
                    key = "objClickable";
                    currentLine = file.ReadLine();
                    currentLine = currentLine.Substring(key.Length + 1);
                    objProp.isClickable = stringToBool(currentLine);

                    // read in objSpace
                    key = "objSpace";
                    currentLine = file.ReadLine();
                    currentLine = currentLine.Substring(key.Length + 1);
                    objProp.kbInputOn = stringToBool(currentLine);

                    // read in objGravity
                    key = "objGravity";
                    currentLine = file.ReadLine();
                    currentLine = currentLine.Substring(key.Length + 1);
                    objProp.hasGravity = stringToBool(currentLine);

                    // read in objImmobile
                    key = "objImmobile";
                    currentLine = file.ReadLine();
                    currentLine = currentLine.Substring(key.Length + 1);
                    objProp.isImmobile = stringToBool(currentLine);

                    // enable/disable immobility
                    if (objProp.isImmobile)
                    {
                        body.isKinematic = true;
                    }
                    else
                    {
                        body.isKinematic = false;
                    }

                    // read in objSolid
                    key = "objSolid";
                    currentLine = file.ReadLine();
                    currentLine = currentLine.Substring(key.Length + 1);
                    objProp.collisions = stringToBool(currentLine);

                    // read in objGoal
                    key = "objGoal";
                    currentLine = file.ReadLine();
                    currentLine = currentLine.Substring(key.Length + 1);
                    objProp.isTarget = stringToBool(currentLine);

                    // read in objKey
                    key = "objKey";
                    currentLine = file.ReadLine();
                    currentLine = currentLine.Substring(key.Length + 1);
                    objProp.isKey = stringToBool(currentLine);

                    // read in objControllable
                    key = "objControllable";
                    currentLine = file.ReadLine();
                    currentLine = currentLine.Substring(key.Length + 1);
                    objProp.controllable = stringToBool(currentLine);

                    if (objProp.controllable)
                    {
                        // enable movement in all directions
                        objMovement.moveLeft = true;
                        objMovement.moveRight = true;
                        objMovement.moveUp = true;
                        objMovement.moveDown = true;
                    }
                    else
                    {
                        objMovement.moveLeft = false;
                        objMovement.moveRight = false;
                        objMovement.moveUp = false;
                        objMovement.moveDown = false;
                    }

                    // read in objSpriteIndex
                    key = "objSpriteIndex";
                    currentLine = file.ReadLine();
                    currentLine = currentLine.Substring(key.Length + 1);
                    objProp.objectSpriteIndex = int.Parse(currentLine);
                    objProp.spriteRenderer.sprite = objProp.objectSprites[objProp.objectSpriteIndex]; // render new background

                    // read in objColorIndex
                    key = "objColorIndex";
                    currentLine = file.ReadLine();
                    currentLine = currentLine.Substring(key.Length + 1);
                    objProp.colorIndex = int.Parse(currentLine);
                    objProp.spriteRenderer.color = objProp.colors[objProp.colorIndex]; // render new color

                    // read in objHasSound
                    key = "objHasSound";
                    currentLine = file.ReadLine();
                    currentLine = currentLine.Substring(key.Length + 1);
                    objProp.hasSound = stringToBool(currentLine);

                    // read in objSoundIndex
                    key = "objSoundIndex";
                    currentLine = file.ReadLine();
                    currentLine = currentLine.Substring(key.Length + 1);
                    objProp.audioClipIndex = int.Parse(currentLine);
                    objProp.audioSource.clip = objProp.audioClips[objProp.audioClipIndex];

                    // if the save file should have more properties
                    if (fileProperties >= 14)
                    {
                        // read in hostile property
                        key = "objHostile";
                        currentLine = file.ReadLine();
                        currentLine = currentLine.Substring(key.Length + 1);
                        objProp.isHostile = stringToBool(currentLine);
                    }

                    // add new object to the list
                    controller.objectList.Add(newObject);

                    String nameKey = "objName";
                    // while the next line is not a new object
                    while (i + 1 != objNum && !hasKey(currentLine, nameKey))
                    {
                        // if there are no more lines
                        if (file.Peek() == -1)
                            return;

                        // read in the next line
                        currentLine = file.ReadLine();
                    }
                }
                // reset the level to round down positions
                Physics2D.autoSimulation = false;
                playGUI.saveObjects();
                playGUI.resetObjects();
            }
        }
        catch (IOException)
        {
            return;
        }

        lastFile = path;
        fileChanged = true;
    }
    private void saveAndLoad()
    {
        // if level saved successfully
        if (saveLevel())
        {
            loadRandomLevel(); // TODO: change to load level after a way to select is implemented
        }
    }
}
