using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Player Player01;
    [SerializeField] private Player Player02;

    [SerializeField] private LayerMenu menu;

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
