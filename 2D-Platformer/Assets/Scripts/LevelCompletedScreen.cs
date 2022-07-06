using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelCompletedScreen : MonoBehaviour
{
    public TextMeshProUGUI completedPointsText;
    public static LevelCompletedScreen levelCompletedScreen;
    public int iLevelToLoad;
    public string sLevelToLoad;
    public bool useIntegerToLoadLevel;
    // Start is called before the first frame update
    void Start()
    {
        if(levelCompletedScreen == null)
        {
            levelCompletedScreen = this;
        }
    }

    public void Setup(int score)
    {
        gameObject.SetActive(true);
        completedPointsText.text = score.ToString() + " POINTS";
    }
    public void NextLevelButton()
    {
        LoadLevel();
    }

    public void ExitButton()
    {

    }

    public void LoadLevel()
    {
        if (useIntegerToLoadLevel)
        {
            SceneManager.LoadScene(iLevelToLoad);
        }
        else
        {
            SceneManager.LoadScene(sLevelToLoad);
        }
    }
}
