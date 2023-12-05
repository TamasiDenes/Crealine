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
        scoreText.text = "Score: " + ScoreManager.Instance.Score.ToString() + "/" + ScoreManager.Instance.MaxScore.ToString();
    }
}
