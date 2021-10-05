using System;
using UnityEngine;

public class Fireball : MonoBehaviour
{

    [SerializeField] Rigidbody rigidBody;
    [SerializeField] float maxLifetime;
    [SerializeField] float fireballSpeed;
    [SerializeField] LayerMask collisionLayer;
    [SerializeField] float effectTimeOnPlayer;
    [SerializeField] float speedBoostOnPlayer;

    float timer = 0;
    bool canMove;
    Vector3 movingDirection;
    Vector3 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

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

        transform.localScale = originalScale;

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
        transform.localScale = originalScale;

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            IgnitePlayer(other.gameObject);
            gameObject.SetActive(false);
        }

        if (ExtensionMethods.LayerMaskExtensions.IsInLayerMask(collisionLayer, other.gameObject))
        {
            gameObject.SetActive(false);
        }

    }

    private void IgnitePlayer(GameObject gameObject)
    {
        gameObject.GetComponent<PlayerInteraction>().Ignite(effectTimeOnPlayer);
        gameObject.GetComponent<PlayerMovement>().SpeedUp(effectTimeOnPlayer, speedBoostOnPlayer);
    }

}