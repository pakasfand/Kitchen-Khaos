using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class Countdown : MonoBehaviour
{
    [SerializeField] int numberOfSeconds;
    [SerializeField] string countDownEndText;
    [SerializeField] float countDownEndWaitTime;
    [SerializeField] TextMeshProUGUI count;

    public SerializableEvent OnCountDownFinished;

    [Serializable]
    public class SerializableEvent : UnityEvent { }

    private void OnEnable()
    {
        StartCoroutine(CountDown());
    }

    private IEnumerator CountDown()
    {
        for (int i = 0; i < numberOfSeconds; i++)
        {
            count.text = (numberOfSeconds - i).ToString();
            yield return new WaitForSeconds(1f);
        }

        count.text = countDownEndText;
        yield return new WaitForSeconds(countDownEndWaitTime);
        OnCountDownFinished?.Invoke();
        yield return null;
        gameObject.SetActive(false);
    }
}
