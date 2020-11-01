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
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void playLevel()
    {

    }

    public void createLevel()
    {
        SceneManager.LoadScene(levelEditor);
    }

    public void quitGame()
    {
        Application.Quit();

    }
}
