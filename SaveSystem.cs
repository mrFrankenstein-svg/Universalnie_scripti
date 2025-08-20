using UnityEngine;
using System.IO;
/*������ �������������
  

1. ����� ����� ��� ����������

        public class PlayerData
        {
            public string playerName;
            public int score;
            public float health;
        }

2. ����������

        PlayerData data = new PlayerData
        {
            playerName = "Alex",
            score = 150,
            health = 87.5f
        };

        SaveSystem.Save(data, "playerSave");


3. ��������

        PlayerData loaded = SaveSystem.Load<PlayerData>("playerSave");
        Debug.Log($"���: {loaded.playerName}, ����: {loaded.score}, ��������: {loaded.health}");


4. �������� ����������

        SaveSystem.Delete("playerSave");

*/
public static class SaveSystem

{
    //  ��������� ����� ����� � JSON
    public static void Save<T>(T data, string fileName)
    {
        string path = GetPath(fileName);
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
        Debug.Log($"[SaveSystem] ������ ��������� � {path}");
    }

    //  ��������� JSON � ����� ���
    public static T Load<T>(string fileName) where T : new()
    {
        string path = GetPath(fileName);
        if (!File.Exists(path))
        {
            Debug.LogWarning($"[SaveSystem] ���� {fileName} �� ������. ���������� ����� ������.");
            return new T(); // ���������� ������ ������
        }

        string json = File.ReadAllText(path);
        T data = JsonUtility.FromJson<T>(json);
        return data;
    }

    //  ������� ����������
    public static void Delete(string fileName)
    {
        string path = GetPath(fileName);
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"[SaveSystem] ���� {fileName} �����.");
        }
    }

    //  ���� �� �����
    private static string GetPath(string fileName)
    {
        return Path.Combine(Application.persistentDataPath, fileName + ".json");
    }
}
