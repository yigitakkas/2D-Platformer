using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public bool controlsOpened;
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameObject controlsMenuUI;
    public PlayerController playerController;


    public void DeactivateMenu()
    {
        playerController.GetComponent<PlayerInput>().SwitchCurrentActionMap("Player");
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
    }

    public void ActivateMenu()
    {
        playerController.GetComponent<PlayerInput>().SwitchCurrentActionMap("UI");
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Controls()
    {
        controlsOpened = true;
    }

    public void ControlsBack()
    {
        controlsOpened = false;
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;
        playerController.isPaused = false;
        SceneManager.LoadScene("MainMenu");
    }
}
