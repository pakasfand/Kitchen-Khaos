using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private float _detectionRadius;
    [SerializeField] private LayerMask _interactionLayers;
    [SerializeField] private Animator _animator;
    [SerializeField] private Transform _leftStackPosition;
    [SerializeField] private Transform _rightStackPosition;

    [Header("Stumble Parameters")]
    [SerializeField] private int _chanceToStumblePerDish;
    [SerializeField] private float _stumbleCheckRate;

    [Header("Collected Dishes")]
    [SerializeField] private GameObject _collectedPlate;
    [SerializeField] private GameObject _collectedCup;

    [Header("Stack Rotation Parameters")]
    [SerializeField] private Vector3 _leftStackIdleRotation;
    [SerializeField] private Vector3 _rightStackIdleRotation;
    [SerializeField] private Vector3 _leftStackMovingRotation;
    [SerializeField] private Vector3 _rightStackMovingRotation;

    private List<DishType> _dishesCollected;
    private bool _isInteracting;
    private Vector3 _leftStackOffset = Vector3.zero;
    private Vector3 _rightStackOffset = Vector3.zero;
    private bool _alternateStack;

    public static Action OnPlayerStartedCleaning;
    public static Action OnPlayerStoppedCleaning;
    public static Action<int> OnStabilityCheckBegin;

    public bool IsInteracting => _isInteracting;
    public List<DishType> DishesCollected => _dishesCollected;

    private float _stabilityCheckTimer;

    private void OnEnable()
    {
        Sink.OnDishesCleaned += OnDishesCleaned;
        StabilityCheck.OnStabilityCompleted += OnStabilityCompleted;
    }

    private void OnDisable()
    {
        Sink.OnDishesCleaned -= OnDishesCleaned;
        StabilityCheck.OnStabilityCompleted -= OnStabilityCompleted;
    }

    private void Awake()
    {
        _dishesCollected = new List<DishType>();
    }

    private void Update()
    {
        if (_animator.GetFloat("Speed") == 0)
        {
            _rightStackPosition.localRotation = Quaternion.Euler(_rightStackIdleRotation);
            _leftStackPosition.localRotation = Quaternion.Euler(_leftStackIdleRotation);
        }
        else
        {
            _rightStackPosition.localRotation = Quaternion.Euler(_rightStackMovingRotation);
            _leftStackPosition.localRotation = Quaternion.Euler(_leftStackMovingRotation);
        }

        if (_dishesCollected.Count > 0)
        {
            _stabilityCheckTimer += Time.deltaTime;

            if (_stabilityCheckTimer >= _stumbleCheckRate)
            {
                _stabilityCheckTimer = 0;
                var rng = UnityEngine.Random.Range(0, 100);
                if (rng < _dishesCollected.Count * _chanceToStumblePerDish)
                {
                    OnStabilityCheckBegin?.Invoke(_dishesCollected.Count);
                }
            }
        }
        else
        {
            _stabilityCheckTimer = 0;
        }
    }

    public void OnInteract(InputAction.CallbackContext value)
    {
        if (value.started)
        {
            var hitColliers = Physics.OverlapSphere(transform.position,
                                _detectionRadius,
                                _interactionLayers);

            for (int i = 0; i < hitColliers.Length; i++)
            {
                if (TryToCleanDishes(hitColliers[i])) { return; }
                if (TryToPickUpDish(hitColliers[i])) { return; }
            }
        }

        if (_isInteracting && value.canceled)
        {
            _isInteracting = false;
            _animator.SetBool("Clean", false);
            OnPlayerStoppedCleaning?.Invoke();
        }
    }

    private bool TryToCleanDishes(Collider collider)
    {
        if (_dishesCollected.Count == 0) { return false; }

        var sink = collider.GetComponent<Sink>();

        if (sink)
        {
            StartCleaningDishes();
            return true;
        }

        return false;
    }

    private void StartCleaningDishes()
    {
        _animator.SetBool("Clean", true);

        _isInteracting = true;
        OnPlayerStartedCleaning?.Invoke();
    }

    private bool TryToPickUpDish(Collider collider)
    {
        if (_animator.GetBool("Pick up left")) { return false; }
        if (_animator.GetBool("Pick up right")) { return false; }

        var enemyAi = collider.GetComponent<AIBehaviour>();

        if (enemyAi)
        {
            PickUpDish(enemyAi);
            return true;
        }

        return false;
    }

    private void PickUpDish(AIBehaviour enemyAi)
    {
        enemyAi.StopAllCoroutines();
        _dishesCollected.Add(enemyAi.dishType);

        var dish = Instantiate(_collectedPlate,
            _alternateStack ? _leftStackPosition : _rightStackPosition);

        _alternateStack = !_alternateStack;

        if (_alternateStack)
        {
            //_animator.SetBool("Pick up right", true);
            _leftStackOffset += new Vector3(0.0f, 0.2f, 0.0f);
            dish.transform.localPosition += _leftStackOffset;
        }
        else
        {
            //_animator.SetBool("Pick up left", true);
            _rightStackOffset += new Vector3(0.0f, 0.2f, 0.0f);
            dish.transform.localPosition += _rightStackOffset;
        }

        enemyAi.gameObject.SetActive(false);
    }

    private void OnDishesCleaned()
    {
        _animator.SetBool("Clean", false);
        DisableDishes();
    }

    private void OnStabilityCompleted(bool status)
    {
        if (!status)
        {
            Stumble();
        }
    }

    private void DisableDishes()
    {
        _leftStackOffset = Vector3.zero;
        _rightStackOffset = Vector3.zero;

        foreach (Transform child in _leftStackPosition)
        {
            child.gameObject.SetActive(false);
        }

        foreach (Transform child in _rightStackPosition)
        {
            child.gameObject.SetActive(false);
        }
    }

    public void Stumble()
    {
        _animator.SetBool("Stumble", true);
        DisableDishes();
        _dishesCollected.Clear();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _detectionRadius);
    }
}
