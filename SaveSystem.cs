using UnityEngine;
using System.IO;
/*Пример использования
  

1. Любой класс для сохранения

        public class PlayerData
        {
            public string playerName;
            public int score;
            public float health;
        }

2. Сохранение

        PlayerData data = new PlayerData
        {
            playerName = "Alex",
            score = 150,
            health = 87.5f
        };

        SaveSystem.Save(data, "playerSave");


3. Загрузка

        PlayerData loaded = SaveSystem.Load<PlayerData>("playerSave");
        Debug.Log($"Имя: {loaded.playerName}, Очки: {loaded.score}, Здоровье: {loaded.health}");


4. Удаление сохранения

        SaveSystem.Delete("playerSave");

*/
public static class SaveSystem

{
    //  Сохраняем любой класс в JSON
    public static void Save<T>(T data, string fileName)
    {
        string path = GetPath(fileName);
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
        Debug.Log($"[SaveSystem] Данные сохранены в {path}");
    }

    //  Загружаем JSON в любой тип
    public static T Load<T>(string fileName) where T : new()
    {
        string path = GetPath(fileName);
        if (!File.Exists(path))
        {
            Debug.LogWarning($"[SaveSystem] Файл {fileName} не найден. Возвращаем новый объект.");
            return new T(); // Возвращаем пустой объект
        }

        string json = File.ReadAllText(path);
        T data = JsonUtility.FromJson<T>(json);
        return data;
    }

    //  Удаляем сохранение
    public static void Delete(string fileName)
    {
        string path = GetPath(fileName);
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"[SaveSystem] Файл {fileName} удалён.");
        }
    }

    //  Путь до файла
    private static string GetPath(string fileName)
    {
        return Path.Combine(Application.persistentDataPath, fileName + ".json");
    }
}
