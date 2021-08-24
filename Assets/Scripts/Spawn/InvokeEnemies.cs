
using System;
using UnityEngine;

public class InvokeEnemies : MonoBehaviour
{
    public static int maxNumberOfEnemies = 30;
    public static int currentNumberOfEnemies;
    [SerializeField] float spawnRate = 1f;
    [SerializeField] float startTime = 0f;

    ObjectPool enemiesPool;

    private void Awake()
    {
        enemiesPool = GetComponent<ObjectPool>();
    }


    private void Start()
    {
        InvokeRepeating(nameof(InvokeEnemy), startTime, 1 / spawnRate);


    }

    private void InvokeEnemy()
    {
        if (currentNumberOfEnemies >= maxNumberOfEnemies) return;
        GameObject enemy = enemiesPool.RequestSubject();
        enemy.SetActive(true);
        enemy.transform.position = transform.position;
        currentNumberOfEnemies++;
    }
}
