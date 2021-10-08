using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class StabilityCheck : MonoBehaviour
{
    [SerializeField] private GameObject _stabilityBarGO;
    [SerializeField] private GameObject _cooldownTimerGO;
    [SerializeField] private GameObject _cursor;
    [SerializeField] private Image _stabilityBar;
    [SerializeField] private Image _countdownBar;
    [SerializeField] private float _durationToCooldownFull;
    [SerializeField] private float _durationToStabilityFull;
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
        PlayerInteraction.OnChefIgnited += OnChefIgnited;
        Sink.OnDishesCleaned += OnDishesCleaned;
    }

    private void OnDisable()
    {
        PlayerInteraction.OnStabilityCheckBegin -= OnStabilityCheckBegin;
        PlayerInteraction.OnPlayerStumble -= OnPlayerStumble;
        PlayerInteraction.OnChefIgnited -= OnChefIgnited;
        Sink.OnDishesCleaned -= OnDishesCleaned;
    }

    private void OnStabilityCheckBegin(int dishesCount)
    {
        _stabilityBarGO.SetActive(true);
        _cooldownTimerGO.SetActive(true);

        _active = true;
        
        _durationToFail = Mathf.Clamp(_durationToFailPerDish * dishesCount, 0.1f, 0.9f);

        var width = Mathf.Lerp(0f, 236f,  _durationToFail);
        _stabilityBar.rectTransform.sizeDelta = new Vector2(width, _stabilityBar.rectTransform.sizeDelta.y);
    }

    private void Update()
    {
        if(_active)
        {
            _coolDownTimer += Time.deltaTime;
            if (_coolDownTimer >= _durationToCooldownFull)
            {
                _coolDownTimer = 0;
            }

            _stabilityTimer += Time.deltaTime;
            if (_stabilityTimer >= _durationToStabilityFull)
            {
                _stabilityTimer = 0f;
            }

            UpdateCountDownTimer();
            UpdateStabilityBar();

            if(_countdownBar.fillAmount <= 0)
            {
                OnStabilityCompleted?.Invoke(false);
                CompleteStabilityCheck();
            }
        }
        
        Debug.Log(_cursor.GetComponent<RectTransform>().anchoredPosition.x);
    }

    public void OnStability(InputAction.CallbackContext value)
    {
        if(!_active) { return; }

        if(value.started)
        {
            if(_stabilityTimer > _durationToFail 
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

    private void OnDishesCleaned() => CompleteStabilityCheck();

    private void OnPlayerStumble() => CompleteStabilityCheck();
    
    private void OnChefIgnited() => CompleteStabilityCheck();

    private void UpdateStabilityBar()
    {
        float cursorPosition = Mathf.Lerp(-29f, 197f, _stabilityTimer / _durationToStabilityFull);
        
        var anchoredPosition = _cursor.GetComponent<RectTransform>().anchoredPosition;
        anchoredPosition = new Vector3(cursorPosition, anchoredPosition.y);
        _cursor.GetComponent<RectTransform>().anchoredPosition = anchoredPosition;

        if(_stabilityTimer > _durationToFail)
        {
            _animator.SetBool("Fall", false);
        }
        else
        {
            _animator.SetBool("Fall", true);
        }
    }

    private void UpdateCountDownTimer()
    {
        float fillValue = Mathf.Lerp(0f, 1f, _coolDownTimer / _durationToCooldownFull);

        _countdownBar.fillAmount = 1 - fillValue;
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
