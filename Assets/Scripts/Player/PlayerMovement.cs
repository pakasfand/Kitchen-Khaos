using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float _walkingSpeed = 5f;
    [SerializeField] private float _rotationSpeed = 2f;
    [SerializeField] private Animator _animator;
    [SerializeField] private float _detectionRadius;
    [SerializeField] private LayerMask _interactionLayers;

    private Vector3 _direction;
    private Rigidbody _rigidbody;
    private List<DishTypes> _dishesCollected;
    private bool _isCleaning;

    public static Action OnPlayerStartedCleaning;
    public static Action OnPlayerStoppedCleaning;

    public float WalkingSpeed { get { return this._walkingSpeed; } set { this._walkingSpeed = value; } }

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();

        _dishesCollected = new List<DishTypes>();
    }

    private void FixedUpdate()
    {
        _rigidbody.velocity = _direction * (_walkingSpeed);
        _animator.SetFloat("Speed", _rigidbody.velocity.magnitude);

        if (_direction != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(_direction), Time.deltaTime * _rotationSpeed);
        }
    }

    public void OnMovement(InputAction.CallbackContext value)
    {
        Vector2 inputMovement = value.ReadValue<Vector2>();

        _direction = new Vector3(inputMovement.x, 0f, inputMovement.y);
    }

    public void OnInteract(InputAction.CallbackContext value)
    {
        if (value.started)
        {
            //_animator.SetTrigger("Interact");

            var hitColliers = Physics.OverlapSphere(transform.position,
                                _detectionRadius,
                                _interactionLayers);

            for (int i = 0; i < hitColliers.Length; i++)
            {
                if (TryToCleanDishes(hitColliers[i]))  { return; }
                if (TryToPickUpDish (hitColliers[i]))  { return; }
            }
        }

        if(_isCleaning && value.canceled)
        {
            _isCleaning = false;
            OnPlayerStoppedCleaning?.Invoke();
        }
    }

    private bool TryToCleanDishes(Collider collider)
    {
        if(_dishesCollected.Count == 0) { return false; }

        var sink = collider.GetComponent<Sink>();

        if (sink)
        {
            CleanDishes();
            return true;
        }

        return false;
    }

    private void CleanDishes()
    {
        // Play clean dishes anim
        _isCleaning = true;
        OnPlayerStartedCleaning?.Invoke();

        Debug.Log("Dishes cleaned!");
        _dishesCollected.Clear();
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
        // Play pick up anim
        Debug.Log("Dishes picked up!");
        enemyAi.StopAllCoroutines();
        _dishesCollected.Add(enemyAi.DishType);
        Destroy(enemyAi.gameObject);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _detectionRadius);
    }
}