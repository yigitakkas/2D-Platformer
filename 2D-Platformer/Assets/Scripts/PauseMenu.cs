using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    private _2DPlatformer _2DPlatformer;
    private InputAction menu;
    public static bool isPaused=false;
    public bool controlsOpened;
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameObject controlsMenuUI;
    void Awake()
    {
        _2DPlatformer = new _2DPlatformer();
    }

    void Update()
    {
    }

    private void OnEnable()
    {
        menu = _2DPlatformer.Menu.Escape;
        menu.Enable();
        menu.performed += Pause;
    }

    private void OnDisable()
    {
        menu.Disable();
    }

    void Pause(InputAction.CallbackContext context)
    {
        isPaused = !isPaused;
        if(isPaused)
        {
            ActivateMenu();
        }
        else
        {
            DeactivateMenu();
            if(controlsOpened)
            {
                controlsMenuUI.SetActive(false);
            }
        }
    }

    public void DeactivateMenu()
    {
        Time.timeScale = 1f;
        pauseMenuUI.SetActive(false);
        //AudioListener.pause = false;
        isPaused = false;
    }

    public void ActivateMenu()
    {
        Time.timeScale = 0f;
        pauseMenuUI.SetActive(true);
        //AudioListener.pause = true;
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
        isPaused = false;
        SceneManager.LoadScene("MainMenu");
    }
}
