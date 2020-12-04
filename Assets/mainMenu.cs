using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class mainMenu : MonoBehaviour
{
    public string levelEditor;
    // Start is called before the first frame update
    void Start()
    {
        GameObject canvas = GameObject.Find("Canvas");
        
        GameObject playButton = canvas.gameObject.transform.GetChild(1).gameObject;
        GameObject play3Button = canvas.gameObject.transform.GetChild(2).gameObject;
        GameObject quitButton = canvas.gameObject.transform.GetChild(3).gameObject;

        playButton.GetComponentInChildren<UnityEngine.UI.Button>().onClick.AddListener(playLevel);
        play3Button.GetComponentInChildren<UnityEngine.UI.Button>().onClick.AddListener(createLevel);
        quitButton.GetComponentInChildren<UnityEngine.UI.Button>().onClick.AddListener(quitGame);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void playLevel()
    {
        GameObject canvas = GameObject.Find("Canvas");

        // Create objects for each button
        GameObject playButton = canvas.gameObject.transform.GetChild(1).gameObject;
        GameObject play3Button = canvas.gameObject.transform.GetChild(2).gameObject;
        GameObject quitButton = canvas.gameObject.transform.GetChild(3).gameObject;

        // Change Button Text
        playButton.GetComponentInChildren<UnityEngine.UI.Text>().text = "Select Level";
        play3Button.GetComponentInChildren<UnityEngine.UI.Text>().text = "Play 3 Random Levels";
        quitButton.GetComponentInChildren<UnityEngine.UI.Text>().text = "Go Back";

        // Remove Listeners
        playButton.GetComponentInChildren<UnityEngine.UI.Button>().onClick.RemoveListener(playLevel);
        play3Button.GetComponentInChildren<UnityEngine.UI.Button>().onClick.RemoveListener(createLevel);
        quitButton.GetComponentInChildren<UnityEngine.UI.Button>().onClick.RemoveListener(quitGame);

        // Add new functions
        playButton.GetComponentInChildren<UnityEngine.UI.Button>().onClick.AddListener(selectLevel);
        play3Button.GetComponentInChildren<UnityEngine.UI.Button>().onClick.AddListener(playThree);
        quitButton.GetComponentInChildren<UnityEngine.UI.Button>().onClick.AddListener(goBack);
    }

    public void createLevel()
    {
        GameObject playobj = GameObject.Find("PlayModeObj");
        playobj.GetComponent<playTrack>().clearPlayLevel();
        DontDestroyOnLoad (playobj.transform.gameObject);
        SceneManager.LoadScene(levelEditor);
    }

    public void quitGame()
    {
        Application.Quit();

    }

    public void selectLevel()
    {
        
    }

    public void playThree()
    {
        GameObject playobj = GameObject.Find("PlayModeObj");
        playobj.GetComponent<playTrack>().setPlay3();

        DontDestroyOnLoad (playobj.transform.gameObject);
        SceneManager.LoadScene(levelEditor);
        //camera.GetComponent<GUIscript>().playMode = true;
        //camera.GetComponent<GUIscript>().levelsLeft = 3;
    }
    
    public void goBack()
    {
        GameObject canvas = GameObject.Find("Canvas");

        // Create objects for each button
        GameObject playButton = canvas.gameObject.transform.GetChild(1).gameObject;
        GameObject play3Button = canvas.gameObject.transform.GetChild(2).gameObject;
        GameObject quitButton = canvas.gameObject.transform.GetChild(3).gameObject;

        // Change Button Text
        playButton.GetComponentInChildren<UnityEngine.UI.Text>().text = "Play Levels";
        play3Button.GetComponentInChildren<UnityEngine.UI.Text>().text = "Create Levels";
        quitButton.GetComponentInChildren<UnityEngine.UI.Text>().text = "Quit Game";

        // Remove Listeners
        playButton.GetComponentInChildren<UnityEngine.UI.Button>().onClick.RemoveListener(selectLevel);
        play3Button.GetComponentInChildren<UnityEngine.UI.Button>().onClick.RemoveListener(playThree);
        quitButton.GetComponentInChildren<UnityEngine.UI.Button>().onClick.RemoveListener(goBack);

        // Add Listeners
        playButton.GetComponentInChildren<UnityEngine.UI.Button>().onClick.AddListener(playLevel);
        play3Button.GetComponentInChildren<UnityEngine.UI.Button>().onClick.AddListener(createLevel);
        quitButton.GetComponentInChildren<UnityEngine.UI.Button>().onClick.AddListener(quitGame);
    }
}
