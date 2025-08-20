using System;
using UnityEngine;

[Tooltip("Этот скрипт добавляет обьекты на сцену.")]
public class Spawner : MonoBehaviour
{

    [SerializeField] private Transform[] spawnPoints;
    public static Spawner Script;

    private void Awake()
    {
        Script = this;
    }

    public void SpawnObject(GameObject prefab = null, string prefabName = null)
    {
        GameObject obj;
        if (prefab != null)
        {
            obj=ObjectHub.hub.Request(prefab);
        }
        else if (prefabName != null)
        {
            obj = ObjectHub.hub.Request(prefabName);
        }
        else
        {
            Debug.LogError(ErorsList.EMPTY_REQUEST+ this);
            return;
        }

        if (spawnPoints.Length == 0)
        {
            Debug.Log(ErorsList.OBJECT_FIND_EROR + spawnPoints.Length);
            return;
        }
        Transform point = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)].transform;
        obj.transform.SetPositionAndRotation(point.position, point.rotation);
    }
}
