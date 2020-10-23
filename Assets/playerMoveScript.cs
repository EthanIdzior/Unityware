using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class playerMoveScript : MonoBehaviour
{
    public int movementFactor = 1;
    public int jumpFactor = 1;
    public Boolean moveLeft;
    public Boolean moveRight;
    public Boolean moveUp;
    public Boolean moveDown;
    public Boolean canJump;

    GUIscript playGUI;

    // Start is called before the first frame update
    void Start()
    {
        // Find the main camera
        GameObject mainCamera = GameObject.Find("Main Camera");
        playGUI = mainCamera.GetComponent<GUIscript>();
    }

    // Update is called once per frame
    void Update()
    {
        // Create a base step size that will be multiplied by the movement factor.
        int stepSize = 1;

        // Create a base jump size
        int jumpSize = 1;

        /*
         * General Note: This movement only works on GameObjects
         * Therefore it will not work using only a sprite.
         * 
         * Status:
         * Left = Working
         * Right = Working
         * Up = Working
         * Down = Working
         * Space = Not Working | Possible Solution being create a global scope private bool ex. isJumping
         * Currently not working because the jump runs too fast to see, it needs to run once every frame.
         */

        // Only work if playing the game
        if (playGUI.controller.playingGame)
        {
            // If W or up arrow is pressed move up
            // Note this does not currently work if key is held
            if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) && moveUp)
            {

                transform.position += new Vector3(0, stepSize * movementFactor, 0);

                if (!IsValidMove())
                {
                    transform.position -= new Vector3(0, stepSize * movementFactor, 0);
                }
            }

            // If A or Left Arrow is pressed move Left
            // Note this does not currently work if key is held
            else if ((Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) && moveLeft)
            {

                transform.position += new Vector3(-1 * (stepSize * movementFactor), 0, 0);

                if (!IsValidMove())
                {
                    transform.position -= new Vector3(-1 * (stepSize * movementFactor), 0, 0);
                }
            }

            // If S or Down Arrow is pressed move Down
            // Note this does not currently work if key is held
            else if ((Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) && moveDown)
            {

                transform.position += new Vector3(0, -1 * (stepSize * movementFactor), 0);

                if (!IsValidMove())
                {
                    transform.position -= new Vector3(0, -1 * (stepSize * movementFactor), 0);
                }
            }

            // If D or Right Arrow is pressed move Left
            // Note this does not currently work if key is held
            else if ((Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) && moveRight)
            {

                transform.position += new Vector3(stepSize * movementFactor, 0, 0);

                if (!IsValidMove())
                {
                    transform.position -= new Vector3(stepSize * movementFactor, 0, 0);
                }
            }

            // If Space is pressed move Jump
            // Note this does not currently work if key is held
            // #TODO
            else if (Input.GetKeyDown(KeyCode.Space) && canJump)
            {
                // Check to see if on the ground or an object. If valid, not a valid jump.
                transform.position += new Vector3(0, -1 * (stepSize * movementFactor), 0);
                if (IsValidMove())
                {
                    transform.position += new Vector3(0, stepSize * movementFactor, 0);
                    return;
                }

                // Reset the position for the original check
                transform.position += new Vector3(0, stepSize * movementFactor, 0);

                // Move up
                for (int i = 0; i < 10; i++)
                {
                    transform.position += new Vector3(0, (jumpSize * jumpFactor) / 10, 0);

                    if (!IsValidMove())
                    {
                        transform.position += new Vector3(0, -1 * ((jumpSize * jumpFactor) / 10), 0);
                    }
                }

                // Move down until colliding with an object.
                while (IsValidMove())
                {
                    transform.position += new Vector3(0, -1 * (jumpSize * jumpFactor), 0);
                }

                transform.position += new Vector3(0, jumpSize * jumpFactor, 0);

            }
        }

        
    }

    // isIsValidMove()
    // Returns true if the current position of the object is valid.
    // Returns false if out of bounds.
    bool IsValidMove()
    {
        // Make sure the object cannot move past (0, 0)
        // Making boundries bounded by (0,0) simplifies movement and scripts later
        foreach (Transform children in transform)
        {
            int roundX = Mathf.RoundToInt(children.transform.position.x);
            int roundY = Mathf.RoundToInt(children.transform.position.y);

            if (roundX < 0 || roundY < 0)
            {
                return false;
            }

            // Once collision is defined, it will go here.
        }

        return true;
    }
}
