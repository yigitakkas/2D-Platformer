using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOverScreen : MonoBehaviour
{
    public TextMeshProUGUI endGamePointsText;
    public static GameOverScreen _gameOverScreen;
    private void Start()
    {
        if (_gameOverScreen == null)
        {
            _gameOverScreen = this;
        }
    }
    public void Setup(int score)
    {
        gameObject.SetActive(true);
        endGamePointsText.text = score.ToString() + " POINTS";
    }

    public void RestartButton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ExitButton()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
