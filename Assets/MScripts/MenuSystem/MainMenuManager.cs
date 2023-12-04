using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public GameObject optionsMenu, levelSelectMenu;

    public void OnClickPlay()
    {
        // SceneManager.LoadScene("MainLevel");
        levelSelectMenu.SetActive(true);
    }

    public void OnClickOptions()
    {
        optionsMenu.SetActive(true);
    }

    public void OnClickQuit()
    {
        Application.Quit();
    }

    public void OnClickBackOptions()
    {
        optionsMenu.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
