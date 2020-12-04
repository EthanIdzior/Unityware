using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playTrack : MonoBehaviour
{
    public bool isPlay3;
    public bool isPlayLevel;
    private List<string> levelList = new List<string>();
    
    // Start is called before the first frame update
    void Start()
    {
        isPlay3 = false;
        isPlayLevel = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Add a level to the levelList
    int addLevel(string levelPath) {
        if (!levelList.Contains(levelPath)) {
            levelPath.Add(levelPath);
            return 0;
        }

        else
        {
            return -1;
        }
    }

    // Pop level from levelList
    string nextLevel() {
        string newLevel = levelList[0];
        levelList.RemoveAt(0);

        return newLevel;
    }

    void clearLevels() {
        levelList.clear();
    }

    int levelsLeft() {
        return levelPath.Count;
    }

    public bool getPlay3() {
        return isPlay3;
    }

    public bool getPlayLevel() {
        return isPlayLevel;
    }

    public void setPlay3() {
        isPlay3 = true;
        isPlayLevel = false;
    }

    public void setPlayLevel() {
        isPlay3 = false;
        isPlayLevel = true;
    }

    public void clearPlayLevel() {
        isPlay3 = false;
        isPlayLevel = false;
    }
}
