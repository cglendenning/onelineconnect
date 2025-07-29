using UnityEngine;
using UnityEditor;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections.Generic;

public class MoanaWindowEditor
{
    [MenuItem("Moana Games/Reset the game")]
    static void Reset()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        File.Delete(Application.persistentDataPath + "/userInfo3.dat");
    }

    [MenuItem("Moana Games/Unlock all levels")]
    static void UnlockAllLevel()
    {
        int LEVEL_EACH_PACKAGE = LevelData.totalLevelsPerWorld;

        Dictionary<int, string> TotalLevelCrossed = new Dictionary<int, string>();
        Dictionary<int, int> currentLevel = new Dictionary<int, int>();

        string str = "";
        for(int i = 1; i <= LEVEL_EACH_PACKAGE + 1; i++)
        {
            str += i + (i == LEVEL_EACH_PACKAGE + 1 ? "" : ",");
        }

        for (int i = 1; i <= LevelData.worldNames.Length; i++)
        {
            TotalLevelCrossed.Add(i, str);
            currentLevel.Add(i, LEVEL_EACH_PACKAGE);
        }

        BinaryFormatter bf = new BinaryFormatter();

        FileStream f = File.Open(Application.persistentDataPath + "/userInfo3.dat", FileMode.OpenOrCreate);

        PlayerData.PlayerDataObj userData = new PlayerData.PlayerDataObj
        {
            levelcross = TotalLevelCrossed,
            currentLevel = currentLevel,
            totalhints = 100
        };

        bf.Serialize(f, userData);

        f.Close();
        PlayerPrefs.Save();
    }
}