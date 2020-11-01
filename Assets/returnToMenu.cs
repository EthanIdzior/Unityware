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

            // Change Sprites
            GUILayout.BeginHorizontal("box");
            if (GUILayout.Button("Return To Menu"))
            {
                SceneManager.LoadScene(menu);
            }
            if (GUILayout.Button("Save Level"))
            {
                
            }
            GUILayout.EndHorizontal();




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
}
