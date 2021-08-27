
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class DishesSpawner : MonoBehaviour
{
    [SerializeField] int maxNumberOfDishes;
    [SerializeField] DishSpawnInfo[] dishesToSpawn;
    [SerializeField] float spawnRate = 1f;
    [SerializeField] float startToSpawnTime = 0f;
    [SerializeField] int numberOfPoolInstances;
    [SerializeField] Transform spawnPoint;

    public static int _maxNumberOfDishes;
    public static int currentNumberOfDishes;


    Shift shift;
    Dictionary<DishSpawnInfo, ObjectPool> poolsTable;
    Queue<GameObject> neededDishes;
    float spawnTimer = 0;
    private float checkAvailabilityTimer;

    [Serializable]
    private struct DishSpawnInfo
    {

        public DishSpawnInfo(DishType dishType, float probability)
        {
            this.dishType = dishType;
            this.probability = probability;

        }

        public DishType dishType;
        [Range(0, 1f)] public float probability;
    }

    private void Awake()
    {

        neededDishes = new Queue<GameObject>();
        poolsTable = new Dictionary<DishSpawnInfo, ObjectPool>();
        shift = FindObjectOfType<Shift>();



        float total = 0;

        for (int i = 0; i < dishesToSpawn.Length; i++)
        {
            ObjectPool pool = (ObjectPool)gameObject.AddComponent(typeof(ObjectPool));
            pool.subject = dishesToSpawn[i].dishType.prefab;
            pool.numberOfInstances = numberOfPoolInstances;

            total += dishesToSpawn[i].probability;

            poolsTable.Add(new DishSpawnInfo(dishesToSpawn[i].dishType, dishesToSpawn[i].probability), pool);
        }

        if (total != 1)
        {
            throw new ProbabilityException("Total Probabilty is not equal to 1");
        }
    }

    private class ProbabilityException : System.Exception
    {
        public ProbabilityException() { }
        public ProbabilityException(string message) : base(message) { }
        public ProbabilityException(string message, System.Exception inner) : base(message, inner) { }
        protected ProbabilityException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }


    private void Start()
    {
        _maxNumberOfDishes = maxNumberOfDishes;

    }

    private void Update()
    {
        spawnTimer += Time.deltaTime;
        checkAvailabilityTimer += Time.deltaTime;

        if (currentNumberOfDishes < _maxNumberOfDishes && spawnTimer >= 1 / spawnRate + startToSpawnTime)
        {
            SpawnEnemy(GetRandomEnemy());
            spawnTimer = startToSpawnTime;
        }

    }

    // private bool CheckAvailabilityForGoals()
    // {
    //     for (int i = 0; i < shift.goalObjectives.Length; i++)
    //     {
    //         int goalAmount = shift.goalObjectives[i].totalAmount;
    //         int currentAmount = GetDishTypeCurrentAmount(shift.goalObjectives[i].characterType);
    //         int neededAmount = goalAmount - currentAmount;
    //         for (int j = 0; j < neededAmount; j++)
    //         {

    //         }
    //     }
    // }

    private int GetDishTypeCurrentAmount(DishSpawnInfo dish)
    {
        ObjectPool pool = poolsTable[dish];
        return pool.GetNumberOfObjectsInPool(ObjectPool.ObjectState.Active);
    }

    private void SpawnEnemy(DishSpawnInfo selectedDish)
    {
        GameObject enemy = poolsTable[selectedDish].RequestSubject();
        enemy.SetActive(true);
        enemy.transform.position = transform.position;
        currentNumberOfDishes++;
    }

    private DishSpawnInfo GetRandomEnemy()
    {
        float randomNumber = Random.Range(0, 1f);

        float probabilitySum = 0;

        for (int i = 0; i < dishesToSpawn.Length; i++)
        {
            probabilitySum += dishesToSpawn[i].probability;
            if (randomNumber <= probabilitySum)
            {
                return dishesToSpawn[i];
            }
        }

        return new DishSpawnInfo(); //Not reachable
    }
}
