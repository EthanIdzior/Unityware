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
    private float originalSpriteX;
    private float originalOutlineX;
    public int thumbnailIndex = 0;
    public int outlineIndex = 1;

    Color unset; // original color
    private Color noColor = new Color(1, 1, 1, 1);
    private Color defaultColor = new Color(0.0f, 0.0f, 0.0f, 0.30f);

    // Start is called before the first frame update
    void Start()
    {
        thumbnail = (this.gameObject.transform.GetChild(thumbnailIndex)).GetComponent<SpriteRenderer>();
        outline = (this.gameObject.transform.GetChild(outlineIndex)).GetComponent<SpriteRenderer>();
        outline.color = new Color(255, 216, 0, 0);
        defaultSprite = thumbnail.sprite;
        originalSpriteX = thumbnail.bounds.size.x;
        originalOutlineX = outline.bounds.size.x;

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
    private void resizeSprite(float targetSize, SpriteRenderer renderer, int index)
    {
        // resize the thumbnail
        var bounds = renderer.sprite.bounds;
        var factor = targetSize / bounds.size.x;
        // this.gameObject.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f); // can't do or bounds will be skewed
        (this.gameObject.transform.GetChild(index)).transform.localScale = new Vector3(factor * 4, factor * 4, factor * 4);
    }
    public void setObject(string path)
    {
        string imagePath;
        Sprite sprite;

        // save the original level path
        levelPath = path;

        // set to level image
        imagePath = path.Replace(".txt", "");
        imagePath = imagePath.Replace("Assets/Resources/", "");

        sprite = Resources.Load<Sprite>(imagePath);
        thumbnail.sprite = sprite;
        thumbnail.color = noColor;

        // resize the thumbnail and outline
        resizeSprite(originalOutlineX, outline, outlineIndex);
        if (thumbnail.sprite != null)
        {
            resizeSprite(originalSpriteX, thumbnail, thumbnailIndex);
        }
        else
        {
            unsetObject();
            return;
        }

        set = true;
    }
    public string unsetObject()
    {
        return unsetObject(false);
    }
    public string unsetObject(bool loadingThumbnail)
    {
        string deletedPath = "";
        // clear up thumbnail
        thumbnail.sprite = defaultSprite;
        thumbnail.color = defaultColor;

        // if it was previously set
        if (set)
        {
            if (!loadingThumbnail)
            {
                // delete the level
                File.Delete(levelPath + ".meta");
                File.Delete(levelPath);

                // delete the thumbnail
                // delete the .meta files for the level and thumbnail
                string thumbnailPath = levelPath.Replace(".txt", ".png");
                File.Delete(thumbnailPath + ".meta");
                File.Delete(thumbnailPath);

                deletedPath = levelPath;
                levelPath = "";
            }
            else
            {
                Resources.Load<Sprite>("loading.png"); // set sprite to the loading image
                thumbnail.color = noColor;
            }

            // reset the scale for the sprites
            thumbnail.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            outline.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

            selected = false;
            set = false;
        }

        return deletedPath;
    }
    private void selectObject()
    {
        levelThumbnails current;

        // loop through rest of objects to make sure that they are not active
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
    public void deselectObject()
    {
        // set outline transparency to be transparent
        Color color = outline.color;
        color.a = 0;
        outline.color = color;

        selectUpdated = false;
    }
}
