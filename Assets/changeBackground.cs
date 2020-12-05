using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Yaqirah Rice
 * Shows a menu that allows the background object's properties to be changed
 */

public class changeBackground : MonoBehaviour
{
    // Variables related to the menu itself
    int height = 190;
    int width = 200;

    // Variables related to audio
    public AudioSource audioSource;
    public AudioClip[] backgroundMusic;
    public int backgroundMusicIndex = 0;
    public bool hasSound = false;

    // Create an array to hold the sprites
    public SpriteRenderer spriteRenderer;
    public Sprite[] backgroundSprites;
    public int backgroundSpriteIndex = 0;

    private bool menuOpen = true;
    public bool soundToggled = false;
    private bool clearBackground = false;

    GameObject playObj;

    // Create an array with preset colors
    public Color[] colors = {/* White: */ new Color(1, 1, 1, 1), /* Magenta: */ new Color(1, 0, 1, 1), /* Red: */ new Color(1, 0, 0, 1), /* Yellow: */ new Color(1, 1, 0, 1), /* Green: */ new Color(0, 1, 0, 1), /* Cyan: */ new Color(0, 1, 1, 1), /* Blue: */ new Color(0, 0, 1, 1), /* Black: */ new Color(0, 0, 0, 1) };
    public int colorIndex = 0;

    // Need to get playing mode
    public GUIscript playGUI;

    // Start is called before the first frame update
    private void Start()
    {
        // Add audio
        audioSource = gameObject.GetComponent<AudioSource>();
        GameObject mainCamera = GameObject.Find("Main Camera");
        playGUI = mainCamera.GetComponent<GUIscript>();
        playObj = GameObject.Find("PlayModeObj");
    }

    private void OnGUI()
    {
        if (menuOpen && (!playGUI.controller.playingGame && playGUI.controller.showGUI) && ((playObj != null) && !(playObj.GetComponent<playTrack>().getPlay3() || playObj.GetComponent<playTrack>().getPlayLevel())))
        {
            GUILayout.BeginArea(new Rect(Screen.width - width, Screen.height - height, width, height), GUI.skin.box);

            GUILayout.BeginHorizontal("box");
            GUILayout.Label("Background Properties");
            if (GUILayout.Button("_"))
            {
                menuOpen = false;
            }
            GUILayout.EndHorizontal();

            // Change Sprites
            GUILayout.BeginHorizontal("box");
            if (GUILayout.Button("Previous Sprite"))
            {
                backgroundSpriteIndex--;
                // If the index has gone below the bounds of the array
                if (backgroundSpriteIndex <= -1)
                {
                    // Go to the last element
                    backgroundSpriteIndex = backgroundSprites.Length - 1;
                }
                // Render the previous sprite
                spriteRenderer.sprite = backgroundSprites[backgroundSpriteIndex];
            }
            if (GUILayout.Button("Next Sprite"))
            {
                backgroundSpriteIndex++;
                // If the index has gone over the array, reset to zero
                if (backgroundSpriteIndex >= backgroundSprites.Length)
                {
                    backgroundSpriteIndex = 0;
                }
                // Render the next sprite
                spriteRenderer.sprite = backgroundSprites[backgroundSpriteIndex];
            }
            GUILayout.EndHorizontal();

            // Change colors
            GUILayout.BeginHorizontal("box");
            if (GUILayout.Button("Previous Color"))
            {
                colorIndex--;
                if (colorIndex <= -1)
                {
                    // Go to the last element
                    colorIndex = colors.Length - 1;
                }
                spriteRenderer.color = colors[colorIndex];
            }
            if (GUILayout.Button("Next Color"))
            {
                colorIndex++;
                if (colorIndex >= colors.Length)
                {
                    // Go to the first element
                    colorIndex = 0;
                }
                spriteRenderer.color = colors[colorIndex];
            }

            GUILayout.EndHorizontal();

            // Change Background Music
            hasSound = GUILayout.Toggle(hasSound, "Background Music");
            if (hasSound)
            {
                if (!soundToggled)
                {
                    audioSource.clip = backgroundMusic[backgroundMusicIndex];
                    audioSource.Play();
                    soundToggled = true;
                }
                // Add sound to objects
                GUILayout.BeginHorizontal("box");
                if (GUILayout.Button("Previous Track"))
                {
                    backgroundMusicIndex--;
                    if (backgroundMusicIndex <= -1)
                    {
                        backgroundMusicIndex = backgroundMusic.Length - 1;
                    }
                    audioSource.clip = backgroundMusic[backgroundMusicIndex];
                    audioSource.Play();
                }
                if (GUILayout.Button("Next Track"))
                {
                    backgroundMusicIndex++;
                    if (backgroundMusicIndex >= backgroundMusic.Length)
                    {
                        backgroundMusicIndex = 0;
                    }
                    audioSource.clip = backgroundMusic[backgroundMusicIndex];
                    audioSource.Play();
                }
                GUILayout.EndHorizontal();
            }
            else
            {
                stopMusic();
            }

            // reset properties
            if (GUILayout.Button("Reset Background"))
            {
                // if the values aren't already at default
                if (changed())
                {
                    // show the menu to reset the background
                    clearBackground = true;
                }
            }

            GUILayout.EndArea();
        }
        else if(!playGUI.controller.playingGame && playGUI.controller.showGUI && ((playObj != null) && !(playObj.GetComponent<playTrack>().getPlay3() || playObj.GetComponent<playTrack>().getPlayLevel())))
        {
            GUILayout.BeginArea(new Rect(Screen.width - 40, Screen.height - 30, 40, 30), GUI.skin.box);
            if (GUILayout.Button("_"))
            {
                menuOpen = true;
            }
            GUILayout.EndArea();
        }
        if (clearBackground)
        {
            GUILayout.BeginArea(new Rect((Screen.width / 2) - (210 / 2), (Screen.height / 2) - (60 / 2), 230, 80), GUI.skin.box);

            GUILayout.Label("Are you sure you would like to reset the background?");
            GUILayout.BeginHorizontal("box");
            if (GUILayout.Button("Confirm"))
            {
                resetBackground();
                clearBackground = false; // hide menu
            }
            if (GUILayout.Button("Cancel"))
            {
                clearBackground = false; // hide menu
            }
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
    }
    /*
     * Helper method to reset all background properties
     */
    public void resetBackground()
    {
        // reset background sprite
        backgroundSpriteIndex = 0;
        spriteRenderer.sprite = backgroundSprites[0];

        // reset background color
        colorIndex = 0;
        spriteRenderer.color = colors[0];

        // set background has sound to false
        hasSound = false;
        soundToggled = true;

        // reset background audio
        backgroundMusicIndex = 0;
        audioSource.clip = backgroundMusic[0];

        stopMusic();
    }
    public bool changed()
    {
        bool result = false;

        if (backgroundSpriteIndex != 0)
        {
            result = true;
        }
        if (colorIndex != 0)
        {
            result = true;
        }
        if (hasSound)
        {
            result = true;
        }
        
        return result;
    }
    public void stopMusic()
    {
        audioSource.Stop();
        backgroundMusicIndex = 0; // reset index
        soundToggled = false;
    }
}
