using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public GameObject[] levels;

    // Start is called before the first frame update
    void Start()
    {
        // ha k�rbe�rt�nk, akkor az els� p�ly�t t�ltj�k be
        if (LevelSelectionMenuManager.currLevel == levels.Length)
            LevelSelectionMenuManager.currLevel = 0;

        for (int i = 0; i < levels.Length; i++)
        {
            if(i == LevelSelectionMenuManager.currLevel)
                levels[i].SetActive(true);
            else
                levels[i].SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
