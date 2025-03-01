using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class SaveSystem
{
    private static SaveData _saveData = new SaveData();

    [System.Serializable]
    public struct SaveData
    {
        public PlayerSaveData PlayerData;
        public GameSaveData GameData;
        public SceneEnemyData EnemyData;
    }

    public static string SaveFileName()
    {
        string saveFile = Application.persistentDataPath + "/save" + ".save";
        return saveFile;
    }

    public static void Save()
    {
        HandleSaveData();

        string saveFilePath = SaveFileName();
        File.WriteAllText(saveFilePath, JsonUtility.ToJson(_saveData, true));

        Debug.Log("Save file path: " + saveFilePath);

        string json = JsonUtility.ToJson(_saveData, true);
        string encryptedJson = EncryptionUtility.EncryptString(json);

        File.WriteAllText(saveFilePath, encryptedJson);
    }

    public static void HandleSaveData()
    {
        GameManager.Instance.PlayerController.Save(ref _saveData.PlayerData);
        GameManager.Instance.Save(ref _saveData.GameData);
        BoardManager boardManager = GameManager.Instance.BoardManager;
        if (boardManager != null)
        {
            boardManager.Save(ref _saveData.EnemyData);
        }
    }

    public static void Load()
    {
        string saveContent = File.ReadAllText(SaveFileName());
        string decryptedJson = EncryptionUtility.DecryptString(saveContent);
        _saveData = JsonUtility.FromJson<SaveData>(decryptedJson);

        /*        _saveData = JsonUtility.FromJson<SaveData>(saveContent);
        */
        HandleLoadData();
    }

    public static void HandleLoadData()
    {
        GameManager.Instance.PlayerController.Load(_saveData.PlayerData);
        GameManager.Instance.Load(_saveData.GameData);

        BoardManager boardManager = GameManager.Instance.BoardManager;
        if (boardManager != null)
        {
            boardManager.Load(_saveData.EnemyData);
        }
    }
    public static bool IsSaveFileEmpty()
    {
        string saveFilePath = SaveFileName();

        string saveContent = File.ReadAllText(saveFilePath);
        return string.IsNullOrEmpty(saveContent); // Check if file content is empty
    }


    public static bool DeleteSaveFileIfExists()
    {
        //delete save file text
        if (File.Exists(SaveFileName()))
        {
            //delete text inside the file
            File.WriteAllText(SaveFileName(), string.Empty);
            return true;
        }
        return false;
    }


}
