using System;
using UnityEngine;

[Tooltip("Этот скрипт хранит в себе характеристики персонажа.")]
public class СharacterStats : MonoBehaviour
{
    public static event Action<GameObject, СharacterStats> OnCreatingTheCharacterStatsScript;

    private void Start()
    {
        OnCreatingTheCharacterStatsScript?.Invoke(gameObject, this);
    }

    [SerializeField] public byte strength;
    [SerializeField] public byte agility;
    [SerializeField] public byte endurance;
    [SerializeField] public byte stamina;
    [SerializeField] public byte values;
    [SerializeField] public byte health;
}
