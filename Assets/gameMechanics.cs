using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Versioning;
using UnityEngine;

public class gameMechanics : MonoBehaviour
{

    // Activation Booleans
    public Boolean isWin = false;
    public Boolean isLose = false;
    public Boolean hasTimer = false;
    public Boolean hasPoints = false;

    // Values that can be set externally
    public float timerStart = 0;

    // Game variables
    public float timeLeft = 0;
    private float lastTime = 0;
    private Boolean timeSet = false;
    private int pointVal = 0;
    public Boolean playingGame = false;
    private Boolean winMarker = false;
    private float winSeconds = 0;
    private Boolean loseMarker = false;
    private float loseSeconds = 0;

    // Image Variables
    public Texture2D winImage;
    public Texture2D loseImage;



    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        // if there is a timer, set a timer.
        if (hasTimer && playingGame)
        {
            // If the timer has not been set, do that.
            if (!timeSet)
            {
                timeLeft = timerStart;
                lastTime = Time.time;
                timeSet = true;
            }

            // Else, decrement the timer and set last time to current time
            else
            {
                if (Time.time - lastTime >= 1)
                {
                    lastTime = Time.time;
                    timeLeft--;
                }
            }
        }


        // if there are points, set points.
        if (hasPoints && playingGame)
        {
            pointVal = 0;

            // Code for when events happen
        }

        // Show the win animation win a win is triggered.
        if (winMarker)
        {
            if (Time.time - winSeconds > 3)
                winMarker = false;
        }
    }

    /*
     * triggerWin
     * Call to trigger a win instantly
     */
    public void triggerWin()
    {
        playingGame = false;
        winMarker = true;
        winSeconds = Time.time;
    }

    /*
     * triggerLose
     * Call to trigger a lose instantly
     */
    public void triggerLose()
    {
        playingGame = false;
        loseMarker = true;
        loseSeconds = Time.time;
    }

    /*
     * playInstructions
     * On start play level instructions
     */
    public void playInstructions(String instructions)
    {
        
    }

    public void resetTimer()
    {
        timeLeft = timerStart;
    }

    void OnGUI()
    {
        if (winMarker)
        {
            GUI.Box(new Rect( 270, 100, winImage.width, winImage.height), winImage);
        }

        if (loseMarker)
        {
            GUI.Box(new Rect(270, 100, winImage.width, winImage.height), loseImage);
        }
    }

}
