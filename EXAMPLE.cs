using UnityEditor;
using UnityEngine;

public class EXAMPLE : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    private void OnEnable()
    {
        PickupItem.OnItemWasTaken += IventActioner;
    }
    private void OnDisable()
    {
        PickupItem.OnItemWasTaken -= IventActioner;
    }
    private void IventActioner(GameObject obj)
    {
        if (obj.name == prefab.name)
            Spawner.Script.SpawnObject(prefab);
    }
}
