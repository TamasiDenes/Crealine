using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Player Player01;
    [SerializeField] private Player Player02;

    [SerializeField] private LayerMenu menu;

    [SerializeField] private InputAction switchPlayer01;
    [SerializeField] private InputAction switchPlayer02;

    private void OnEnable()
    {
        switchPlayer01.Enable();
        switchPlayer02.Enable();
    }

    public void OnDisable()
    {
        switchPlayer01.Disable();
        switchPlayer02.Disable();
    }

    // Singleton
    private static GameManager instance;

    public static GameManager Instance
    {
        get 
        {
            if (instance == null)
                Debug.Log("GameManager is null!");

            return instance; 
        }
    }

    private void Awake()
    {
        instance = this;
        switchPlayer01.performed += OnSwitchPlayer01;
        switchPlayer02.performed += OnSwitchPlayer02;

    }

    private void OnSwitchPlayer01(InputAction.CallbackContext context)
    {
        Player01.SwitchMoving();
    }

    private void OnSwitchPlayer02(InputAction.CallbackContext context)
    {
        Player02.SwitchMoving();
    }

    // aktuális eredmény - mennyi reward van aktiválva
    int score = 0;
    public int Score
    {
        get
        {
            return score;
        }
        set
        {
            score = value;
            if (Record < score)
                Record = score;

            if(score == MaxScore)
            {
                Time.timeScale = 0f;

                OnLevelComplete();
            }
        }
    }

    private void OnLevelComplete()
    {
        if (LevelSelectionMenuManager.currLevel == LevelSelectionMenuManager.unlockedLevels)
        {
            LevelSelectionMenuManager.unlockedLevels++;
            PlayerPrefs.SetInt("UnlockedLevels", LevelSelectionMenuManager.unlockedLevels);
        }

        menu.SuccessMenu();

    }

    internal bool IsGameEnd()
    {
        return Score == MaxScore;
    }

    public int MaxScore = 0;

    public int Record = 0;
}
