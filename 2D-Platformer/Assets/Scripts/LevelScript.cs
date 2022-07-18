using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelScript : MonoBehaviour
{
    public static bool levelCompleteUp;
    public TextMeshProUGUI completedPointsText;
    public void PassLevel() //finish line'a gelince çalýþacak
    {
        int currentLevel = SceneManager.GetActiveScene().buildIndex;
        if(currentLevel >= PlayerPrefs.GetInt("levelsUnlocked"))
        {
            if(currentLevel < 6)
            {
                PlayerPrefs.SetInt("levelsUnlocked", currentLevel + 1);
            }
            else if(currentLevel==6)
            {
                PlayerPrefs.SetInt("levelsUnlocked", currentLevel);
            }
        }
    }
    private void Start()
    {
        levelCompleteUp = true;
    }
    public void Setup(int score)
    {
        gameObject.SetActive(true);
        completedPointsText.text = score.ToString() + " POINTS";
    }
    public void NextButton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        levelCompleteUp = false;
    }
    public void ExitButton()
    {
        SceneManager.LoadScene("MainMenu");
        levelCompleteUp = false;
    }
}
