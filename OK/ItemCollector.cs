using UnityEngine;
using System;

[Tooltip("���� ������ ������ ���� �� ������ ��� ����.")]

[RequireComponent(typeof(Collider))]
public class ItemCollector : MonoBehaviour
{
    public static event Action<GameObject, PickupItem> OnItemHasBeenPickedUp;
    //[SerializeField] Spawner spawner;
    private void Reset()
    {
        // ��������� ������/AI ����� ���������
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        PickupItem item = other.GetComponent<PickupItem>();
        if (item != null)
        {
            // Debug.Log($"{gameObject.name} �������� {item.itemName} x{item.value}");

            OnItemHasBeenPickedUp?.Invoke(gameObject, item);
            //Spawner.On�reateAnObjectFromASpawner?.Invoke();
            // ��� ����� �������� � ��������� ��� �������� �����
            // Example: inventory.Add(item);

            //Destroy(other.gameObject);
        }
    }
}
