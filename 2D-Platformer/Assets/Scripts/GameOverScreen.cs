using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOverScreen : MonoBehaviour
{
    public TextMeshProUGUI endGamePointsText;
    public static GameOverScreen _gameOverScreen;
    public static bool gameOverUp;
    private void Start()
    {
        if (_gameOverScreen == null)
        {
            _gameOverScreen = this;
        }
        gameOverUp = true;
    }
    public void Setup(int score)
    {
        gameObject.SetActive(true);
        endGamePointsText.text = score.ToString() + " POINTS";
    }

    public void RestartButton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        gameOverUp = false;
    }

    public void ExitButton()
    {
        SceneManager.LoadScene("MainMenu");
        gameOverUp = false;
    }
}
