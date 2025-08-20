using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public Transform player;
    public float speed = 3f;
    public float chaseRange = 10f;

    void Update()
    {
        if (!player) return;
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= chaseRange)
        {
            transform.LookAt(player);
            transform.position += transform.forward * speed * Time.deltaTime;
        }
    }
}
