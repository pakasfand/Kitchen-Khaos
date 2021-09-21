
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameLoop : MonoBehaviour
{
    public static GameLoop instance;

    [Header("Game Management")]
    [SerializeField] GameObject player;
    [SerializeField] DishesSpawner spawner;
    [SerializeField] Shift[] shifts;

    [Header("UI")]
    [SerializeField] ShiftTimer shiftTimer;
    [SerializeField] GameObject goalContainer;
    [SerializeField] GameObject goalPrefab;
    [SerializeField] GameObject countdown;
    [SerializeField] GameObject winPanel;
    [SerializeField] GameObject retryPanel;
    [SerializeField] GameObject instructionsPanel;
    [SerializeField] float waitTimeBetweenGoals;
    [SerializeField] TextMeshProUGUI breakTextUI;


    [Serializable]
    public class SerializableEvent : UnityEvent { }

    public static Action<bool> OnShiftOver;

    int currentShiftIndex = 0;
    Coroutine shiftTransition;
    PlayerInteraction _playerInteraction;
    bool outOfShift = true;

    private void Awake()
    {
        if (instance != null) throw new Exception("More than one Game Manager");
        instance = this;

        _playerInteraction = player.GetComponent<PlayerInteraction>();
        Sink.OnDishesCleaned += UpdateGoals;
    }


    private void Start()
    {
        instructionsPanel.SetActive(true);

        for (int i = 0; i < shifts.Length; i++)
        {
            if (CheckForRepeatedGoals(shifts[i])) throw new Exception("Shift #" + (i + 1) + " has the same dish type for different goals");
            if (!CheckProbabilitySum(shifts[i])) throw new Exception("Shift #" + (i + 1) + " does not have a sum of spawn probability equal to 1");
        }
        EnablePlayer(false);
    }

    private void Update()
    {

        if (outOfShift) return;

        if (shifts[currentShiftIndex].GoalsCompleted())
        {
            outOfShift = true;
            EndShift(true);
            currentShiftIndex++;

            if (currentShiftIndex >= shifts.Length)
            {
                WinGame();
                return;
            }

            shiftTransition = StartCoroutine(StartShiftTransition(shifts[currentShiftIndex - 1]));
            return;
        }

        if (shiftTimer.RunOutOfTime())
        {
            outOfShift = true;
            EndShift(false);
            EnablePlayer(false);
            retryPanel.SetActive(true);
            return;
        }
    }

    public void StartShift()
    {
        EnablePlayer(true);

        shifts[currentShiftIndex].Initialize();
        GenerateGoals(shifts[currentShiftIndex]);

        shiftTimer.maxTime = shifts[currentShiftIndex].shiftTime;
        shiftTimer.gameObject.SetActive(true);

        spawner.shift = shifts[currentShiftIndex];
        spawner.enabled = true;

        outOfShift = false;
    }

    private void EndShift(bool completed)
    {
        shiftTimer.gameObject.SetActive(false);
        DestroyGoals();
        spawner.DeactivateAllDishes();
        spawner.enabled = false;
        OnShiftOver?.Invoke(completed);
    }

    private IEnumerator StartShiftTransition(Shift previousShift)
    {
        player.GetComponent<PlayerInteraction>().enabled = false;

        breakTextUI.gameObject.SetActive(true);
        float timer = previousShift.transitionTime;

        yield return new WaitUntil(() =>
        {
            timer -= Time.deltaTime;
            timer = Mathf.Max(timer, 0f);
            breakTextUI.text = String.Format("Nice job! Next shift in: " + timer.ToString("F2"));
            return timer <= 0;
        });

        breakTextUI.gameObject.SetActive(false);

        if (previousShift.sequence != null)
        {
            previousShift.sequence.Play();
            yield return null;
            yield return new WaitWhile(() => { return previousShift.sequence.state == PlayState.Playing; });
        }

        countdown.SetActive(true);

        yield return new WaitUntil(() =>
            {
                return countdown.activeSelf == false;
            });


        shiftTransition = null;
    }

    private void GenerateGoals(Shift shift)
    {
        foreach (Goal goal in shift.goals)
        {
            GameObject goalGO = Instantiate(goalPrefab, goalContainer.transform);

            Image image = goalGO.GetComponentInChildren<Image>();
            image.sprite = goal.dishType.UISprite;
            image.SetNativeSize();

            TextMeshProUGUI amount = goalGO.GetComponentInChildren<TextMeshProUGUI>();
            amount.text = "x" + goal.totalAmount;

            goalGO.SetActive(false);
            goal.goalGO = goalGO;
        }

        StartCoroutine(PlayActivationAnimation());
    }

    private void UpdateGoals()
    {
        foreach (var dish in _playerInteraction.DishesCollected)
        {
            shifts[currentShiftIndex].TryDecrementGoalAmount(dish);
        }

        _playerInteraction.DishesCollected.Clear();
    }

    private void DestroyGoals()
    {
        for (int i = 0; i < goalContainer.transform.childCount; i++)
        {
            Destroy(goalContainer.transform.GetChild(i).gameObject);
        }
    }

    private IEnumerator PlayActivationAnimation()
    {
        for (int i = 0; i < goalContainer.transform.childCount; i++)
        {
            goalContainer.transform.GetChild(i).gameObject.SetActive(true);
            goalContainer.transform.GetChild(i).GetComponent<Animation>().Play();
            yield return new WaitForSeconds(waitTimeBetweenGoals);
        }
    }

    private void WinGame()
    {
        if (shiftTransition != null) StopCoroutine(shiftTransition);
        EnablePlayer(false);
        winPanel.SetActive(true);
        return;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }



    private bool CheckForRepeatedGoals(Shift shift)
    {
        Dictionary<DishType, bool> dishTypes = new Dictionary<DishType, bool>();
        for (int i = 0; i < shift.goals.Count; i++)
        {
            if (dishTypes.ContainsKey(shift.goals[i].dishType))
            {
                return true; // has duplicates
            }
            dishTypes.Add(shift.goals[i].dishType, true);
        }
        return false; // no duplicates
    }
    private bool CheckProbabilitySum(Shift shift)
    {
        float total = 0;
        for (int i = 0; i < shift.goals.Count; i++)
        {
            total += shift.goals[i].spawnProbability;
        }

        return total == 1;
    }


    private void EnablePlayer(bool enable)
    {
        player.GetComponent<PlayerInteraction>().enabled = enable;
        player.GetComponent<PlayerMovement>().enabled = enable;
    }
}

