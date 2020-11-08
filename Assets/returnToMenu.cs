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
    // Variables used for saving to help compatiblity later on
    int maxProperties = 13; // the number of properties that can be set, used for saving

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

    private bool error = false;
    private String errorMessage = "";

    private bool fileChanged = false;
    private String change = "";

    // Start is called before the first frame update
    private void Start()
    {
        // Retrieve variables from other scripts
        GameObject mainCamera = GameObject.Find("Main Camera");
        playGUI = mainCamera.GetComponent<GUIscript>();
        controller = mainCamera.GetComponent<gameMechanics>();
        background = (GameObject.Find("background")).GetComponent<changeBackground>();
        objZeroProperties = ((GameObject.Find("Object0")).transform.GetChild(0)).GetComponent<objectProperties>(); // TODO: check if this is being retrieve correctly

        UnityEngine.Debug.Log(objZeroProperties + " ");
        UnityEngine.Debug.Log(objZeroProperties.objectSprites.Length);
    }

    private void OnGUI()
    {
        if (menuOpen && !playGUI.controller.playingGame)
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
                if (!String.IsNullOrEmpty(playGUI.levelName))
                {
                    saveLevel();
                }
                else
                {
                    error = true;
                    errorMessage = "Level must be named before saving";
                }
                
            }
            if (GUILayout.Button("Load Level"))
            {
                change = "loaded";
                // TODO verify level

                // load level in editor
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
                    // TODO: verify imported level
                    if (validLevel(path))
                    {
                        // if file does not exist
                        if (!File.Exists("Assets/Saves/" + Path.GetFileName(path)))
                        {
                            // copy file to local saves
                            File.Copy(path, "Assets/Saves/" + Path.GetFileName(path));

                            fileChanged = true;
                            change = "imported";
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
                        errorMessage = "The imported level is not valid";
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
            GUILayout.BeginArea(new Rect((Screen.width / 2) - (210 / 2), (Screen.height / 2) - (60 / 2), 210, 60), GUI.skin.box);
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
            GUILayout.BeginArea(new Rect((Screen.width / 2) - (210 / 2), (Screen.height / 2) - (60 / 2), 210, 60), GUI.skin.box);
            GUILayout.Label("Level " + change + " successfully");
            if (GUILayout.Button("Close"))
            {
                fileChanged = false;
            }
            GUILayout.EndArea();
        }
    }
    /**
     * Helper method to ensure the name is valid
     */
    private bool validKey(String currentLine)
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
    private bool validID(String currentLine)
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
    private bool validWin(String currentLine)
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
    private bool validTime(String currentLine)
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
    private bool validInstruction(String currentLine)
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
    private bool validObjectTotal(String currentLine, ref int objTotal)
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
    private bool validObjectNum(String currentLine, int objTotal, ref int objNum)
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
    private bool validMaxProperties(String currentLine, ref int maxProperties)
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
    private bool validMaxWidth(String currentLine, ref int maxWidth)
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
    private bool validMaxHeight(String currentLine, ref int maxHeight)
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
    private bool validBgSpriteIndex(String currentLine)
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
    private bool validBgColorIndex(String currentLine)
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
    private bool validBgHasMusic(String currentLine)
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
    private bool validBgMusicIndex(String currentLine)
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
    private bool validObjName(String currentLine, int i)
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
    private bool validObjPositionX(String currentLine, int maxWidth)
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
    private bool validObjPositionY(String currentLine, int maxHeight)
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
    private bool validObjPositionZ(String currentLine)
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
    private bool validObjRotation(String currentLine, String letter)
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
    private bool validObjScale(String currentLine, String letter)
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
    private bool validObjDraggable(String currentLine)
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
    private bool validObjClickable (String currentLine)
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
    private bool validObjSpace(String currentLine)
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
    private bool validObjGravity(String currentLine, ref bool gravity)
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
    private bool validObjImmobile(String currentLine, bool gravity, ref bool immobile)
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
    private bool validObjSolid(String currentLine)
    {
        String key = "objSolid";
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
    private bool validObjGoal(String currentLine, bool immobile, ref bool goal)
    {
        String key = "objGoal";
        if (hasKey(currentLine, key))
        {
            currentLine = currentLine.Substring(key.Length + 1);

            if (!isIntBoolean(currentLine, key))
                return false;

            goal = stringToBool(currentLine);

            // both must be active at the same time
            if (immobile != goal)
                return false;

            return true;
        }
        else
            return false;
    }
    private bool validObjKey(String currentLine, bool immobile, ref bool objKey)
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
    private bool validObjControllable(String currentLine, bool goal, bool objKey)
    {
        String key = "objControllable";
        if (hasKey(currentLine, key))
        {
            currentLine = currentLine.Substring(key.Length + 1);

            if (!isIntBoolean(currentLine, key))
                return false;

            bool controllable = stringToBool(currentLine);

            // controllable cannot be active at the same time as goal or key
            if ((goal && controllable) || (objKey && controllable))
                return false;

            return true;
        }
        else
            return false;
    }
    private bool validObjSpriteIndex(String currentLine)
    {
        // TODO: complete method
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
    /**
     * Method to ensure that levels are valid
     */
    private bool validLevel(String path)
    {
        bool result = true;
        String currentLine = "";

        int objTotal = 0;
        int objNum = 0;
        int maxProperties = 0;
        int maxWidth = 0;
        int maxHeight = 0;
        bool gravity = false;
        bool immobile = false;
        bool goal = false;
        bool key = false;

        try
        {
            // TODO: check if level is valid
            using (StreamReader file = new StreamReader(path))
            {
                // TODO: verify level properties

                // verify name
                currentLine = file.ReadLine();
                if (!validKey(currentLine))
                    return false;

                // verify id
                currentLine = file.ReadLine();
                if (!validID(currentLine))
                    return false;
                
                // verify win condition
                currentLine = file.ReadLine();
                if (!validWin(currentLine))
                    return false;

                // verify time
                currentLine = file.ReadLine();
                if (!validTime(currentLine))
                    return false;

                // verify instruction
                currentLine = file.ReadLine();
                if (!validInstruction(currentLine))
                    return false;

                // verify objectTotal
                currentLine = file.ReadLine();
                if (!validObjectTotal(currentLine, ref objTotal))
                    return false;

                // verify objectNum
                currentLine = file.ReadLine();
                if (!validObjectNum(currentLine, objTotal, ref objNum))
                    return false;
                    

                // verify maxProperties
                currentLine = file.ReadLine();
                if (!validMaxProperties(currentLine, ref maxProperties))
                    return false;

                // verify maxWidth
                currentLine = file.ReadLine();
                if (!validMaxWidth(currentLine, ref maxWidth))
                    return false;

                // verify maxHeight
                currentLine = file.ReadLine();
                if (!validMaxHeight(currentLine, ref maxHeight))
                    return false;

                // verify background properties
                // verify bgSpriteIndex
                currentLine = file.ReadLine();
                if (!validBgSpriteIndex(currentLine))
                    return false;

                // verify bgColorIndex
                currentLine = file.ReadLine();
                if (!validBgColorIndex(currentLine))
                    return false;

                // verify bgHasMusic
                currentLine = file.ReadLine();
                if (!validBgHasMusic(currentLine))
                    return false;

                // verify bgMusicIndex
                currentLine = file.ReadLine();
                if (!validBgMusicIndex(currentLine))
                    return false;

                // TODO: verify object properties
                // TODO: add for loop once it works for one object
                for (int i = 0; i < objNum; i++)
                {
                    if (i == 0) // read first run, otherwise will read at the end of the loop
                    {
                        currentLine = file.ReadLine();
                    }

                    // verify objName
                    if (!validObjName(currentLine, i))
                        return false;

                    // verify objPositionX
                    currentLine = file.ReadLine();
                    if (!validObjPositionX(currentLine, maxWidth))
                        return false;

                    // verify objPositionY
                    currentLine = file.ReadLine();
                    if (!validObjPositionY(currentLine, maxHeight))
                        return false;

                    // verify objPositionZ
                    currentLine = file.ReadLine();
                    if (!validObjPositionZ(currentLine))
                        return false;

                    // verify objRotationX
                    currentLine = file.ReadLine();
                    if (!validObjRotation(currentLine, "X"))
                        return false;

                    // verify objRotationY
                    currentLine = file.ReadLine();
                    if (!validObjRotation(currentLine, "Y"))
                        return false;

                    // verify objRotationZ
                    currentLine = file.ReadLine();
                    if (!validObjRotation(currentLine, "Z"))
                        return false;

                    // verify objScaleX
                    currentLine = file.ReadLine();
                    if (!validObjScale(currentLine, "X"))
                        return false;

                    // verify objScaleY
                    currentLine = file.ReadLine();
                    if (!validObjScale(currentLine, "Y"))
                        return false;

                    // verify objScaleZ
                    currentLine = file.ReadLine();
                    if (!validObjScale(currentLine, "Z"))
                        return false;

                    // verify objDraggable
                    currentLine = file.ReadLine();
                    if (!validObjDraggable(currentLine))
                        return false;

                    // verify objClickable
                    currentLine = file.ReadLine();
                    if (!validObjClickable(currentLine))
                        return false;

                    // verify objSpace
                    currentLine = file.ReadLine();
                    if (!validObjSpace(currentLine))
                        return false;

                    // verify objGravity
                    currentLine = file.ReadLine();
                    if (!validObjGravity(currentLine, ref gravity))
                        return false;

                    // verify objImmobile
                    currentLine = file.ReadLine();
                    if (!validObjImmobile(currentLine, gravity, ref immobile))
                        return false;

                    // verify objSolid
                    currentLine = file.ReadLine();
                    if (!validObjSolid(currentLine))
                        return false;

                    // verify objGoal
                    currentLine = file.ReadLine();
                    if (!validObjGoal(currentLine, immobile, ref goal))
                        return false;

                    // verify objKey
                    currentLine = file.ReadLine();
                    if (!validObjKey(currentLine, immobile, ref key))
                        return false;

                    // verify objControllable
                    currentLine = file.ReadLine();
                    if (!validObjControllable(currentLine, goal, key))
                        return false;

                    // TODO: verify objSpriteIndex
                    // TODO FINISH METHOD FOR OBJSPRITEINDEX

                    // TODO: verify objColorIndex
                    // TODO: verify objHasSound
                    // TODO: verify objSoundIndex

                    String nameKey = "objName";
                    // while the next line is not a new object
                    while (i + 1 != objNum && !hasKey(currentLine, nameKey))
                    {
                        // if there are no more lines
                        if (file.Peek() == -1)
                            return false;

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
    private void saveLevel()
    {
        String fileName = playGUI.levelName;

        fileName = nameToFileName(fileName);
        fileName += ".txt";
        fileName = "Assets/Saves/" + fileName; // add subfolder the saves go to for the full path

        saveLevel(fileName);
    }
    /**
     * Method to save levels given the path
     */
    private void saveLevel(String path)
    {
        // check if the file exists
        if (File.Exists(path))
        {
            // TODO: can't implement until the first file is saved
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
        save.WriteLine("bgHasMusic:" + boolToString(background.soundToggled));
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
            save.WriteLine("objSolid:" + boolToString(objprop.isSolid));
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
            save.WriteLine("objHasSound:" + objprop.hasSound);
            // sound index
            save.WriteLine("objSoundIndex:" + objprop.audioClipIndex);
        }


        save.Close();

        fileChanged = true;
    }
}
