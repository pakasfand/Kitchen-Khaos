/*
LINK TO FILE IN GITHUB: https://github.com/pakasfand/Kitchen-Khaos/blob/master/Assets/Scripts/Spawn/DishesSpawner.cs

Created for Kitchen Khaos
Play here: https://asbutt.itch.io/kitchen-khaos
Github project: https://github.com/pakasfand/Kitchen-Khaos

This class implements a spawning system for dishes. It was designed according to the following directions:

- Keep the total number of active dishes on the level under a limit. The limit is given by the shift.
- Spawn dishes randomly with a weighted probability for each dish type. 
- Spawn dishes required to achieve level goals every now and then. When spawning a required dish, prioritize
dishes with less probability of spawning normally. This way the player has better chances to complete the level.
- Check for availability of required dishes from time to time and update list of needed dishes accordingly.
- If the maximum number of active dishes is reached, spawn only from required dishes even if that 
trespasses the limit.
- If a dish type is not required to achieve shift goals, it wont' be spawned druing the shift.
- Spawn randomly from a set of locations. The locations are given by the user.
- Use Object Pooling to improve performance.

NOTES:
* The probabilities to spawn each dish type are given by the current shift.
* The number of checks for availability is limited and given by the game designer. The less checks, the more
difficult the game is because dish availability for goals won't be guaranteed at all times.

ASSUMPTIONS:
* The shift must be assigned before enabling the spawner.

Created by Juan David Diaz Garcia. 
LinkedIn: https://www.linkedin.com/in/juan-david-diaz-garcia-8b72781b0/   
Github profile: https://github.com/D4vidDG 
Email: jdiazga@unal.edu.co 
*/
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;
using ExtensionMethods;

public class DishSpawner : MonoBehaviour
{
    //parameters
    [SerializeField] DishType[] dishTypesToSpawn;
    [SerializeField] float timeToSpawn = 1f;
    [SerializeField] float startDelay = 0f;
    [SerializeField] int maxTimesToCheckForAvailability;
    [SerializeField] Transform[] spawnPoints;
    [SerializeField][Range(0, 1f)] float spawnFromNeededProbability;
    [SerializeField] int numberOfPoolInstances;

    //state variables
    int maxNumberOfActiveDishes = 100;
    int currentNumberOfDishes = 0;
    Shift currentShift;
    //timers
    float spawnTimer = 0;
    float checkAvailabilityTimer = 0;
    //data structures
    Dictionary<DishType, DishTypeSpawnInfo> dishTypeToSpawnInfo;
    List<DishType> neededDishes;

    //Sets the current shift in which the spawner will work.
    public void SetShift(Shift shift)
    {
        foreach (DishType dishType in dishTypeToSpawnInfo.Keys)
        {
            //check if this dish is required for goals
            bool dishTypeNeededInShift = false;
            foreach (Goal goal in shift.goals)
            {
                if (dishType == goal.dishType)
                {
                    dishTypeNeededInShift = true;
                    dishTypeToSpawnInfo[dishType].spawnProbability = goal.spawnProbability;
                }
            }

            //if dish type is not required in the shift, do not spawn it
            if (!dishTypeNeededInShift) dishTypeToSpawnInfo[dishType].spawnProbability = 0;
        }

        ResetSpawner();
        this.maxNumberOfActiveDishes = shift.maxNumberOfActiveDishes;
        this.currentShift = shift;
    }

    //Deactivates all active dishes on the level.
    public void DeactivateAllDishes()
    {
        foreach (DishTypeSpawnInfo dishTypeSpawnInfo in dishTypeToSpawnInfo.Values)
        {
            dishTypeSpawnInfo.sourcePool.DeactivateAll();
        }
    }

