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
    int height = 150;
    int width = 200;

    // Variables related to audio
    public AudioSource audioSource;
    public AudioClip[] backgroundMusic;
    private int backgroundMusicIndex = 0;
    public bool hasSound = false;

    // Create an array to hold the sprites
    public SpriteRenderer spriteRenderer;
    public Sprite[] backgroundSprites;
    private int backgroundSpriteIndex = 0;

    bool soundToggled = false;

    // Create an array with preset colors
    private Color[] colors = {/* White: */ new Color(1, 1, 1, 1), /* Magenta: */ new Color(1, 0, 1, 1), /* Red: */ new Color(1, 0, 0, 1), /* Yellow: */ new Color(1, 1, 0, 1), /* Green: */ new Color(0, 1, 0, 1), /* Cyan: */ new Color(0, 1, 1, 1), /* Blue: */ new Color(0, 0, 1, 1), /* Black: */ new Color(0, 0, 0, 1) };
    private int colorIndex = 0;

    // Need to get playing mode
    public GUIscript playGUI;

    // Start is called before the first frame update
    private void Start()
    {
        // Add audio
        audioSource = gameObject.GetComponent<AudioSource>();
        GameObject mainCamera = GameObject.Find("Main Camera");
        playGUI = mainCamera.GetComponent<GUIscript>();
    }

    private void OnGUI()
    {
        if (!playGUI.controller.playingGame)
        {
            GUILayout.BeginArea(new Rect(Screen.width - width, Screen.height - height, width, height), GUI.skin.box);

            GUILayout.Label("Background Properties");

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
                audioSource.Stop();
                backgroundMusicIndex = 0; // reset index
                soundToggled = false;
            }


            GUILayout.EndArea();
        }
        
    }
}
