using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using TMPro;
using System.Linq;

public class SettingsMenu : MonoBehaviour
{
    public AudioMixer audioMixer;
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown graphicsDropdown;
    Resolution[] resolutions;
    List<Resolution> uniqueResolutions = new List<Resolution>();

    private void Start()
    {
        graphicsDropdown.value = 5;
        graphicsDropdown.RefreshShownValue();

        resolutionDropdown.ClearOptions();
        //List<string> options = new List<string>();
        List<string> res = new List<string>();
        int currentResolutionIndex = 0;
        int counter = 0;
        foreach(Resolution sr in Screen.resolutions)
        {
            string newRes = string.Format("{0}x{1}", sr.width, sr.height);
            if (!res.Contains(newRes))
            {
                res.Add(newRes);
                currentResolutionIndex = counter;
                counter++;
            }
        }
        for(int i=0;i<10;i++)
        {
            Debug.Log(res[i] + " ");
        }
        resolutionDropdown.AddOptions(res);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        /*for(int i=0; i<resolutions.Length;i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height + " @ " + resolutions[i].refreshRate + " Hz";
            options.Add(option);

            if(resolutions[i].Equals(Screen.currentResolution))
            {
                currentResolutionIndex = i;
            }
        }*/

        //resolutionDropdown.AddOptions(options);
    }
    public void SetVolume(float volume)
    {
        audioMixer.SetFloat("volume", volume);
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
        Screen.SetResolution(resolution.width, resolution.height,Screen.fullScreen);
    }
}
