using UnityEngine;
using System;

public class HealthSystem : MonoBehaviour
{
    [SerializeField] byte maxHealth = 100;
    [SerializeField] byte currentHealth;
    public static Action<GameObject>  onDeath;

    void Start() => currentHealth = maxHealth;

    public void TakeDamage(byte amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            onDeath.Invoke(gameObject);
        }
    }

    public void Heal(byte amount)
    {
        int health = Mathf.Min(currentHealth + amount, maxHealth);
        if (health > byte.MaxValue)
            health = byte.MaxValue;
        else
            currentHealth = (byte)health;
    }
}
