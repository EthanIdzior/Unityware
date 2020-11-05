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
                    "Export a level",
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
                            catch (IOException e)
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
                                catch (IOException e)
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
     * Method to ensure that levels are valid
     */
    private bool validLevel(String path)
    {
        bool result = true;
        String currentLine = "";
        String key = "";

        try
        {
            // TODO: check if level is valid
            using (StreamReader file = new StreamReader(path))
            {
                // TODO: verify level properties

                // verify name
                currentLine = file.ReadLine();
                key = "name";
                // verify first half of line
                if (hasKey(currentLine, key)){
                    // verify second half is not empty
                    // strip key
                    currentLine = currentLine.Substring(key.Length + 1);

                    // check if name is empty or null
                    if (String.IsNullOrEmpty(currentLine))
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
                // TODO: verify id
                // TODO: verify win condition
                // TODO: verify time
                // TODO: verify instruction
                // TODO: verify objectTotal
                // TODO: verify objectNum
                // TODO: verify maxProperties
                // TODO: verify maxWidth
                // TODO: verify maxHeight

                // TODO: verify background properties
                // TODO: verify bgSpriteIndex
                // TODO: verify bgColorIndex
                // TODO: verify bgHasMusic
                // TODO: verify bgMusicIndex

                // TODO: verify object properties

            }
        }
        catch (IOException e)
        {
            return false;
        }

        return result;
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
