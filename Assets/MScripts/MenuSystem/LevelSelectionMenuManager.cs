using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectionMenuManager : MonoBehaviour
{
    public static int currLevel;
    public static int unlockedLevels;
    public  LevelObject[] levelObjects;

    public void OnClickLevel(int levelNum)
    {
        currLevel = levelNum;
        Time.timeScale = 1.0f;
        SceneManager.LoadScene("GameScene");
    }

    public void OnClickBack()
    {
        this.gameObject.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        unlockedLevels = PlayerPrefs.GetInt("UnlockedLevels", 0);
        for (int i = 0; i < levelObjects.Length; i++)
        {
            if(unlockedLevels > i)
            {
                levelObjects[i].levelButton.image.color = Color.yellow;

                levelObjects[i].levelButton.interactable = true;
            }
            else if(unlockedLevels == i)
            {
                levelObjects[i].levelButton.image.color = Color.white;

                levelObjects[i].levelButton.interactable = true;
            }
            else
            {
                levelObjects[i].levelButton.image.color = Color.grey;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
