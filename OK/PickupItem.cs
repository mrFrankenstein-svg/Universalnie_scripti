using System;
using UnityEngine;

[Tooltip("Этот скрипт должен быть на поднимаемом предмете.")]

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class PickupItem : MonoBehaviour
{
    public static Action<GameObject> OnItemWasTaken;
    public string itemName { get; } // название предмета
    public byte value { get; } = 1; // количество (например, патроны, золото)

    private void Reset()
    {
        // Делаем коллайдер триггером
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;

        // Настраиваем Rigidbody
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true; // чтобы предмет не падал
        rb.useGravity = false; // отключаем гравитацию
    }
    private void OnDisable()
    {
        OnItemWasTaken?.Invoke(gameObject);
    }
}
