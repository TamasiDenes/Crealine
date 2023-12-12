using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] LayerMenu menu;
    // actual score: the number of active rewards
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

            if (score == MaxScore)
            {
                Time.timeScale = 0f;

                OnLevelComplete();
            }
        }
    }
    // total number of rewards
    public int MaxScore { get; set; }
    // the highest achieved score during the gameplay
    public int Record { get; set; }


    private void OnLevelComplete()
    {
        if (LevelSelectionMenuManager.currLevel == LevelSelectionMenuManager.unlockedLevels)
        {
            LevelSelectionMenuManager.unlockedLevels++;
            PlayerPrefs.SetInt("UnlockedLevels", LevelSelectionMenuManager.unlockedLevels);
        }

        FindObjectOfType<AudioManager>().Play("WinLevel");
        menu.SuccessMenu();
    }

    public bool IsGameEnd()
    {
        return Score == MaxScore;
    }
}
