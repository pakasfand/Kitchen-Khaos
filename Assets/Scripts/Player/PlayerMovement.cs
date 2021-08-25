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

    public float WalkingSpeed { get { return this._walkingSpeed; } set { this._walkingSpeed = value; } }

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        var desiredVelocity = _direction * _walkingSpeed;

        _rigidbody.velocity = _playerInteraction.IsInteracting ?
                                Vector3.zero : desiredVelocity;

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
}