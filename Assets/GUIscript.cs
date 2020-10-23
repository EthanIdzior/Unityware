using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class GUIscript : MonoBehaviour
{
    public GameObject Object;
    private int prefabOffset = 9; // Use for math related to spawning objects. Object0 is located out of bounds (look to the right in the scene editor) in order to be usable as a prototype for creating other objects and is not to be controlled/counted with the other objects.

    public gameMechanics controller;
    string buttonSymbol = "▶";
    bool trackReset = false;
    bool objectError = false;

    // Variables related to retrieving the background music
    private GameObject background;
    private bool hasSound = false;
    private AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<gameMechanics>();
        background = GameObject.Find("background");
    }

    // Update is called once per frame
    void Update()
    {
        if (controller.playingGame == false && buttonSymbol == "| |")
        {
            buttonSymbol = "▶";
        }
    }
    /**
     * Helper method to add an object
     */
    void createObject()
    {
        // if the canvas can fit more objects
        if (controller.objectList.Count <= (controller.maxWidth * controller.maxHeight))
        {
            GameObject newObject = (GameObject)Instantiate(Object) as GameObject;
            float x = UnityEngine.Random.Range(-8 - prefabOffset, 9 - prefabOffset);
            float y = UnityEngine.Random.Range(-4, 5);

            // make objects not land on top of each other
            int i = 0;
            while (i < controller.objectList.Count)
            {
                GameObject obj = controller.objectList[i];

                if (obj.transform.position.x == x && obj.transform.position.y == y)
                {
                    // reroll
                    x = UnityEngine.Random.Range(-8 - prefabOffset, 9 - prefabOffset);
                    y = UnityEngine.Random.Range(-4, 5);

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
    void OnGUI()
    {
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
        if (GUI.Button (new Rect(Screen.width - 100, 0, 80, 20), buttonSymbol))
        {
            // Change from edit to play
            if (buttonSymbol == "▶") {
                buttonSymbol = "| |";
                controller.playingGame = true;

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

                if (hasSound)
                {
                    audioSource.Pause();
                }
            }

        }

        if (controller.hasTimer && controller.playingGame)
        {
            GUI.Label(new Rect(Screen.width - 100, 20, 80, 20), (controller.timeLeft).ToString());
            trackReset = true;
        }

        if (!(controller.hasTimer && controller.playingGame) && trackReset)
        {
            controller.resetTimer();
            trackReset = false;
        }
    }

}
