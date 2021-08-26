using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Cup : MonoBehaviour
{
    [SerializeField] GameObject waterPuddlePrefab;
    [SerializeField] float chanceToBreak;
    [SerializeField] float timeToBreak;


    private void Start()
    {
        InvokeRepeating(nameof(TryToBreak), timeToBreak, timeToBreak);
    }

    private void TryToBreak()
    {
        float randomValue = Random.Range(0, 1);
        if (chanceToBreak > randomValue)
        {
            Instantiate(waterPuddlePrefab, transform.position, Quaternion.identity, transform);
        }
        gameObject.SetActive(false);

    }
}
