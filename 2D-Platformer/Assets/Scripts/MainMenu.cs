using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("FirstLevel");
    }
    public void QuitGame()
    {
        Debug.Log("QUIT GAME");
        Application.Quit();
    }
}
