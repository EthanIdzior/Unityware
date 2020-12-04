﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playTrack : MonoBehaviour
{
    public bool isPlay3;
    public bool isPlayLevel;
    public List<string> levelList = new List<string>();
    
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
    public int addLevel(string levelPath) {
        if (!levelList.Contains(levelPath)) {
            levelList.Add(levelPath);
            return 0;
        }

        else
        {
            return -1;
        }
    }

    // Pop level from levelList
    public string nextLevel() {
        string newLevel = levelList[0];
        levelList.RemoveAt(0);

        return newLevel;
    }

    public void clearLevels() {
        levelList.Clear();
    }

    public int levelsLeft() {
        return levelList.Count;
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
