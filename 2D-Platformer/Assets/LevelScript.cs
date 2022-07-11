using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelScript : MonoBehaviour
{
    public TextMeshProUGUI completedPointsText;
    public void PassLevel() //finish line'a gelince çalýþacak
    {
        int currentLevel = SceneManager.GetActiveScene().buildIndex;
        if(currentLevel >= PlayerPrefs.GetInt("levelsUnlocked"))
        {
            PlayerPrefs.SetInt("levelsUnlocked", currentLevel+1);
        }
    }
    public void Setup(int score)
    {
        gameObject.SetActive(true);
        completedPointsText.text = score.ToString() + " POINTS";
    }
    public void NextButton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    public void ExitButton()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
