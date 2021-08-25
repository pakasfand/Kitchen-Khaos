using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Sink : MonoBehaviour
{
    [SerializeField] private GameObject _progressBarGO;
    [SerializeField] private Image _progressBar;
    [SerializeField] private float _cleaningTime;

    private bool _active;
    private float _activeTimer;

    public static Action OnDishesCleaned;

    private void OnEnable()
    {
        PlayerInteraction.OnPlayerStartedCleaning += OnPlayerStartedCleaning;
        PlayerInteraction.OnPlayerStoppedCleaning += OnPlayerStoppedCleaning;
    }

    private void OnDisable()
    {
        PlayerInteraction.OnPlayerStartedCleaning -= OnPlayerStartedCleaning;
        PlayerInteraction.OnPlayerStoppedCleaning -= OnPlayerStoppedCleaning;
    }

    private void OnPlayerStartedCleaning()
    {
        _progressBarGO.SetActive(true);
        _active = true;
    }

    private void Update()
    {
        if(_active)
        {
            _activeTimer += Time.deltaTime;

            float width = (_activeTimer/_cleaningTime) * (5.0f);

            _progressBar.rectTransform.sizeDelta =
                new Vector2(width, _progressBar.rectTransform.rect.height);
        }

        if(_activeTimer >= _cleaningTime)
        {
            _active = false;

            _progressBarGO.SetActive(false);

            OnDishesCleaned?.Invoke();
        }
    }

    private void OnPlayerStoppedCleaning()
    {
        _progressBarGO.SetActive(false);
        _active = false;
        _activeTimer = 0f;
    }
}
