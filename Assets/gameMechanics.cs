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
    public float initialStartTime; // initial start time from the editor, use to reset the start time

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

    // variables related to objects
    public int objectTotal = 0; // the total number of objects, keeps counting so each object has a unique name
    public List<GameObject> objectList;
    public int maxWidth;
    public int maxHeight;

    // Image Variables
    public Texture2D winImage;
    public Texture2D loseImage;

    // Variables related to retrieving the background music
    private GameObject background;
    private bool hasSound = false;
    private AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        initialStartTime = timerStart; // retrieve the first value of the start timer, as set in the unity editor
        objectList = new List<GameObject>();

        // retrieve the background object to control background music later
        background = GameObject.Find("background");
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
            if (Time.time - winSeconds > .5)
                winMarker = false;
        }

        // Show the lose animation win a lose is triggered.
        if (loseMarker)
        {
            if (Time.time - loseSeconds > .5)
                loseMarker = false;
        }
    }

    /*
     * triggerWin
     * Call to trigger a win instantly
     */
    public void triggerWin()
    {
        stopGame();
        winMarker = true;
        winSeconds = Time.time;
    }

    /*
     * triggerLose
     * Call to trigger a lose instantly
     */
    public void triggerLose()
    {
        stopGame();
        loseMarker = true;
        loseSeconds = Time.time;
    }

    public void resetTimer()
    {
        timeLeft = timerStart;
    }

    void OnGUI()
    {
        if (winMarker)
        {
            GUI.Box(new Rect((Screen.width/2) - (winImage.width / 2), (Screen.height / 2) - (winImage.height / 2), winImage.width, winImage.height), winImage);
        }

        if (loseMarker)
        {
            GUI.Box(new Rect((Screen.width / 2) - (loseImage.width / 2), (Screen.height / 2) - (loseImage.height / 2), loseImage.width, loseImage.height), loseImage);
        }
    }
    private void stopGame()
    {
        playingGame = false;

        // retrieve audio variables
        audioSource = background.GetComponent<AudioSource>();
        hasSound = background.GetComponent<changeBackground>().hasSound;

        if (hasSound)
        {
            audioSource.Stop();
        }
    }
}
