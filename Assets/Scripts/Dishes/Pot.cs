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


    Vector3 originalScale;
    float modelRotation;

    private void Awake()
    {
        AI = GetComponent<AIBehaviour>();
        anim = GetComponent<Animation>();
        originalScale = transform.localScale;
    }



    private void Update()
    {
        explosionTimer += Time.deltaTime;


        model.rotation = Quaternion.Euler(model.rotation.eulerAngles.x, modelRotation, model.rotation.eulerAngles.z);

        if (explosionTimer >= timeToExplode)
        {
            AI.enabled = false;
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
        AI.enabled = true;
    }

    private void Rotate()
    {
        modelRotation += angularSpeed * Time.deltaTime * AI.GetVelocityFraction();
    }

    private void OnDisable()
    {

        anim.Stop();
    }
}
