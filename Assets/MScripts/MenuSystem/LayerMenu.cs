using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class LayerMenu : MonoBehaviour
{
    bool isPaused = false;

    [SerializeField] private InputAction inputAction;
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameObject successMenuUI;

    private void OnEnable()
    {
        inputAction.Enable();
    }

    public void OnDisable()
    {
        inputAction.Disable();
    }

    // Start is called before the first frame update
    void Awake()
    {
        inputAction.performed += OnEsc;
    }

    public void OnEsc(InputAction.CallbackContext context)
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            if (FindObjectOfType<ScoreManager>().IsGameEnd())
                SuccessMenu();
            else
                Pause();
        }
        else
            Resume();
    }

    public void SuccessMenu()
    {
        successMenuUI.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;

    }

    public void Resume()
    {
        successMenuUI.SetActive(value: false);
        pauseMenuUI.SetActive(value: false);
        Time.timeScale = 1f;

    }

    public void NextLevel()
    {
        LevelSelectionMenuManager.currLevel++;
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
        Time.timeScale = 1f;

    }

    public void Restart()
    {
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
        Time.timeScale = 1f;

    }

    public void LoadMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
