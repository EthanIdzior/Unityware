using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class objectDrag : MonoBehaviour
{
    private bool isDragging;
    bool menuOpen = false;
    public bool isDraggable = false;
    public bool mouseOver;
    Vector2 mousePos;

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
            GUILayout.TextField("The Current Value of Draggable is " + isDraggable);
            if (GUILayout.Button("Set Draggable"))
            {
                isDraggable = !isDraggable;
            }

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
