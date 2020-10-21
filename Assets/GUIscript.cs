using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIscript : MonoBehaviour
{
    public gameMechanics controller;
    string buttonSymbol = "▶";
    bool trackReset = false;

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

    void OnGUI()
    {
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
