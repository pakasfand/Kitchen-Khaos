using System;
using UnityEngine;

public class Fireball : MonoBehaviour
{

    [SerializeField] Rigidbody rigidBody;
    [SerializeField] float maxLifetime;
    [SerializeField] float fireballSpeed;


    float timer = 0;
    public bool canMove;
    private Vector3 movingDirection;

    private void FixedUpdate()
    {
        if (canMove)
        {
            rigidBody.MovePosition(transform.position + movingDirection.normalized * fireballSpeed * Time.fixedDeltaTime);
        }
    }

    private void OnEnable()
    {
        timer = 0;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= maxLifetime)
        {
            gameObject.SetActive(false);
        }
    }

    public void Launch(Vector3 direction)
    {
        transform.forward = direction.normalized;
        canMove = true;
        movingDirection = direction;
    }

}