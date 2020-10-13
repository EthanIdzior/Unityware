using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Required for sprites
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/**
 * Yaqirah Rice
 * Shows a menu that allows the background object's properties to be changed
 */

public class changeBackground : MonoBehaviour
{
    // Variables related to the menu itself
    int height = 75;
    int width = 200;

    // Create an array to hold the sprites
    public SpriteRenderer spriteRenderer;
    public Sprite[] backgroundSprites;
    private int backgroundSpriteIndex = 0;

    // Create an array with preset colors
    private Color[] colors = {/* White: */ new Color(1, 1, 1, 1), /* Magenta: */ new Color(1, 0, 1, 1), /* Red: */ new Color(1, 0, 0, 1), /* Yellow: */ new Color(1, 1, 0, 1), /* Green: */ new Color(0, 1, 0, 1), /* Cyan: */ new Color(0, 1, 1, 1), /* Blue: */ new Color(0, 0, 1, 1), /* Black: */ new Color(0, 0, 0, 1) };
    private int colorIndex = 0;

    // Start is called before the first frame update
    private void Start()
    {
        // Load all sprites from the sprite sheet
        AsyncOperationHandle<Sprite[]> spriteHandle = Addressables.LoadAssetAsync<Sprite[]>("Assets/Graphics/Backgrounds/backgroundSheet.png");
        spriteHandle.Completed += LoadSpritesWhenReady;
    }

    // Helper method to load sprites
    void LoadSpritesWhenReady(AsyncOperationHandle<Sprite[]> handleToCheck)
    {
        if (handleToCheck.Status == AsyncOperationStatus.Succeeded)
        {
            backgroundSprites = handleToCheck.Result;
        }
    }

    private void OnGUI()
    {
            GUILayout.BeginArea(new Rect(Screen.width - width, Screen.height - height, width, height), GUI.skin.box);


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

            GUILayout.EndArea();
        
    }
}