    private void Awake()
    {
        neededDishes = new List<DishType>();
        dishTypeToSpawnInfo = new Dictionary<DishType, DishTypeSpawnInfo>();

        //create object pools for each dish type
        for (int i = 0; i < dishTypesToSpawn.Length; i++)
        {
            DishType dishType = dishTypesToSpawn[i];
            GameObject dishObjectPool = new GameObject(dishType + " Pool container");
            dishObjectPool.transform.parent = this.transform;
            ObjectPool pool = (ObjectPool)dishObjectPool.AddComponent(typeof(ObjectPool));
            pool.subject = dishType.prefab;
            pool.numberOfInstances = numberOfPoolInstances;
            dishTypeToSpawnInfo.Add(dishType, new DishTypeSpawnInfo(pool));
        }

    }

    private void OnEnable()
    {
        PlayerInteraction.OnDishPickedUp += OnDishPickedUp;
    }

    private void OnDisable()
    {
        PlayerInteraction.OnDishPickedUp -= OnDishPickedUp;
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            UpdateListOfNeededDishes();
        }

        spawnTimer += Time.deltaTime;
        checkAvailabilityTimer += Time.deltaTime;

        //check for availability for goals and update list of needed dishes
        if (checkAvailabilityTimer > currentShift.shiftTime / maxTimesToCheckForAvailability)
        {
            UpdateListOfNeededDishes();
            checkAvailabilityTimer = 0;
        }

