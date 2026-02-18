using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class SaveSystem
{
    public static void SaveOptions()
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/player.opciones";
        FileStream stream = new FileStream(path, FileMode.Create);

        PlayerData optionData = new PlayerData(DataType.Options);   

        formatter.Serialize(stream, optionData);
        stream.Close();
    }

    public static PlayerData LoadOptions()
    {
        string path = Application.persistentDataPath + "/player.opciones";
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);
            PlayerData optionData = (PlayerData)formatter.Deserialize(stream);

            stream.Close();
            return optionData;
        } else
        {
            Debug.LogError("Option file not found in " + path);
            return null;    
        }
    }

    public static void SaveGamePref()
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/player.prefs";
        FileStream stream = new FileStream(path, FileMode.Create);

        PlayerData prefData = new PlayerData(DataType.Gameplay);

        formatter.Serialize(stream, prefData);
        stream.Close();
    }
    public static PlayerData LoadPrefs()
    {
        string path = Application.persistentDataPath + "/player.prefs";
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);
            PlayerData prefData = (PlayerData)formatter.Deserialize(stream);

            stream.Close();
            return prefData;
        }
        else
        {
            Debug.LogError("Pref file not found in " + path);
            return null;
        }
    }
}
