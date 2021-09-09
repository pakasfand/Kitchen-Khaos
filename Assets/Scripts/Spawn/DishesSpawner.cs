
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class DishesSpawner : MonoBehaviour
{
    [SerializeField] int maxNumberOfDishes;
    [SerializeField] DishType[] dishTypes;
    [SerializeField] float spawnRate = 1f;
    [SerializeField] float startToSpawnTime = 0f;
    [SerializeField] int numberOfPoolInstances;
    [SerializeField] int maxTimesToCheckForAvailability;
    [SerializeField] Transform[] spawnPoints;
    [SerializeField] [Range(0, 1f)] float spawnFromNeededQueueProbability;



    int currentNumberOfDishes = 0;
    float spawnTimer = 0;
    float checkAvailabilityTimer;
    Dictionary<DishType, DishTypeSpawnInfo> spawnInfoByDishType;
    List<DishType> neededDishesQueue;


    [HideInInspector]
    public Shift shift
    {

        get
        {
            return _shift;
        }
        set
        {
            ResetSpawner();
            print("Shift set");
            _shift = value;
        }


    }

    Shift _shift;


    [Serializable]
    private class DishTypeSpawnInfo
    {
        public ObjectPool pool;
        public float spawnProbability;

        public DishTypeSpawnInfo()
        {
        }

        public DishTypeSpawnInfo(ObjectPool pool)
        {
            this.pool = pool;
        }
    }

    private void Awake()
    {
        neededDishesQueue = new List<DishType>();
        spawnInfoByDishType = new Dictionary<DishType, DishTypeSpawnInfo>();


        for (int i = 0; i < dishTypes.Length; i++)
        {
            GameObject poolContainer = new GameObject(dishTypes[i] + " Pool container");
            poolContainer.transform.parent = this.transform;
            ObjectPool pool = (ObjectPool)poolContainer.AddComponent(typeof(ObjectPool));
            pool.subject = dishTypes[i].prefab;
            pool.numberOfInstances = numberOfPoolInstances;

            spawnInfoByDishType.Add(dishTypes[i], new DishTypeSpawnInfo(pool));
        }
    }

    private void OnEnable()
    {
        CheckAvailabilityForGoals();
    }

    public void DeactivateAllDishes()
    {
        foreach (DishTypeSpawnInfo dishTypeSpawnInfo in spawnInfoByDishType.Values)
        {
            dishTypeSpawnInfo.pool.DeactivateAll();
        }
    }

    private void Update()
    {
        spawnTimer += Time.deltaTime;
        checkAvailabilityTimer += Time.deltaTime;

        if (checkAvailabilityTimer > _shift.shiftTime / maxTimesToCheckForAvailability)
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

    public void SetShift(Shift shift)
    {
        foreach (DishType dishType in spawnInfoByDishType.Keys)
        {
            bool inShift = false;
            foreach (Goal goal in shift.goals)
            {
                if (dishType == goal.dishType)
                {
                    inShift = true;
                    spawnInfoByDishType[dishType].spawnProbability = goal.spawnProbability;
                }
            }
            if (!inShift) spawnInfoByDishType[dishType].spawnProbability = 0;
        }

        this._shift = shift;
    }

    private GameObject SpawnDish(DishType dishType)
    {
        GameObject dish = spawnInfoByDishType[dishType].pool.RequestSubject();
        dish.SetActive(true);
        dish.transform.position = PickRandomLocation();
        return dish;
    }

    private void SpawnFromNeeded()
    {
        SpawnDish(neededDishesQueue[0]);
        neededDishesQueue.Remove(neededDishesQueue[0]);
    }

    private void CheckAvailabilityForGoals()
    {
        for (int i = 0; i < _shift.goals.Count; i++)
        {
            int goalAmount = _shift.goals[i].currentAmount;
            DishType dishType = _shift.goals[i].dishType;
            int currentAmountInScene = GetDishTypeCurrentAmountInScene(dishType);
            int currentAmountInQueue = GetDishTypeCurrentAmountInQueue(_shift.goals[i].dishType);
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
            float difference = spawnInfoByDishType[x].spawnProbability - spawnInfoByDishType[y].spawnProbability;
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
        if (!spawnInfoByDishType.ContainsKey(dishType)) throw new Exception("The dishes requested in goals are not in the spawner");

        ObjectPool pool = spawnInfoByDishType[dishType].pool;
        return pool.GetNumberOfObjectsInPool(ObjectPool.ObjectState.Active);
    }



    private DishType GetRandomDishType()
    {
        float randomNumber = Random.Range(0, 1f);

        float probabilitySum = 0;

        for (int i = 0; i < _shift.goals.Count; i++)
        {
            probabilitySum += _shift.goals[i].spawnProbability;
            if (randomNumber <= probabilitySum)
            {
                return _shift.goals[i].dishType;
            }
        }

        return null;
    }

    private Vector3 PickRandomLocation()
    {
        int randomIndex = Random.Range(0, spawnPoints.Length);
        return spawnPoints[randomIndex].position;
    }

    private void ResetSpawner()
    {
        currentNumberOfDishes = 0;
        spawnTimer = 0;
        checkAvailabilityTimer = 0;
        neededDishesQueue.Clear();
    }
}