        //if its time to spawn a dish
        if (spawnTimer >= timeToSpawn + startDelay)
        {
            //if limit of dishes is not reached
            if (currentNumberOfDishes < maxNumberOfActiveDishes)
            {
                //choose randomly between spawning a required dish or a random dish
                bool spawnFromNeeded = spawnFromNeededProbability >= Random.Range(0, 1f);
                if (spawnFromNeeded && neededDishes.Count != 0)
                {
                    SpawnFromNeeded();
                }
                else
                {
                    SpawnDish(GetRandomDishType());
                }
            }
            //if limit reached 
            else if (maxNumberOfActiveDishes <= currentNumberOfDishes && neededDishes.Count != 0)
            {
                //check for goal availability
                UpdateListOfNeededDishes();
                if (neededDishes.Count != 0)
                {
                    SpawnFromNeeded();
                }
            }

            //reset timer
            spawnTimer = startDelay;
        }
    }


    //Spawns a given dish type on a random location
    private GameObject SpawnDish(DishType dishType)
    {
        //retrieve new dish from its object pool
        GameObject dish = dishTypeToSpawnInfo[dishType].sourcePool.RequestSubject();
        //activate dish on scene
        dish.SetActive(true);
        //position dish on random location
        dish.GetComponent<NavMeshAgent>().enabled = false;
        dish.transform.position = PickRandomLocation();
        dish.GetComponent<NavMeshAgent>().enabled = true;
        currentNumberOfDishes++;
        return dish;
    }

    //Spawns a dish from required queue
    private void SpawnFromNeeded()
    {
        SpawnDish(neededDishes[0]);
        neededDishes.Remove(neededDishes[0]);
    }

    //Picks a randon spawn point. Assumes there's at least one active spawn point on scene.
    private Vector3 PickRandomLocation()
    {
        Transform chosen = RandomExtensions.PickRandomFrom(spawnPoints);
        //if location is active, return it
        if (chosen.gameObject.activeSelf == true)
        {
            return chosen.position;
        }
        //else, try with another location
        else
        {
            return PickRandomLocation();
        }
    }

    //Updates list of needed dishes according to the shift goals.
    private void UpdateListOfNeededDishes()
    {
        //for each goal
        for (int i = 0; i < currentShift.goals.Count; i++)
        {
            //calculate the needed amount of dishes to reach the goal
            Goal goal = currentShift.goals[i];
            int goalAmount = goal.currentAmount;
            DishType goalDishType = goal.dishType;

            int currentAmountInScene = GetDishTypeCurrentAmountInScene(goalDishType);
            int currentAmountInQueue = GetDishTypeCurrentAmountInQueue(goalDishType);
            int neededAmount = goalAmount - currentAmountInScene - currentAmountInQueue;
            Debug.Log("Checking for" + goalDishType.name + ". Scene:" + currentAmountInScene + ". Queue:" + currentAmountInQueue);

            //if dish type is needed
            if (0 < neededAmount)
            {
                //add to list of needed dishes
                for (int j = 0; j < neededAmount; j++)
                {
                    neededDishes.Add(goalDishType);
                }
            }
            //else if there's more than enough dishes of that type
            else if (neededAmount < 0)
            {
                //remove excess from list of needed dishes 
                int excessAmount = Mathf.Min(Mathf.Abs(neededAmount), currentAmountInQueue);
                RemoveDishFromNeeded(goalDishType, excessAmount);
            }

            Debug.Log("After check. Queue:" + GetDishTypeCurrentAmountInQueue(goalDishType));
        }


        //sort list of needed dishes from most rare to less rare
        neededDishes.Sort((DishType x, DishType y) =>
            {
                float difference = dishTypeToSpawnInfo[x].spawnProbability - dishTypeToSpawnInfo[y].spawnProbability;
                if (difference == 0) return 0;

                return (int)Mathf.Sign(difference);
            });
    }

    //Removes a number of dishes of certain type from the list of needed dishes
    private void RemoveDishFromNeeded(DishType requestedType, int amount)
    {
        if (amount == 0) return;

        for (int i = 0; i < amount; i++)
        {
            neededDishes.Remove(requestedType);
        }
    }

    private int GetDishTypeCurrentAmountInQueue(DishType requestedType)
    {
        return neededDishes.FindAll((DishType dishType) => { return dishType == requestedType; }).Count;
    }

    private int GetDishTypeCurrentAmountInScene(DishType dishType)
    {
        if (!dishTypeToSpawnInfo.ContainsKey(dishType)) throw new Exception("The dish type" + dishType.name + "requested in goals is not in the spawner");
        ObjectPool dishTypePool = dishTypeToSpawnInfo[dishType].sourcePool;
        return dishTypePool.GetNumberOfObjectsInPool(ObjectPool.ObjectState.Active);
    }

    //Returns a random dish based on weighted probabilities. Assumes all probabilities sum to 1.
    private DishType GetRandomDishType()
    {
        float randomNumber = Random.Range(0, 1f);

        float probabilitySum = 0;

        for (int i = 0; i < currentShift.goals.Count; i++)
        {
            probabilitySum += currentShift.goals[i].spawnProbability;
            if (randomNumber <= probabilitySum)
            {
                return currentShift.goals[i].dishType;
            }
        }

        return null;
    }


    private void OnDishPickedUp()
    {
        //reduce number of dishes in scene when they are picked up
        currentNumberOfDishes--;
    }

    private void ResetSpawner()
    {
        currentNumberOfDishes = 0;
        spawnTimer = 0;
        checkAvailabilityTimer = 0;
        neededDishes.Clear();
    }


    private void OnDrawGizmosSelected()
    {
        if (spawnPoints.Length == 0) return;

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            //Draw a red sphere on the location of the spawn point
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(spawnPoints[i].position, 1f);
            //Draw a green line between the current and the previous spawn point
            Gizmos.color = Color.green;
            if (1 <= i) Gizmos.DrawLine(spawnPoints[i - 1].position, spawnPoints[i].position);
            else Gizmos.DrawLine(spawnPoints[0].position, spawnPoints[spawnPoints.Length - 1].position);
        }
    }

    //Internal data structure that holds the source pool of the dish type and its spawn probability. 
    [Serializable]
    private class DishTypeSpawnInfo
    {
        public ObjectPool sourcePool;
        public float spawnProbability;

        public DishTypeSpawnInfo()
        {
        }

        public DishTypeSpawnInfo(ObjectPool pool)
        {
            this.sourcePool = pool;
        }

        public DishTypeSpawnInfo(ObjectPool pool, float spawnProbability)
        {
            this.sourcePool = pool;
            this.spawnProbability = spawnProbability;
        }
    }

}
