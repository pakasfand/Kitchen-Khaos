
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class Cup : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float normalHopHeigth;
    [SerializeField] float spillingHopHeigth;
    [SerializeField] int jumpsBeforeTryingToSpill;
    [SerializeField] Transform model;
    [SerializeField] float gravity;
    [Header("Spilling")]
    [SerializeField] GameObject[] puddlePrefabs;
    [SerializeField] ParticleSystem tryToSpillIndicator;
    [Range(0, 1f)] [SerializeField] float chanceToSpill;
    [Range(0, 1f)] [SerializeField] float chanceToSpillIncrement;
    [SerializeField] float timeToTryToSpill;
    [SerializeField] float waitTimeAfterTrySpilling;

    AIBehaviour AI;

    float currentChanceToSpill;
    float tryToSpillTimer = 0;
    Coroutine hopCoroutine;
    Coroutine tryToSpillCoroutine;

    private void Awake()
    {
        AI = GetComponent<AIBehaviour>();
    }

    private void Start()
    {
        currentChanceToSpill = chanceToSpill;
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
        float time = 0;
        float hopTime = GetHopTime(hopHeigth);
        float hopVelocity = GetHopVelocity(hopHeigth);
        float initialY = model.localPosition.y;
        yield return new WaitUntil(() =>
        {
            time += Time.deltaTime;
            float calculatedPosition = (-gravity * time * time / 2) + hopVelocity * time + initialY;

            model.localPosition = new Vector3(model.localPosition.x, calculatedPosition, model.localPosition.z);

            if (time >= hopTime || model.localPosition.y <= initialY)
            {
                model.localPosition = new Vector3(model.localPosition.x, initialY, model.localPosition.z);
                return true;
            }

            return false;
        });
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
        tryToSpillIndicator.Play();

        for (int i = 0; i < jumpsBeforeTryingToSpill; i++)
        {
            yield return StartCoroutine(Hop(spillingHopHeigth));
        }

        if (CanSpill())
        {
            GameObject puddlePrefab = GetRandomPuddle();
            if (puddlePrefab != null) Instantiate(puddlePrefab, transform.position, puddlePrefab.transform.rotation, null);
        }

        tryToSpillIndicator.Stop();
        yield return new WaitForSeconds(waitTimeAfterTrySpilling);

        AI.enabled = true;
        tryToSpillCoroutine = null;
    }

    private GameObject GetRandomPuddle()
    {
        return puddlePrefabs[Random.Range(0, puddlePrefabs.Length)];
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

    private void OnDisable()
    {
        StopAllCoroutines();
        currentChanceToSpill = chanceToSpill;
        tryToSpillTimer = 0;
    }
}
