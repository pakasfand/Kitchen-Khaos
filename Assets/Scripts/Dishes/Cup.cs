
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Cup : MonoBehaviour
{
    [SerializeField] GameObject puddlePrefab;
    [Range(0, 1f)] [SerializeField] float chanceToSpill;
    [Range(0, 1f)] [SerializeField] float chanceToSpillIncrement;
    [SerializeField] float timeToTryToSpill;
    [SerializeField] int maxSpillTimes;
    [SerializeField] float normalHopHeigth;
    [SerializeField] float spillingHopHeigth;
    [SerializeField] Rigidbody rigidBody;
    [SerializeField] int jumpsBeforeTryingToSpill;

    AIBehaviour AI;

    float currentChanceToSpill;
    float tryToSpillTimer = 0;
    float gravity;
    Coroutine hopCoroutine;
    Coroutine tryToSpillCoroutine;

    private void Awake()
    {
        AI = GetComponent<AIBehaviour>();
    }

    private void Start()
    {
        currentChanceToSpill = chanceToSpill;
        gravity = Physics.gravity.magnitude;
        hopCoroutine = StartCoroutine(Hop(normalHopHeigth));
    }


    private void Update()
    {
        if (tryToSpillCoroutine != null) return;
        if (hopCoroutine == null && !AI.IsStopped()) hopCoroutine = StartCoroutine(Hop(normalHopHeigth));
        tryToSpillTimer += Time.deltaTime;
        if (tryToSpillTimer >= timeToTryToSpill)
        {
            tryToSpillCoroutine = StartCoroutine(TryToSpill());
        }
    }

    private IEnumerator Hop(float hopHeigth)
    {
        rigidBody.velocity = new Vector3(rigidBody.velocity.x, GetHopVelocity(hopHeigth), rigidBody.velocity.z);
        yield return new WaitForSeconds(GetHopTime(hopHeigth));
        hopCoroutine = null;
    }

    private float GetHopVelocity(float hopHeigth)
    {
        return Mathf.Sqrt(2 * gravity * hopHeigth);
    }

    private float GetHopTime(float hopHeigth)
    {
        return (2 * GetHopVelocity(hopHeigth)) / gravity;
    }


    private IEnumerator TryToSpill()
    {
        tryToSpillTimer = 0;
        yield return new WaitUntil(() =>
        {
            if (hopCoroutine == null) hopCoroutine = StartCoroutine(Hop(normalHopHeigth));
            return AI.HasReachedDestination();

        });

        yield return new WaitUntil(() =>
        {
            return hopCoroutine == null;
        });

        AI.enabled = false;

        for (int i = 0; i < jumpsBeforeTryingToSpill; i++)
        {
            yield return StartCoroutine(Hop(spillingHopHeigth));
        }

        if (CanSpill())
        {
            if (puddlePrefab != null) Instantiate(puddlePrefab, transform.position, puddlePrefab.transform.rotation, null);
        }
        yield return new WaitForSeconds(2f);
        AI.enabled = true;
        tryToSpillCoroutine = null;
    }

    private bool CanSpill()
    {
        float randomValue = Random.Range(0, 1f);
        if (currentChanceToSpill > randomValue)
        {
            currentChanceToSpill = chanceToSpill;
            return true;
        }
        else
        {
            currentChanceToSpill += chanceToSpillIncrement;
            return false;
        }
    }
}
