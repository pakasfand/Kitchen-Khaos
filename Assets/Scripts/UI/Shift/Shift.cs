using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Shift : MonoBehaviour
{
    class Goal
    {
        private readonly DishTypes _characterType;
        private readonly GameObject _goalGO;
        private int _amount;

        public DishTypes CharacterType => _characterType;
        public GameObject GoalGO => _goalGO;

        public Goal(GameObject goalGO, DishTypes charactertype, int totalAmount)
        {
            _characterType = charactertype;
            _goalGO = goalGO;
            _amount = totalAmount;
        }

        public void DecrementAmount()
        {
            if(_amount > 0)
            {
                _amount -= 1;
                _goalGO.GetComponentInChildren<TextMeshProUGUI>().text = "x" + _amount;
            }

            if(_amount <= 0)
            {
                Destroy(_goalGO);
            }
        }
    }

    [System.Serializable]
    struct GoalObjective
    {
        public DishTypes characterType;
        public int totalAmount;
    }

    [Serializable]
    public class SerializableEvent : UnityEvent { }

    [Header("Shift Manager")]
    [SerializeField] float shiftTime;
    [SerializeField] TextMeshProUGUI timer;
    [SerializeField] RectTransform goalContainer;
    [SerializeField] GoalObjective[] goalObjectives;
    [SerializeField] RectTransform goalPrefab;

    [Header("UI")]
    [SerializeField] float goalSeparation;
    [SerializeField] float waitTimeBetweenGoals;

    public SerializableEvent OnShiftEnded;

    private float currentNumberOfGoals;
    private float goingTime = 0;
    private PlayerInteraction _playerInteraction;
    private List<Goal> _goals;

    private void OnEnable()
    {
        Sink.OnDishesCleaned += UpdateGoals;
    }

    private void OnDisable()
    {
        Sink.OnDishesCleaned -= UpdateGoals;
    }

    private void Start()
    {
        _goals = new List<Goal>();
        _playerInteraction = FindObjectOfType<PlayerInteraction>();
        ActivateGoals();
    }


    private void Update()
    {
        goingTime += Time.deltaTime;
        timer.text = Mathf.Floor(goingTime / 60) + ":" + Mathf.Floor((goingTime / 10) % 6) + Mathf.Floor(goingTime % 10);

        if (goingTime >= shiftTime)
        {
            OnShiftEnded.Invoke();
        }
    }

    public void GenerateGoals()
    {
        for (int i = 0; i < goalObjectives.Length; i++)
        {
            RectTransform goal = CreateGoal(goalObjectives[i].characterType,
                goalObjectives[i].totalAmount);

            goal.gameObject.SetActive(true);

            _goals.Add(new Goal(goal.gameObject,
                goalObjectives[i].characterType,
                goalObjectives[i].totalAmount));
        }
    }

    private void ActivateGoals()
    {
        GenerateGoals();

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

    private RectTransform CreateGoal(DishTypes characterType, int totalAmount)
    {
        RectTransform goal = Instantiate(goalPrefab, goalContainer);
        Sprite sprite = Resources.Load<Sprite>(Enum.GetName(typeof(DishTypes), characterType));
        goal.GetComponentInChildren<Image>().sprite = sprite;
        goal.GetComponentInChildren<TextMeshProUGUI>().text = "x" + totalAmount;
        goal.gameObject.SetActive(false);

        return goal;
    }

    private void TryDecrementGoalAmount(DishTypes dishType)
    {
        foreach (var goal in _goals)
        {
            if(dishType == goal.CharacterType)
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
}
