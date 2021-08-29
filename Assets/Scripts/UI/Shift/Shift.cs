using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Shift : MonoBehaviour
{
    [Serializable]
    public class Goal
    {

        public DishType dishType;
        [HideInInspector] public GameObject goalGO;
        public int totalAmount;
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
                Destroy(goalGO);
            }
        }
    }

    [Serializable]
    public class SerializableEvent : UnityEvent { }

    [Header("Shift Manager")]
    public List<Goal> goals;
    public float shiftTime;
    [SerializeField] float breakTime;
    [SerializeField] Image timer;
    [SerializeField] Animator timerOutline;
    [SerializeField] RectTransform goalContainer;
    [SerializeField] RectTransform goalPrefab;


    [Header("UI")]
    [SerializeField] float goalSeparation;
    [SerializeField] float waitTimeBetweenGoals;
    [SerializeField] float runningOutOfTimeFraction;
    [SerializeField] Color flickingColor;
    [SerializeField] float flickingTime;
    [SerializeField] TextMeshProUGUI breakUI;


    public SerializableEvent OnShiftEnded;
    [HideInInspector] public bool hasEnded;

    private float currentNumberOfGoals;
    private float goingTime = 0;
    private PlayerInteraction _playerInteraction;
    private bool flicking = false;
    private Coroutine flickerCoroutine;
    private bool started;

    private void OnEnable()
    {
        Sink.OnDishesCleaned += UpdateGoals;
    }

    private void OnDisable()
    {
        Sink.OnDishesCleaned -= UpdateGoals;
    }

    private void Awake()
    {
        DestroyGoals();

        GenerateGoals(false);

        _playerInteraction = FindObjectOfType<PlayerInteraction>();

    }

    private void Start()
    {
        foreach (Goal goal in goals)
        {
            goal.currentAmount = goal.totalAmount;
        }
    }

    private void Update()
    {
        if (!started) return;
        goingTime += Time.deltaTime;
        timer.fillAmount = 1 - goingTime / shiftTime;

        if (timer.fillAmount <= runningOutOfTimeFraction && !flicking)
        {
            flickerCoroutine = StartCoroutine(Flicker(timer));
            timerOutline.SetTrigger("Change Clock");
            flicking = true;

        }

        if (goingTime >= shiftTime)
        {
            StopCoroutine(flickerCoroutine);
            OnShiftEnded.Invoke();
            hasEnded = true;
        }
    }

    public void StartShift()
    {
        transform.GetChild(0).gameObject.SetActive(true);
        ActivateGoals();
        started = true;
    }

    public IEnumerator Break()
    {

        hasEnded = true;
        breakUI.gameObject.SetActive(true);
        float timer = breakTime;
        yield return new WaitUntil(() =>
        {
            timer -= Time.deltaTime;
            breakUI.text = String.Format("Nice job! Next shift in: " + timer.ToString("F2"));
            return timer <= 0;
        });
    }

    public bool CheckGoalsCompletition()
    {
        foreach (Goal goal in goals)
        {
            if (goal.currentAmount != 0) return false;
        }
        return true;
    }

    public void PregenerateGoals()
    {
        GenerateGoals(true);
    }

    private void GenerateGoals(bool active)
    {
        for (int i = 0; i < goals.Count; i++)
        {
            RectTransform goal = InstantiateGoal(goals[i].dishType,
                goals[i].totalAmount);

            goal.gameObject.SetActive(active);

            goals[i].goalGO = goal.gameObject;
        }
    }

    public void AddGoal(DishType dishType, int totalAmount)
    {
        GameObject goalGO = InstantiateGoal(dishType, totalAmount).gameObject;
        goals.Add(new Goal(goalGO, dishType, totalAmount));
    }

    private void ActivateGoals()
    {
        StartCoroutine(PlayActivationAnimation());
    }

    private IEnumerator PlayActivationAnimation()
    {
        for (int i = 0; i < goalContainer.childCount; i++)
        {
            goalContainer.GetChild(i).gameObject.SetActive(true);
            goalContainer.GetChild(i).GetComponent<Animation>().Play();
            yield return new WaitForSeconds(waitTimeBetweenGoals);
        }
    }

    private RectTransform InstantiateGoal(DishType dishType, int totalAmount)
    {
        RectTransform goal = Instantiate(goalPrefab, goalContainer);
        Image image = goal.GetComponentInChildren<Image>();
        image.sprite = dishType.UISprite;
        image.SetNativeSize();
        goal.GetComponentInChildren<TextMeshProUGUI>().text = "x" + totalAmount;
        goal.gameObject.SetActive(false);
        return goal;
    }

    private void DestroyGoals()
    {
        for (int i = 0; i < goalContainer.childCount; i++)
        {
            Destroy(goalContainer.GetChild(i).gameObject);
        }
    }

    public void PreDestroyGoals()
    {
        for (int i = 0; i < goalContainer.childCount; i++)
        {
            DestroyImmediate(goalContainer.GetChild(i).gameObject);
        }
    }


    private void TryDecrementGoalAmount(DishType dishType)
    {

        foreach (var goal in goals)
        {
            if (dishType == goal.dishType)
            {
                goal.DecrementAmount();
            }
        }
    }

    private void UpdateGoals()
    {
        if (!started) return;
        foreach (var dish in _playerInteraction.DishesCollected)
        {
            TryDecrementGoalAmount(dish);
        }

        _playerInteraction.DishesCollected.Clear();
    }


    private IEnumerator Flicker(Image image)
    {
        Color originalColor = image.color;
        image.color = flickingColor;
        yield return new WaitForSeconds(flickingTime);
        image.color = originalColor;
        yield return new WaitForSeconds(flickingTime);
        StartCoroutine(Flicker(image));
    }


}
