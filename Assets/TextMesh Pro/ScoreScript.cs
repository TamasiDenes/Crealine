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
        scoreText.text = "Score: " + GameManager.Instance.Score.ToString() + "/" + GameManager.Instance.MaxScore.ToString();
    }
}
