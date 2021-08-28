using System;
using UnityEngine;

public class Fireball : MonoBehaviour
{

    [SerializeField] Rigidbody rigidBody;
    [SerializeField] float maxLifetime;
    [SerializeField] float fireballSpeed;
    [SerializeField] LayerMask[] collisionLayers;
    [SerializeField] float effectTimeOnPlayer;
    [SerializeField] float speedBoostOnPlayer;

    float timer = 0;
    bool canMove;
    Vector3 movingDirection;

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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            IgnitePlayer(other.gameObject);
            gameObject.SetActive(false);
        }

        for (int i = 0; i < collisionLayers.Length; i++)
        {

            if (ExtensionMethods.LayerMaskExtensions.IsInLayerMask(collisionLayers[i], other.gameObject))
            {
                gameObject.SetActive(false);
            }
        }
    }

    private void IgnitePlayer(GameObject gameObject)
    {
        gameObject.GetComponent<PlayerInteraction>().Ignite(effectTimeOnPlayer);
        gameObject.GetComponent<PlayerMovement>().DisableMovement(effectTimeOnPlayer);
    }
}