using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem
{
    public static void SaveSettings (SettingsMenu settingsMenu)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/settings.fun";
        FileStream stream = new FileStream(path, FileMode.Create);

        SettingsData data = new SettingsData(settingsMenu);

        formatter.Serialize(stream, data);
        stream.Close();
    }

    public static SettingsData LoadSettings ()
    {
        string path = Application.persistentDataPath + "/settings.fun";
        if(File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);
            SettingsData data = formatter.Deserialize(stream) as SettingsData;
            stream.Close();
            return data;
        }
        else
        {
            Debug.LogError("save file not found in " + path);
            return null;
        }
    }
}
