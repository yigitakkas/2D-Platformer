using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using TMPro;
using System.Linq;
using UnityEngine.UI;
using System;

public class SettingsMenu : MonoBehaviour
{
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown graphicsDropdown;
    public TMP_Dropdown windowModeDropdown;
    public Slider volumeSlider;
    Resolution[] resolutions;

    private void Start()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        //int currentResolutionIndex = 0;

        for(int i=0; i<resolutions.Length;i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height + " @ " + resolutions[i].refreshRate + " Hz";
            options.Add(option);
            /*if(resolutions[i].Equals(Screen.currentResolution))
            {
                currentResolutionIndex = i;
            }*/
        }
        resolutionDropdown.AddOptions(options);
        LoadAllSettings();
        if (!PlayerPrefs.HasKey("musicVolume"))
        {
            PlayerPrefs.SetFloat("musicVolume", 1);
            LoadVolume();
        }
        else
        {
            LoadVolume();
        }
    }
    public void SetVolume(float volume)
    {
        AudioListener.volume = volumeSlider.value;
        SaveVolume();
    }

    private void SaveVolume()
    {
        PlayerPrefs.SetFloat("musicVolume", volumeSlider.value);
    }

    private void LoadVolume()
    {
        volumeSlider.value = PlayerPrefs.GetFloat("musicVolume");
    }

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }


    public void SetScreen(int screenIndex)
    {
        if(screenIndex==0)
        {
            Screen.fullScreen = true;
        }
        else if(screenIndex==1)
        {
            Screen.fullScreen = false;
        }
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height,Screen.fullScreen);
    }

    public void SaveAllSettings()
    {
        SaveSystem.SaveSettings(this);
    }

    public void LoadAllSettings()
    {
        SettingsData data = SaveSystem.LoadSettings();
        resolutionDropdown.value = data.resolution;
        graphicsDropdown.value = data.graphics;
        windowModeDropdown.value = data.windowMode;
        graphicsDropdown.RefreshShownValue();
    }
}
