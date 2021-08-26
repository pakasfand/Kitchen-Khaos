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
    [SerializeField] private GameObject _collectedPlate;
    [SerializeField] private Transform _leftStackPosition;
    [SerializeField] private Transform _rightStackPosition;

    [SerializeField] private Vector3 _leftStackIdleRotation;
    [SerializeField] private Vector3 _rightStackIdleRotation;
    [SerializeField] private Vector3 _leftStackMovingRotation;
    [SerializeField] private Vector3 _rightStackMovingRotation;

    private List<DishTypes> _dishesCollected;
    private bool _isInteracting;
    private Vector3 _leftStackOffset = Vector3.zero;
    private Vector3 _rightStackOffset = Vector3.zero;

    public static Action OnPlayerStartedCleaning;
    public static Action OnPlayerStoppedCleaning;

    public bool IsInteracting => _isInteracting;

    public List<DishTypes> DishesCollected => _dishesCollected;

    public bool _alternateStack;

    private void OnEnable()
    {
        Sink.OnDishesCleaned += OnDishesCleaned;
    }

    private void OnDisable()
    {
        Sink.OnDishesCleaned -= OnDishesCleaned;
    }

    private void Awake()
    {
        _dishesCollected = new List<DishTypes>();
    }

    private void Update()
    {
        if(_animator.GetFloat("Speed") == 0)
        {
            _rightStackPosition.localRotation = Quaternion.Euler(_rightStackIdleRotation);
            _leftStackPosition.localRotation = Quaternion.Euler(_leftStackIdleRotation);
        }
        else
        {
            _rightStackPosition.localRotation = Quaternion.Euler(_rightStackMovingRotation);
            _leftStackPosition.localRotation = Quaternion.Euler(_leftStackMovingRotation);
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
        _dishesCollected.Add(enemyAi.DishType);

        var dish = Instantiate(_collectedPlate, 
            _alternateStack ?_leftStackPosition : _rightStackPosition);

        _alternateStack = !_alternateStack;

        if(_alternateStack)
        {
            _animator.SetBool("Pick up right", true);
            _leftStackOffset += new Vector3(0.0f, 0.2f, 0.0f);
        }
        else
        {
            _animator.SetBool("Pick up left", true);
            _rightStackOffset += new Vector3(0.0f, 0.2f, 0.0f);
        }

        dish.transform.localPosition += _leftStackOffset;

        Destroy(enemyAi.gameObject);
    }

    private void OnDishesCleaned()
    {
        _animator.SetBool("Clean", false);
        DestroyDishes();
    }

    private void DestroyDishes()
    {
        _leftStackOffset = Vector3.zero;
        _rightStackOffset = Vector3.zero;

        foreach (Transform child in _leftStackPosition)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in _rightStackPosition)
        {
            Destroy(child.gameObject);
        }
    }

    public void Stumble()
    {
        _animator.SetBool("Stumble", true);
        DestroyDishes();
        _dishesCollected.Clear();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _detectionRadius);
    }
}
