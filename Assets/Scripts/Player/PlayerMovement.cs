using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float _walkingSpeed = 5f;
    [SerializeField] private float _rotationSpeed = 2f;

    private float speedMultiplier = 1;
    private Vector3 _direction;
    private Rigidbody _rigidbody;
    //private Animator animator;

    public float WalkingSpeed { get { return this._walkingSpeed; } set { this._walkingSpeed = value; } }

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        //animator = GetComponentInChildren<Animator>();
    }

    private void FixedUpdate()
    {
        _rigidbody.velocity = _direction * (_walkingSpeed * speedMultiplier);
        //animator.SetFloat("Speed", m_rb.velocity.magnitude);
    }

    public void OnMovement(InputAction.CallbackContext value)
    {
        Vector2 inputMovement = value.ReadValue<Vector2>();

        _direction = new Vector3(inputMovement.x, 0f, inputMovement.y);

        if (_direction != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, 
                Quaternion.LookRotation(_direction), Time.deltaTime * _rotationSpeed);
        }
    }

    public void OnInteract(InputAction.CallbackContext value)
    {
        if(value.started)
        {
            // Interact
        }
    }

    //public void SetSpeedModifier(float speedModifier)
    //{
    //    speedMultiplier = 1 + speedModifier;
    //}
}