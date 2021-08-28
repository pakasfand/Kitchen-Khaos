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
    [SerializeField] private PlayerInteraction _playerInteraction;

    private Vector3 _direction;
    private Rigidbody _rigidbody;
    private float _powerUpTimeLeft;
    private float _disableTimeLeft;
    private float _powerUpSpeedModifier;
    private bool _isDisable;

    public float WalkingSpeed { get { return this._walkingSpeed; } set { this._walkingSpeed = value; } }

    private void OnEnable()
    {
        PowerUp.OnPowerUpConsumed += OnPowerUpConsumed;
    }

    private void OnDisable()
    {
        PowerUp.OnPowerUpConsumed -= OnPowerUpConsumed;
    }

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if(_isDisable) { return; }

        var desiredVelocity = _direction * _walkingSpeed * _powerUpSpeedModifier;

        _rigidbody.velocity = _playerInteraction.IsInteracting ?
                                Vector3.zero : desiredVelocity;

        _animator.SetFloat("Speed", _rigidbody.velocity.magnitude);

        if (_direction != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(_direction), Time.deltaTime * _rotationSpeed);
        }
    }

    private void Update()
    {
        if(_powerUpTimeLeft > 0)
        {
            _powerUpTimeLeft -= Time.deltaTime;
        }
        else
        {
            _powerUpSpeedModifier = 1f;
        }

        if (_disableTimeLeft > 0)
        {
            _disableTimeLeft -= Time.deltaTime;
        }
        else
        {
            _isDisable = false;
        }
    }

    public void OnMovement(InputAction.CallbackContext value)
    {
        Vector2 inputMovement = value.ReadValue<Vector2>();

        _direction = new Vector3(inputMovement.x, 0f, inputMovement.y);
    }

    private void OnPowerUpConsumed(float _lifeTime, float _speedBoost)
    {
        _animator.SetBool("Eat", true);
        _powerUpTimeLeft = _lifeTime;
        _powerUpSpeedModifier = _speedBoost;
    }

    public void DisableMovement(float disableTime)
    {
        _disableTimeLeft = disableTime;
        _isDisable = true;
    }
}