using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SettingsData
{
    public int resolution;
    public int graphics;
    public int windowMode;

    public SettingsData (SettingsMenu settingsMenu)
    {
        resolution = settingsMenu.resolutionDropdown.value;
        graphics = settingsMenu.graphicsDropdown.value;
        windowMode = settingsMenu.windowModeDropdown.value;
    }
}
