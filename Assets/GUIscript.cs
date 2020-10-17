using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIscript : MonoBehaviour
{
    public gameMechanics controller;
    string buttonSymbol = "▶";
    bool trackReset = false;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<gameMechanics>();
    }

    // Update is called once per frame
    void Update()
    {
        if (controller.playingGame == false && buttonSymbol == "| |")
        {
            buttonSymbol = "▶";
        }
    }

    void OnGUI()
    {
        if (GUI.Button (new Rect(Screen.width - 100, 0, 80, 20), buttonSymbol))
        {
            // Change from edit to play
            if (buttonSymbol == "▶") {
                buttonSymbol = "| |";
                controller.playingGame = true;
            }

            // Change from play to edit
            else if (buttonSymbol == "| |")
            {
                buttonSymbol = "▶";
                controller.playingGame = false;
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
