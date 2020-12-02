using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playTrack : MonoBehaviour
{
    public bool isPlay3;
    public bool isPlayLevel;
    
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
