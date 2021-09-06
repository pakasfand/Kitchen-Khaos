using System;
using UnityEngine;

public class Pot : MonoBehaviour
{

    [SerializeField] float angularSpeed;
    [SerializeField] float timeToExplode;
    [SerializeField] int numberOfFireballs;
    [SerializeField] Transform fireballSpawnPoint;
    [SerializeField] Fireball fireballPrefab;
    [SerializeField] GameObject explosionPrefab;
    [SerializeField] Transform model;
    [SerializeField] ObjectPool fireballsPool;

    float explosionTimer = 0;

    public static Action OnPotExplodes;

    AIBehaviour AI;
    Animation anim;
    private Vector3 originalScale;

    private void Awake()
    {
        AI = GetComponent<AIBehaviour>();
        anim = GetComponent<Animation>();
        originalScale = transform.localScale;

    }



    private void Update()
    {
        explosionTimer += Time.deltaTime;


        if (explosionTimer >= timeToExplode)
        {
            AI.Stop();
            anim.Play();
        }

        if (!AI.IsStopped())
        {
            Rotate();
        }
    }

    public void Explode()
    {
        Instantiate(explosionPrefab, fireballSpawnPoint.position, Quaternion.identity, null);
        for (int i = 0; i < numberOfFireballs; i++)
        {
            if (fireballPrefab == null) return;
            Fireball fireBall = fireballsPool.RequestSubject().GetComponent<Fireball>();
            fireBall.gameObject.SetActive(true);
            fireBall.transform.position = fireballSpawnPoint.position;
            fireBall.transform.parent = null;

            Vector3 direction = Quaternion.AngleAxis((360 / numberOfFireballs) * i, Vector3.up) * Vector3.forward;
            fireBall.Launch(direction);
        }

        explosionTimer = 0;
        OnPotExplodes?.Invoke();
    }

    private void Rotate()
    {
        model.Rotate(new Vector3(0, angularSpeed * Time.deltaTime * AI.GetVelocityFraction(), 0), Space.World);
    }

    private void OnDisable()
    {

        anim.Stop();
    }
}
