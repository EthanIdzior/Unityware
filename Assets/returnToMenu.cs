using System.Collections;
using System.Collections.Generic;
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
                saveLevel();
            }
            if (GUILayout.Button("Load Level"))
            {
                // TODO
            }
            GUILayout.EndHorizontal();

            // Save/Load to/from a user defined location
            GUILayout.BeginHorizontal("box");
            if (GUILayout.Button("Export Level"))
            {
                // TODO
            }
            if (GUILayout.Button("Import Level"))
            {
                // TODO
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

    }
    /**
     * Method to save the levels locally
     */
    private void saveLevel()
    {
        // TODO
        // get the desired file path, which is the level name with certain letters stripped or replaced
            // replace space with -
            // strip NUL, \, /, :, *, ", <, >, |, and .
        // check if the file exists
            // If so check if it has the same level id
                // if no, throw an error stating that the level already exists and return
        // set up the file to save to

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
