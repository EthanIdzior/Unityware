using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gameMechanics : MonoBehaviour
{
    public Boolean isWin = false;
    public Boolean isLose = false;
    public Boolean hasTimer = false;
    public Boolean hasPoints = false;
    private Boolean showText = false;
    public float timerStart = 0;
    public float timeLeft = 0;
    private float lastTime = 0;
    private Boolean timeSet = false;
    private int pointVal = 0;
    public Boolean playingGame = false;
    private Boolean winMarker = false;



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

        if (!showText && playingGame) 
        {
            
        }

        // if there are points, set points.
        if (hasPoints && playingGame)
        {
            pointVal = 0;

            // Code for when events happen
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
            // Set to actually win
        }
    }
}
