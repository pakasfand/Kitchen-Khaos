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
        [HideInInspector]
        public int currentAmount
        {
            get
            {
                if (_currentAmount == -1) return totalAmount;
                return _currentAmount;
            }

            set
            {
                _currentAmount = value;
            }
        }

        private int _currentAmount;

        public Goal(GameObject goalGO, DishType dishType, int totalAmount)
        {
            this.dishType = dishType;
            this.goalGO = goalGO;
            this.totalAmount = totalAmount;
            currentAmount = totalAmount;
        }

        public Goal()
        {
            _currentAmount = -1;
        }

        public void DecrementAmount()
        {
            if (currentAmount == -1) currentAmount = totalAmount;
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

    [HideInInspector]
    public SerializableEvent OnShiftEnded;

    private float currentNumberOfGoals;
    private float goingTime = 0;
    private PlayerInteraction _playerInteraction;
    private bool flicking = false;

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

        GenerateGoals();

        _playerInteraction = FindObjectOfType<PlayerInteraction>();

    }

    private void Start()
    {
        ActivateGoals();
    }

    private void Update()
    {
        goingTime += Time.deltaTime;
        timer.fillAmount = 1 - goingTime / shiftTime;

        if (timer.fillAmount <= runningOutOfTimeFraction && !flicking)
        {
            StartCoroutine(Flicker(timer));
            timerOutline.SetTrigger("Change Clock");
            flicking = true;

        }

        if (goingTime >= shiftTime)
        {
            StopAllCoroutines();
            OnShiftEnded.Invoke();
        }
    }

    public void PregenerateGoals()
    {
        GenerateGoals();
    }

    private void GenerateGoals()
    {
        for (int i = 0; i < goals.Count; i++)
        {
            RectTransform goal = InstantiateGoal(goals[i].dishType,
                goals[i].totalAmount);

            goal.gameObject.SetActive(true);

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
            goalContainer.GetChild(i).gameObject.SetActive(false);
        }

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
