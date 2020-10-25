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
    bool levelMenuToggle = false;

    // Variables related to retrieving the background music
    private GameObject background;
    private bool hasSound = false;
    private AudioSource audioSource;

    // Types of win
    private bool dontMove = false;
    private bool collectKeys = false;
    private bool goToTarget = false;

    private String instruction = "";

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

        if (Input.GetKeyDown(KeyCode.L))
        {
            if (!levelMenuToggle)
                levelMenuToggle = true;

            else
                levelMenuToggle = false;

            print("Toggled");
        }
    }
    /**
     * Helper method to add an object
     */
    public void createObject()
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
        if (!controller.playingGame)
        {

            if (levelMenuToggle)
            {
                int levelMenuWidth = 200;
                int levelMenuHeight = 300;
                GUILayout.BeginArea(new Rect((Screen.width/2) - (levelMenuWidth/2), (Screen.height/2) - (levelMenuHeight/2), levelMenuWidth, levelMenuHeight), GUI.skin.box);

                GUILayout.BeginHorizontal("box");
                GUILayout.Label("Win Condition");
                if (GUILayout.Button("X"))
                {
                    levelMenuToggle = false;
                }
                GUILayout.EndHorizontal();

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

                goToTarget = GUILayout.Toggle(goToTarget, "Space Does Action");
                // Toggle the others
                if (goToTarget)
                {
                    dontMove = false;
                    collectKeys = false;
                }

                GUILayout.Label("Level Time");
                controller.timerStart = float.Parse(GUILayout.TextField((controller.timerStart).ToString(), 1));

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
