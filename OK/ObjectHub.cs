using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ObjectHub � ���/��������� ��������.
/// - � ���������� ����� ��������� ������� � public List<GameObject> prefabs.
/// - ������ �� ����� MAX_HIDDEN (254) ������� ��������� ��������.
/// - Request(...) ���������� ��������� ������ (�� ����) ��� ������ �����.
/// - Release(obj) ���������� ������ � ��������� (������������) ���� ���������� ���, ���� ��� �����.
/// </summary>
public class ObjectHub : MonoBehaviour
{
    public static ObjectHub hub { get;  private set;}
    [SerializeField] const int MAX_HIDDEN = 254;

    [Header("Prefabs (����� ��������� ������� ������)")]
    [SerializeField] List<GameObject> prefabs = new List<GameObject>();

    [Header("�����")]
    [SerializeField] Transform hiddenContainer; // ���� ���������� ������� �������; ���� null, ����� ������ �������� ������
    [SerializeField] bool keepScaleAndPositionOnRelease = false; // ���� true � �� ���������� position/rotation/scale ��� Release

    // ���������� ���������
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

        // �������������� �������
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
                Debug.LogWarning($"ObjectHub: � ������ prefabs ���� ��������� �������� � ������ '{n}'. ������������ ������.");
        }
    }

    #region API: Request / Release

    /// <summary>
    /// ������ ������� �� ����� ������� (��� ������� ������ ��������� � ������ � ���������� ��� � ������ ������).
    /// </summary>
    public GameObject Request(string prefabName)
    {
        if (string.IsNullOrEmpty(prefabName)) return null;
        if (prefabByName.TryGetValue(prefabName, out var prefab))
            return Request(prefab);

        Debug.LogWarning($"ObjectHub: ������ � ������ '{prefabName}' �� ���������������. ������� ����� �� ����� � �����/��������...");
        // ��������� ����� ������ � ������ prefabs (����� ��� ��������� � �� ��������� � �������)
        foreach (var p in prefabs)
        {
            if (p != null && p.name == prefabName)
            {
                prefabByName[p.name] = p;
                if (!pools.ContainsKey(p)) pools[p] = new Queue<GameObject>();
                return Request(p);
            }
        }

        Debug.LogError($"ObjectHub: ������ '{prefabName}' �� ������.");
        return null;
    }

    /// <summary>
    /// ������ ������� �� ������ �� ������.
    /// </summary>
    public GameObject Request(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogError("ObjectHub: Request(null) � �������� ������.");
            return null;
        }

        if (!pools.ContainsKey(prefab))
            pools[prefab] = new Queue<GameObject>();

        var queue = pools[prefab];
        GameObject instance = null;

        if (queue.Count > 0)
        {
            // ������ �� ����
            instance = queue.Dequeue();
            totalHiddenCount = Mathf.Max(0, totalHiddenCount - 1);
            // ���������� � ����������
            instance.transform.SetParent(null);
            instance.SetActive(true);
        }
        else
        {
            // ������ �����
            instance = Instantiate(prefab);
            // ����������� ��� �������� ��� � ������� (����������)
            instance.name = prefab.name;
            // �������� ����������� PooledObject, ����� ����� originPrefab ��� Release
            var pm = instance.GetComponent<PooledObject>();
            if (pm == null) pm = instance.AddComponent<PooledObject>();
            pm.originPrefab = prefab;
        }

        return instance;
    }

    /// <summary>
    /// ���������� ������ � ��������� (������������ � ����� � ���). ���� ��� ����� (MAX_HIDDEN) � ���������� ������.
    /// </summary>
    public void Release(GameObject instance)
    {
        if (instance == null) return;

        var pm = instance.GetComponent<PooledObject>();
        if (pm == null || pm.originPrefab == null)
        {
            // ���� ��� �����, ������ ��������� � ��� ����� ���������� ������� ��� �� �����
            Debug.LogWarning("ObjectHub: ������� ������� ������, ������� �� ��� ������ ����� ObjectHub. ���������.");
            Destroy(instance);
            return;
        }

        GameObject prefab = pm.originPrefab;

        if (!pools.ContainsKey(prefab))
            pools[prefab] = new Queue<GameObject>();

        if (totalHiddenCount >= MAX_HIDDEN)
        {
            // ����� ��� � �������
            Destroy(instance);
            return;
        }

        // ����������: ������������, ��������/�������/������� ����� ��������
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

    #region ������� ���������� ��������� � ������������

    /// <summary>
    /// �������� ������ � ������ (� ���������������� ���).
    /// </summary>
    public void AddPrefab(GameObject prefab)
    {
        if (prefab == null) return;
        if (!prefabs.Contains(prefab)) prefabs.Add(prefab);
        if (!pools.ContainsKey(prefab)) pools[prefab] = new Queue<GameObject>();
        if (!prefabByName.ContainsKey(prefab.name)) prefabByName[prefab.name] = prefab;
    }

    /// <summary>
    /// ������� ������ �� ������. ������������ ���������� ������� �������� � ����.
    /// </summary>
    public void RemovePrefab(GameObject prefab)
    {
        if (prefab == null) return;
        prefabs.Remove(prefab);
        prefabByName.Remove(prefab.name);
        pools.Remove(prefab);
    }

    /// <summary>
    /// ������������� (�������) N �������� ������� ������� � �������� � ��� (�� ����� ������).
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
    /// ������� ������� ������ ���� (������� ������� �������� �����).
    /// </summary>
    public int GetTotalHiddenCount() => totalHiddenCount;

    #endregion
}

/// <summary>
/// ��������������� ���������, ������� �������� ������� � ������ ������ �� ������-��������.
/// </summary>
public class PooledObject : MonoBehaviour
{
    [HideInInspector] public GameObject originPrefab;
}