[Serializable]
public class Shift
{
    public List<Goal> goals;
    public float shiftTime;
    public float transitionTime;
    public PlayableDirector sequence;

    private float currentNumberOfGoals;
    private PlayerInteraction _playerInteraction;
    private Coroutine flickerCoroutine;


    public bool GoalsCompleted()
    {
        foreach (Goal goal in goals)
        {
            if (goal.currentAmount != 0) return false;
        }

        return true;
    }

    public void Initialize()
    {
        foreach (Goal goal in goals)
        {
            goal.currentAmount = goal.totalAmount;
        }
    }

    public void TryDecrementGoalAmount(DishType dishType)
    {

        foreach (var goal in goals)
        {
            if (dishType == goal.dishType)
            {
                goal.DecrementAmount();
            }
        }
    }


}

[Serializable]
public class Goal
{

    public DishType dishType;
    public int totalAmount;
    [Range(0, 1f)] public float spawnProbability;

    [HideInInspector] public GameObject goalGO;
    [HideInInspector] public int currentAmount;



    public Goal(GameObject goalGO, DishType dishType, int totalAmount)
    {
        this.dishType = dishType;
        this.goalGO = goalGO;
        this.totalAmount = totalAmount;
        currentAmount = totalAmount;
    }

    public void DecrementAmount()
    {
        if (currentAmount > 0)
        {
            currentAmount -= 1;
            goalGO.GetComponentInChildren<TextMeshProUGUI>().text = "x" + currentAmount;
        }

        if (currentAmount <= 0)
        {
            UnityEngine.Object.Destroy(goalGO);
        }
    }
}