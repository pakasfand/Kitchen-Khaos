
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class DrunkEffect : MonoBehaviour
{
    [SerializeField] PostProcessVolume[] states;
    [SerializeField] float startTime;
    [SerializeField] float blendTimeBetweenStates;
    [SerializeField] float duration;
    [SerializeField] AnimationCurve statesBlendCurve;

    public bool isPlaying;

    public void Play()
    {
        StartCoroutine(PlayEffect());
    }

    private IEnumerator PlayEffect()
    {
        float timer = 0;
        PostProcessVolume newState = ExtensionMethods.RandomExtensions.PickRandomFrom(states);
        PostProcessVolume oldState = newState;


        yield return new WaitWhile(() =>
        {
            timer += Time.deltaTime;
            newState.weight = timer / startTime;
            return timer <= startTime;
        });

        timer = 0;
        newState = PickDifferentState(oldState);
        float t = 0;

        yield return new WaitWhile(() =>
        {
            timer += Time.deltaTime;

            if ((timer % blendTimeBetweenStates) == 0 && timer >= blendTimeBetweenStates)
            {
                oldState = newState;
                newState = PickDifferentState(oldState);
            }

            oldState.weight = Mathf.Lerp(1, 0, t);
            newState.weight = Mathf.Lerp(0, 1, t);

            return timer <= duration;
        });
    }

    private PostProcessVolume PickDifferentState(PostProcessVolume oldState)
    {
        PostProcessVolume newState = ExtensionMethods.RandomExtensions.PickRandomFrom(states);
        if (newState == oldState) return PickDifferentState(oldState);
        return newState;
    }

}
