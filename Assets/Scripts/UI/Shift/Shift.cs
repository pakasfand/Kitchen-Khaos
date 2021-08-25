using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Shift : MonoBehaviour
{

    public enum CharacterType
    {
        Plate,
        Cup,
    }

    [System.Serializable]
    struct Goal
    {
        public CharacterType characterType;
        public int objective;
    }

    [Serializable]
    public class SerializableEvent : UnityEvent { }

    [Header("Shift Manager")]
    [SerializeField] float shiftTime;
    [SerializeField] TextMeshProUGUI timer;
    [SerializeField] RectTransform goalContainer;
    [SerializeField] Goal[] goals;
    [SerializeField] RectTransform goalPrefab;
    public SerializableEvent OnShiftEnded;
    [Header("UI")]
    [SerializeField] float goalSeparation;
    [SerializeField] float waitTimeBetweenGoals;

    float currentNumberOfGoals;
    float goingTime = 0;

    private void Start()
    {
        StartCoroutine(ActivateGoals());
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

    public void RegenerateGoals()
    {
        if (goalContainer.childCount != 0)
        {
            DestroyGoals();
        }

        for (int i = 0; i < goals.Length; i++)
        {
            RectTransform goal = CreateGoal(goals[i].characterType, goals[i].objective);
            goal.gameObject.SetActive(true);
        }
    }

    private IEnumerator ActivateGoals()
    {
        if (goalContainer.childCount == 0) RegenerateGoals();

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

    private void DestroyGoals()
    {
        for (int i = 0; i < goalContainer.childCount; i++)
        {
            Destroy(goalContainer.GetChild(i).gameObject);
        }
    }

    private RectTransform CreateGoal(CharacterType characterType, int objective)
    {
        RectTransform goal = Instantiate(goalPrefab, goalContainer);
        Sprite sprite = Resources.Load<Sprite>(Enum.GetName(typeof(CharacterType), characterType));
        goal.GetComponentInChildren<Image>().sprite = sprite;
        goal.GetComponentInChildren<TextMeshProUGUI>().text = "x" + objective;

        goal.localPosition =
            new Vector3(goal.localPosition.x, -goal.sizeDelta.y * (goalContainer.childCount - 1) - goalSeparation * goalContainer.childCount, goal.localPosition.z);
        goal.gameObject.SetActive(false);
        return goal;
    }
}
