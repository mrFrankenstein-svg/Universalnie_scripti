using System;
using UnityEngine;

[Tooltip("���� ������ ������ ���� �� ����������� ��������.")]

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class PickupItem : MonoBehaviour
{
    public static Action<GameObject> OnItemWasTaken;
    public string itemName { get; } // �������� ��������
    public byte value { get; } = 1; // ���������� (��������, �������, ������)

    private void Reset()
    {
        // ������ ��������� ���������
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;

        // ����������� Rigidbody
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true; // ����� ������� �� �����
        rb.useGravity = false; // ��������� ����������
    }
    private void OnDisable()
    {
        OnItemWasTaken?.Invoke(gameObject);
    }
}
