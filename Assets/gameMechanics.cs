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
    public float timerStart = 0;
    private float timeLeft = 0;
    private float lastTime = 0;
    private Boolean timeSet = false;
    private int pointVal = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // if win is reached, win

        // if lose happens, lose

        // if there is a timer, set a timer.
        if (hasTimer)
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
        if (hasPoints)
        {
            pointVal = 0;

            // Code for when events happen
        }
    }
}
