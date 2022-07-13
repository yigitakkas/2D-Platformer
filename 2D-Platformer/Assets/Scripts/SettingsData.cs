using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SettingsData
{
    public int resolution;
    public int graphics;
    public int windowMode;
    //public float volume;

    public SettingsData (SettingsMenu settingsMenu)
    {
        resolution = settingsMenu.resolutionDropdown.value;
        graphics = settingsMenu.graphicsDropdown.value;
        //volume = settingsMenu.volumeSlider.value;
        windowMode = settingsMenu.windowModeDropdown.value;
    }
}
