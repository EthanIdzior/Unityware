using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

// Required for sprites
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class objectDrag : MonoBehaviour
{
    private bool isDragging;
    bool menuOpen = false;
    public bool isDraggable = false;
    public bool mouseOver;
    Vector2 mousePos;

    // Create an array to hold the sprites
    public SpriteRenderer spriteRenderer;
    public Sprite[] objectSprites;
    private int objectSpriteIndex = 0;

    // Create an array with preset colors
    private Color[] colors = { /* Black: */ new Color(0, 0, 0, 1),  /* White: */ new Color(1, 1, 1, 1), /* Magenta: */ new Color(1, 0, 1, 1), /* Red: */ new Color(1, 0, 0, 1), /* Yellow: */ new Color(1, 1, 0, 1), /* Green: */ new Color(0, 1, 0, 1), /* Cyan: */ new Color(0, 1, 1, 1), /* Blue: */ new Color(0, 0, 1, 1) };
    private int colorIndex = 0;

    // Start called on the first frame update
    private void Start()
    {
        // Load all sprites from the sprite sheet
        AsyncOperationHandle<Sprite[]> spriteHandle = Addressables.LoadAssetAsync<Sprite[]>("Assets/Graphics/Objects/sheet.png");
        spriteHandle.Completed += LoadSpritesWhenReady;
    }

    // Helper method to load sprites
    void LoadSpritesWhenReady(AsyncOperationHandle<Sprite[]> handleToCheck)
    {
        if (handleToCheck.Status == AsyncOperationStatus.Succeeded)
        {
            objectSprites = handleToCheck.Result;
        }
    }

    private void OnMouseOver()
    {
        mouseOver = true;
    }

    private void OnMouseExit()
    {
        mouseOver = false;
    }
    
    private void OnGUI()
    {
        
        if (menuOpen)
       {
            mousePos = Input.mousePosition; 
            GUILayout.BeginArea(new Rect(-10, 4, 300, 200), GUI.skin.box);
            GUILayout.Label("The Current Value of Draggable is " + isDraggable);
            if (GUILayout.Button("Set Draggable"))
            {
                isDraggable = !isDraggable;
            }

            // Change Sprites
            GUILayout.BeginHorizontal("box");
            if (GUILayout.Button("Previous Sprite"))
            {
                objectSpriteIndex--;
                // If the index has gone below the bounds of the array
                if (objectSpriteIndex <= -1)
                {
                    // Go to the last element
                    objectSpriteIndex = objectSprites.Length - 1;
                }
                // Render the previous sprite
                spriteRenderer.sprite = objectSprites[objectSpriteIndex];
            }
            if (GUILayout.Button("Next Sprite"))
            {
                objectSpriteIndex++;
                // If the index has gone over the array, reset to zero
                if (objectSpriteIndex >= objectSprites.Length)
                {
                    objectSpriteIndex = 0;
                }
                // Render the next sprite
                spriteRenderer.sprite = objectSprites[objectSpriteIndex];
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

            if (GUILayout.Button("Close"))
            {
                menuOpen = false;
            }

            GUILayout.EndArea();


        }
        
    }
    

    public void OnMouseDown()
    {
        if (mouseOver)
        {
            isDragging = true;
        }
    }

    public void OnMouseUp()
    {
        isDragging = false;
    }

    void Update()
    {
        if (mouseOver && Input.GetMouseButton(1))
        {
            menuOpen = true;
        }
            if (isDragging && isDraggable )
        {
            mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
            transform.Translate(mousePos);
        }
    }
}
