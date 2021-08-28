
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class DishesSpawner : MonoBehaviour
{
    [SerializeField] int maxNumberOfDishes;
    [SerializeField] DishTypeSpawnInfo[] dishTypesToSpawn;
    [SerializeField] float spawnRate = 1f;
    [SerializeField] float startToSpawnTime = 0f;
    [SerializeField] int numberOfPoolInstances;
    [SerializeField] int maxTimesToCheckForAvailability;
    [SerializeField] Transform[] spawnPoints;
    [SerializeField] [Range(0, 1f)] float spawnFromNeededQueueProbability;


    int currentNumberOfDishes = 0;
    float spawnTimer = 0;
    float checkAvailabilityTimer;
    Dictionary<DishType, DishTypeSpawnInfo> dishTypeInfo;
    public List<DishType> neededDishesQueue;


    Shift shift;

    [Serializable]
    private class DishTypeSpawnInfo
    {

        [HideInInspector] public ObjectPool pool;
        public DishType dishType;
        [Range(0, 1f)] public float probability;

        public DishTypeSpawnInfo(DishType dishType, float probability, ObjectPool pool)
        {
            this.dishType = dishType;
            this.probability = probability;
            this.pool = pool;
        }

    }

    private void Awake()
    {

        neededDishesQueue = new List<DishType>();
        dishTypeInfo = new Dictionary<DishType, DishTypeSpawnInfo>();
        shift = FindObjectOfType<Shift>();



        float total = 0;

        for (int i = 0; i < dishTypesToSpawn.Length; i++)
        {
            GameObject poolContainer = new GameObject("Pool container");
            poolContainer.transform.parent = this.transform;
            ObjectPool pool = (ObjectPool)poolContainer.AddComponent(typeof(ObjectPool));
            pool.subject = dishTypesToSpawn[i].dishType.prefab;
            pool.numberOfInstances = numberOfPoolInstances;
            dishTypesToSpawn[i].pool = pool;

            total += dishTypesToSpawn[i].probability;

            dishTypeInfo.Add(dishTypesToSpawn[i].dishType, dishTypesToSpawn[i]);
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
        CheckAvailabilityForGoals();
    }

    private void Update()
    {
        spawnTimer += Time.deltaTime;
        checkAvailabilityTimer += Time.deltaTime;

        if (checkAvailabilityTimer > shift.shiftTime / maxTimesToCheckForAvailability)
        {
            CheckAvailabilityForGoals();
            checkAvailabilityTimer = 0;
        }

        if (spawnTimer >= (1 / spawnRate) + startToSpawnTime)
        {
            if (currentNumberOfDishes <= maxNumberOfDishes)
            {
                bool spawnFromNeeded = spawnFromNeededQueueProbability >= Random.Range(0, 1f);
                if (spawnFromNeeded && neededDishesQueue.Count != 0)
                {
                    SpawnFromNeeded();
                }
                else
                {
                    SpawnDish(GetRandomDishType());
                }
                currentNumberOfDishes++;
            }
            else if (neededDishesQueue.Count != 0)
            {
                SpawnFromNeeded();
            }
            spawnTimer = startToSpawnTime;
        }
    }

    private GameObject SpawnDish(DishType dishType)
    {
        GameObject dish = dishTypeInfo[dishType].pool.RequestSubject();
        dish.SetActive(true);
        dish.transform.position = PickRandomLocation().position;
        return dish;
    }

    private void SpawnFromNeeded()
    {
        SpawnDish(neededDishesQueue[0]);
        neededDishesQueue.Remove(neededDishesQueue[0]);
    }

    private void CheckAvailabilityForGoals()
    {
        for (int i = 0; i < shift.goals.Count; i++)
        {
            int goalAmount = shift.goals[i].currentAmount;
            DishType dishType = shift.goals[i].dishType;
            int currentAmountInScene = GetDishTypeCurrentAmountInScene(dishType);
            int currentAmountInQueue = GetDishTypeCurrentAmountInQueue(shift.goals[i].dishType);
            int neededAmount = goalAmount - currentAmountInScene - currentAmountInQueue;


            if (neededAmount <= 0)
            {
                int amount = Mathf.Min(Mathf.Abs(neededAmount), currentAmountInQueue);
                neededDishesQueue = ClearQueueFromDishType(dishType, amount);
                continue;
            }

            for (int j = 0; j < neededAmount; j++)
            {
                neededDishesQueue.Add(dishType);
            }
        }

        neededDishesQueue.Sort((DishType x, DishType y) =>
        {
            float difference = dishTypeInfo[x].probability - dishTypeInfo[y].probability;
            if (difference == 0) return 0;

            return (int)Mathf.Sign(difference); //This would sort from the most rare to the less rare
        });
    }

    private List<DishType> ClearQueueFromDishType(DishType requestedType, int amount)
    {
        if (amount == 0) return neededDishesQueue;
        List<DishType> newList = new List<DishType>();
        int count = 0;
        foreach (DishType dishType in neededDishesQueue)
        {
            if (dishType == requestedType && count < amount)
            {
                count++;
                continue;
            }

            newList.Add(dishType);
        }

        return newList;

    }

    private int GetDishTypeCurrentAmountInQueue(DishType dishType)
    {
        return neededDishesQueue.FindAll((DishType currentType) => { return currentType == dishType; }).Count;
    }

    private int GetDishTypeCurrentAmountInScene(DishType dishType)
    {
        if (!dishTypeInfo.ContainsKey(dishType)) Debug.LogWarning("The dishes requested in goals are not in the spawner");
        ObjectPool pool = dishTypeInfo[dishType].pool;
        return pool.GetNumberOfObjectsInPool(ObjectPool.ObjectState.Active);
    }



    private DishType GetRandomDishType()
    {
        float randomNumber = Random.Range(0, 1f);

        float probabilitySum = 0;

        for (int i = 0; i < dishTypesToSpawn.Length; i++)
        {
            probabilitySum += dishTypesToSpawn[i].probability;
            if (randomNumber <= probabilitySum)
            {
                return dishTypesToSpawn[i].dishType;
            }
        }

        return null; //Not reachable
    }

    private Transform PickRandomLocation()
    {
        int randomIndex = Random.Range(0, spawnPoints.Length);
        return spawnPoints[randomIndex];
    }
}
