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
    private bool _burning;
    private float _powerUpSpeedModifier;
    private float _effectSpeedModifier;

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
        if (_direction != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(_direction), Time.deltaTime * _rotationSpeed);
        }

        if (_burning)
        {
            _direction = transform.forward.normalized;
            _animator.SetFloat("SpeedMultiplier", _effectSpeedModifier);
        }
        else
        {
            _animator.SetFloat("SpeedMultiplier", 1f);
        }

        var desiredVelocity = _direction * _walkingSpeed * _powerUpSpeedModifier * _effectSpeedModifier;

        _rigidbody.velocity = _playerInteraction.IsInteracting ?
                                Vector3.zero : desiredVelocity;

        _animator.SetFloat("Speed", _rigidbody.velocity.magnitude);
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
            _burning = false;
            _effectSpeedModifier = 1f;
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

    public void SpeedUp(float disableTime, float speedBoost)
    {
        _burning = true;
        _disableTimeLeft = disableTime;
        _effectSpeedModifier = speedBoost;
    }
}