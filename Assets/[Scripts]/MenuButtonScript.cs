using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtonScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void MenuButtonClicked()
    {
        SceneManager.LoadScene("Start");
    }

    public void ControlsButtonClicked()
    {
        SceneManager.LoadScene("Controls");
    }

    public void PlayButtonClicked()
    {
        SceneManager.LoadScene("Main");
    }

    public void Exit()
    {
        Application.Quit();
    }
}
