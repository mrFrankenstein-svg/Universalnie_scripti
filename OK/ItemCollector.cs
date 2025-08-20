using UnityEngine;
using System;

[Tooltip("Этот скрипт должен быть на Игроке или Боте.")]

[RequireComponent(typeof(Collider))]
public class ItemCollector : MonoBehaviour
{
    public static event Action<GameObject, PickupItem> OnItemHasBeenPickedUp;
    //[SerializeField] Spawner spawner;
    private void Reset()
    {
        // Коллайдер игрока/AI будет триггером
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        PickupItem item = other.GetComponent<PickupItem>();
        if (item != null)
        {
            // Debug.Log($"{gameObject.name} подобрал {item.itemName} x{item.value}");

            OnItemHasBeenPickedUp?.Invoke(gameObject, item);
            //Spawner.OnСreateAnObjectFromASpawner?.Invoke();
            // Тут можно добавить в инвентарь или изменить статы
            // Example: inventory.Add(item);

            //Destroy(other.gameObject);
        }
    }
}
