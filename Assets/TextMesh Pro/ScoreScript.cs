using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreScript : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;

    public void Awake()
    {
        scoreText = GetComponent<TextMeshProUGUI>();
    }

    public void Update()
    {
        ScoreManager scoreManager = FindObjectOfType<ScoreManager>();

        scoreText.text = "Score: " + scoreManager.Score.ToString() + "/" + scoreManager.MaxScore.ToString();
    }
}
