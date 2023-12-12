using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RecordScript : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI recordText;

    public void Awake()
    {
        recordText = GetComponent<TextMeshProUGUI>();
    }

    public void Update()
    {
        recordText.text = "Record: " + FindObjectOfType<ScoreManager>().Record.ToString();
    }
}
