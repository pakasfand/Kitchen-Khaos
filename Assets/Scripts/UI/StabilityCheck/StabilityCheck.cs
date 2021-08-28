using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class StabilityCheck : MonoBehaviour
{
    [SerializeField] private GameObject _stabilityBarGO;
    [SerializeField] private GameObject _cooldownTimerGO;
    [SerializeField] private Image _stabilityBar;
    [SerializeField] private Image _countdownBar;
    [SerializeField] private Gradient _stabilitygradient;
    [SerializeField] private Gradient _cooldowngradient;
    [SerializeField] private float _durationToCooldownFull;
    [SerializeField] private float _durationToStabilityFull;
    [SerializeField] private Color _passColor;
    [SerializeField] private Color _failColor;
    [SerializeField] private float _durationToFailPerDish;
    [SerializeField] private Animator _animator;

    private bool _active;
    private float _coolDownTimer;
    private float _stabilityTimer;
    private float _durationToFail;

    public static Action<bool> OnStabilityCompleted;

    private void OnEnable()
    {
        PlayerInteraction.OnStabilityCheckBegin += OnStabilityCheckBegin;
        PlayerInteraction.OnPlayerStumble += OnPlayerStumble;
        Sink.OnDishesCleaned += OnDishesCleaned;
    }

    private void OnDisable()
    {
        PlayerInteraction.OnStabilityCheckBegin -= OnStabilityCheckBegin;
        PlayerInteraction.OnPlayerStumble -= OnPlayerStumble;
        Sink.OnDishesCleaned -= OnDishesCleaned;
    }

    private void OnStabilityCheckBegin(int dishesCount)
    {
        _stabilityBarGO.SetActive(true);
        _cooldownTimerGO.SetActive(true);

        _active = true;

        _countdownBar.fillAmount = 1f;
        _stabilityBar.fillAmount = 0f;

        _durationToFail = Mathf.Clamp(1 - (_durationToFailPerDish * dishesCount), 0.1f, 1);

        var colorKeys = new GradientColorKey[2];
        var alphakeys = new GradientAlphaKey[2];

        colorKeys[0].color = _passColor;
        colorKeys[0].time = _durationToFail;

        colorKeys[1].color = _failColor;
        colorKeys[1].time = 1f;

        alphakeys[0].alpha = 1f;
        alphakeys[0].time = 0f;
        alphakeys[1].alpha = 1f;
        alphakeys[1].time = 1f;

        _stabilitygradient.SetKeys(colorKeys, alphakeys);
    }

    private void Update()
    {
        if(_active)
        {
            _coolDownTimer += Time.deltaTime / _durationToCooldownFull;
            if (_coolDownTimer >= _durationToCooldownFull)
            {
                _coolDownTimer = 0;
            }

            _stabilityTimer += Time.deltaTime / _durationToStabilityFull;
            if (_stabilityTimer >= _durationToStabilityFull)
            {
                _stabilityTimer = 0;
            }


            UpdateCountDownTimer();
            UpdateStabilityBar();

            if(_countdownBar.fillAmount <= 0)
            {
                OnStabilityCompleted?.Invoke(false);
                CompleteStabilityCheck();
            }
        }
    }

    public void OnStability(InputAction.CallbackContext value)
    {
        if(!_active) { return; }

        if(value.started)
        {
            if(_stabilityBar.fillAmount < _durationToFail 
                && _countdownBar.fillAmount > 0)
            {
                OnStabilityCompleted?.Invoke(true);
            }
            else
            {
                OnStabilityCompleted?.Invoke(false);
            }

            CompleteStabilityCheck();
        }
    }

    private void OnDishesCleaned()
    {
        CompleteStabilityCheck();
    }

    private void OnPlayerStumble()
    {
        CompleteStabilityCheck();
    }

    private void UpdateStabilityBar()
    {
        float fillValue = Mathf.Lerp(0f, 1f, _stabilityTimer);
        float colorValue = Mathf.Lerp(0f, 1f, _stabilityTimer);

        if(fillValue > _durationToFail)
        {
            _animator.SetBool("Fall", true);
        }
        else
        {
            _animator.SetBool("Fall", false);
        }

        _stabilityBar.fillAmount = fillValue;
        _stabilityBar.color = _stabilitygradient.Evaluate(colorValue);
    }

    private void UpdateCountDownTimer()
    {
        float fillValue = Mathf.Lerp(0f, 1f, _coolDownTimer);
        float colorValue = Mathf.Lerp(0f, 1f, _coolDownTimer);

        _countdownBar.fillAmount = 1 - fillValue;
        _countdownBar.color = _cooldowngradient.Evaluate(colorValue);
    }

    private void CompleteStabilityCheck()
    {
        _active = false;
        _stabilityTimer = 0f;
        _coolDownTimer = 0f;
        _stabilityBarGO.SetActive(false);
        _cooldownTimerGO.SetActive(false);
    }
}
