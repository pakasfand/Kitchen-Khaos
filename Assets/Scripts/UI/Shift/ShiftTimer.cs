using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ShiftTimer : MonoBehaviour
{
    [SerializeField] Image timer;
    [SerializeField] Animator timerOutline;
    [SerializeField] Color flickingColor;
    [SerializeField] float flickingRate;
    [SerializeField] float runningOutOfTimeFraction;

    [HideInInspector] public float maxTime;

    float goingTime;
    bool flicking;
    Color originalColor;

    private void Awake()
    {
        originalColor = timer.color;
    }

    private void OnEnable()
    {
        timer.color = originalColor;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        goingTime = 0;
        flicking = false;
    }

    private void Update()
    {
        goingTime += Time.deltaTime;
        timer.fillAmount = Mathf.Max(1 - goingTime / maxTime, 0);

        if (timer.fillAmount <= runningOutOfTimeFraction && !flicking)
        {
            StartCoroutine(Flicker(timer, flickingRate, flickingColor));
            timerOutline.SetTrigger("Change Clock");
            flicking = true;
        }
    }

    public bool RunOutOfTime()
    {
        return goingTime >= maxTime;
    }

    private IEnumerator Flicker(Image image, float rate, Color colro)
    {
        Color originalColor = image.color;
        image.color = flickingColor;
        yield return new WaitForSeconds(1 / flickingRate);
        image.color = originalColor;
        yield return new WaitForSeconds(1 / flickingRate);
        StartCoroutine(Flicker(image, flickingRate, flickingColor));
    }
}
