using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class levelThumbnails : MonoBehaviour
{
    public bool set = false;
    public bool selected = false;
    public bool selectUpdated = false;
    public string levelPath = "";

    public levelMenu menu;
    public SpriteRenderer thumbnail;
    public SpriteRenderer outline;
    Sprite defaultSprite;

    Color unset; // original color
    private Color noColor = new Color(1, 1, 1, 1);
    private Color defaultColor = new Color(0.0f, 0.0f, 0.0f, 0.30f);

    // Start is called before the first frame update
    void Start()
    {
        thumbnail = this.gameObject.GetComponent<SpriteRenderer>();
        outline = (this.gameObject.transform.GetChild(0)).GetComponent<SpriteRenderer>();
        defaultSprite = thumbnail.sprite;

        // retrieve menu properties
        menu = (GameObject.Find("displaybackground")).GetComponent<levelMenu>();
    }

    // Update is called once per frame
    void Update()
    {
        // if set while not active anymore
        if (!selected && selectUpdated)
        {
            deselectObject();
        }
    }

    void OnMouseDown()
    {
        if (set)
        {
            selected = true;
            selectObject();
        }
    }
    public void setObject(string path)
    {
        string imagePath;
        Texture2D texture;
        Sprite sprite;

        // save the original level path
        levelPath = path;

        // set to level image
        imagePath = path.Replace(".txt", "");
        imagePath = imagePath.Replace("Assets/Resources/", "");

        sprite = Resources.Load<Sprite>(imagePath);
        thumbnail.sprite = sprite;
        thumbnail.color = noColor;

        set = true;
    }
    public void unsetObject()
    {
        // clear up thumbnail
        thumbnail.sprite = defaultSprite;
        thumbnail.color = defaultColor;
        
        // if it was previously set
        if (set)
        {
            // delete the level
            File.Delete(levelPath + ".meta");
            File.Delete(levelPath);

            string thumbnailPath = levelPath.Replace(".txt", ".png");
            File.Delete(thumbnailPath + ".meta");
            File.Delete(thumbnailPath);

            levelPath = "";

            // delete the thumbnail
            // delete the .meta files for the level and thumbnail

            selected = false;
            set = false;
        }
    }
    private void selectObject()
    {
        levelThumbnails current;

        // TODO: loop through rest of objects to make sure that they are not active
        foreach (GameObject obj in menu.panelList)
        {
            current = obj.GetComponent<levelThumbnails>();

            // if active and not the current object
            if (current.selected && (obj.name != (this.gameObject).name))
            {
                current.selected = false;
            }
        }

        // set outline transparency to be opaque
        Color color = outline.color;
        color.a = 1;
        outline.color = color;

        menu.selected = true;
        menu.selectedLevel = this.gameObject;
        menu.selectedLevelPath = levelPath;
        selectUpdated = true;
    }
    private void deselectObject()
    {
        // set outline transparency to be transparent
        Color color = outline.color;
        color.a = 0;
        outline.color = color;

        selectUpdated = false;
    }
}
