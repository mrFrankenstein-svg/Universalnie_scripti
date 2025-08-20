using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ObjectHub — пул/хранилище объектов.
/// - В инспекторе можно добавлять префабы в public List<GameObject> prefabs.
/// - Хранит не более MAX_HIDDEN (254) скрытых инстансов суммарно.
/// - Request(...) возвращает доступный объект (из пула) или создаёт новый.
/// - Release(obj) возвращает объект в хранилище (деактивирует) либо уничтожает его, если пул полон.
/// </summary>
public class ObjectHub : MonoBehaviour
{
    public static ObjectHub hub { get;  private set;}
    [SerializeField] const int MAX_HIDDEN = 254;

    [Header("Prefabs (можно добавлять сколько угодно)")]
    [SerializeField] List<GameObject> prefabs = new List<GameObject>();

    [Header("Опции")]
    [SerializeField] Transform hiddenContainer; // куда складывать скрытые объекты; если null, будет создан дочерний объект
    [SerializeField] bool keepScaleAndPositionOnRelease = false; // если true — не сбрасывать position/rotation/scale при Release

    // Внутренние структуры
    private Dictionary<GameObject, Queue<GameObject>> pools = new Dictionary<GameObject, Queue<GameObject>>();
    private Dictionary<string, GameObject> prefabByName = new Dictionary<string, GameObject>();
    private int totalHiddenCount = 0;

    private void Awake()
    {
        hub = this;
        if (hiddenContainer == null)
        {
            GameObject cont = new GameObject("ObjectHub_HiddenContainer");
            cont.transform.SetParent(transform);
            hiddenContainer = cont.transform;
        }

        // Инициализируем словари
        pools.Clear();
        prefabByName.Clear();

        foreach (var p in prefabs)
        {
            if (p == null) continue;
            if (!pools.ContainsKey(p))
                pools[p] = new Queue<GameObject>();

            string n = p.name;
            if (!prefabByName.ContainsKey(n))
                prefabByName[n] = p;
            else
                Debug.LogWarning($"ObjectHub: В списке prefabs есть несколько префабов с именем '{n}'. Используется первый.");
        }
    }

    #region API: Request / Release

    /// <summary>
    /// Запрос объекта по имени префаба (имя префаба должно совпадать с именем в инспекторе или с именем ассета).
    /// </summary>
    public GameObject Request(string prefabName)
    {
        if (string.IsNullOrEmpty(prefabName)) return null;
        if (prefabByName.TryGetValue(prefabName, out var prefab))
            return Request(prefab);

        Debug.LogWarning($"ObjectHub: Префаб с именем '{prefabName}' не зарегистрирован. Попытка найти по имени в сцене/ресурсах...");
        // Попробуем найти префаб в списке prefabs (вдруг имя совпадало и не добавлено в словарь)
        foreach (var p in prefabs)
        {
            if (p != null && p.name == prefabName)
            {
                prefabByName[p.name] = p;
                if (!pools.ContainsKey(p)) pools[p] = new Queue<GameObject>();
                return Request(p);
            }
        }

        Debug.LogError($"ObjectHub: Префаб '{prefabName}' не найден.");
        return null;
    }

    /// <summary>
    /// Запрос объекта по ссылке на префаб.
    /// </summary>
    public GameObject Request(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogError("ObjectHub: Request(null) — неверный префаб.");
            return null;
        }

        if (!pools.ContainsKey(prefab))
            pools[prefab] = new Queue<GameObject>();

        var queue = pools[prefab];
        GameObject instance = null;

        if (queue.Count > 0)
        {
            // достаём из пула
            instance = queue.Dequeue();
            totalHiddenCount = Mathf.Max(0, totalHiddenCount - 1);
            // Активируем и возвращаем
            instance.transform.SetParent(null);
            instance.SetActive(true);
        }
        else
        {
            // создаём новый
            instance = Instantiate(prefab);
            // обязательно имя инстанса как у префаба (требование)
            instance.name = prefab.name;
            // помечаем компонентом PooledObject, чтобы знать originPrefab при Release
            var pm = instance.GetComponent<PooledObject>();
            if (pm == null) pm = instance.AddComponent<PooledObject>();
            pm.originPrefab = prefab;
        }

        return instance;
    }

    /// <summary>
    /// Возвращает объект в хранилище (деактивирует и кладёт в пул). Если пул полон (MAX_HIDDEN) — уничтожает объект.
    /// </summary>
    public void Release(GameObject instance)
    {
        if (instance == null) return;

        var pm = instance.GetComponent<PooledObject>();
        if (pm == null || pm.originPrefab == null)
        {
            // Если нет метки, просто уничтожим — или можем попытаться угадать пул по имени
            Debug.LogWarning("ObjectHub: Попытка вернуть объект, который не был создан через ObjectHub. Уничтожаю.");
            Destroy(instance);
            return;
        }

        GameObject prefab = pm.originPrefab;

        if (!pools.ContainsKey(prefab))
            pools[prefab] = new Queue<GameObject>();

        if (totalHiddenCount >= MAX_HIDDEN)
        {
            // места нет — удаляем
            Destroy(instance);
            return;
        }

        // Подготовка: деактивируем, вращение/позиция/масштаб можно сбросить
        if (!keepScaleAndPositionOnRelease)
        {
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;
        }

        instance.SetActive(false);
        instance.transform.SetParent(hiddenContainer, false);

        pools[prefab].Enqueue(instance);
        totalHiddenCount++;
    }

    #endregion

    #region Утилиты управления префабами и предзагрузка

    /// <summary>
    /// Добавить префаб в список (и зарегистрировать пул).
    /// </summary>
    public void AddPrefab(GameObject prefab)
    {
        if (prefab == null) return;
        if (!prefabs.Contains(prefab)) prefabs.Add(prefab);
        if (!pools.ContainsKey(prefab)) pools[prefab] = new Queue<GameObject>();
        if (!prefabByName.ContainsKey(prefab.name)) prefabByName[prefab.name] = prefab;
    }

    /// <summary>
    /// Удалить префаб из списка. Существующие спрятанные объекты остаются в пуле.
    /// </summary>
    public void RemovePrefab(GameObject prefab)
    {
        if (prefab == null) return;
        prefabs.Remove(prefab);
        prefabByName.Remove(prefab.name);
        pools.Remove(prefab);
    }

    /// <summary>
    /// Предзагрузить (создать) N объектов данного префаба и положить в пул (не более лимита).
    /// </summary>
    public void Preload(GameObject prefab, int count)
    {
        if (prefab == null) return;
        if (!pools.ContainsKey(prefab)) pools[prefab] = new Queue<GameObject>();

        for (int i = 0; i < count; i++)
        {
            if (totalHiddenCount >= MAX_HIDDEN) break;
            GameObject inst = Instantiate(prefab);
            inst.name = prefab.name;
            var pm = inst.GetComponent<PooledObject>() ?? inst.AddComponent<PooledObject>();
            pm.originPrefab = prefab;
            inst.SetActive(false);
            inst.transform.SetParent(hiddenContainer, false);
            pools[prefab].Enqueue(inst);
            totalHiddenCount++;
        }
    }

    /// <summary>
    /// Вернуть текущий размер пула (сколько скрытых объектов всего).
    /// </summary>
    public int GetTotalHiddenCount() => totalHiddenCount;

    #endregion
}

/// <summary>
/// Вспомогательный компонент, который помечает инстанс и хранит ссылку на префаб-родитель.
/// </summary>
public class PooledObject : MonoBehaviour
{
    [HideInInspector] public GameObject originPrefab;
}
