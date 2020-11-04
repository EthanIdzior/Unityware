using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class returnToMenu : MonoBehaviour
{
    // Variables related to the menu itself
    int height = 150;
    int width = 200;

    //needed to return to menu
    public string menu;

    private bool menuOpen = true;

    // Need to get playing mode
    public GUIscript playGUI;

    private bool error = false;
    private String errorMessage = "";

    // Start is called before the first frame update
    private void Start()
    {
        // Add audio
 
        GameObject mainCamera = GameObject.Find("Main Camera");
        playGUI = mainCamera.GetComponent<GUIscript>();
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
                // TODO verify level

                // load level in editor
            }
            GUILayout.EndHorizontal();

            // Save/Load to/from a user defined location
            GUILayout.BeginHorizontal("box");
            if (GUILayout.Button("Export Level"))
            {
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
                String path;

                // get the path of the level
                path = EditorUtility.OpenFilePanel(
                    "Export a level",
                    "",
                    "txt");
                if (path.Length != 0)
                {
                    // TODO: verify imported level

                    // TOOD: copy file to local saves
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
    }
    /**
     * Method to save the levels locally, gets a filename from the level name
     */
    private void saveLevel()
    {
        String fileName = playGUI.levelName;

        fileName = nameToFileName(fileName);
        fileName += ".txt";
        fileName = "Saves/" + fileName; // add subfolder the saves go to for the full path

        saveLevel(fileName);
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
     * Method to save levels given the path
     */
    private void saveLevel(String path)
    {
        // TODO

        // check if the file exists
            // If so check if it has the same level id
                // if no, throw an error stating that the level already exists and return
        // set up the file to save to

        // save values in the form name:value, ex solid:0. This is to allow older saves to be compatible as we can just set newer properties to default values if the solid descriptor is not found

        // Begin writing to the file line by line
        // Save level settings
            // Level name
            // level id
            // Win condition (as int)
            // level time
            // level instruction
        // Save general background/object properties
            // object Total (next int index for new objects, total objects created)
            // current number of objects
            // number of properties per object
            // max width
            // max height
            // background properties
            // sprite index
            // color index
            // music bool (0 or 1)
            // music index, -1 if not applicable
        // object properties (per object, wrap in for loop)
            // object name
            // draggable bool (0 or 1)
            // clickable bool
            // space bool
            // gravity bool
            // immobile bool
            // goal bool
            // key bool
            // controllable bool
            // sprite index
            // color index
            // sound bool
            // sound index
    }
}
