

using UnityEngine;

public class Beer : MonoBehaviour
{
    [SerializeField] float timeToSplatterBeer;
    [SerializeField] ParticleSystem beerParticles;

    AIBehaviour AI;
    Animator animator;
    float splatterTimer = 0;
    private bool splattering;

    private void Awake()
    {
        AI = GetComponent<AIBehaviour>();
        animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (AI.enabled == false) return;
        splatterTimer += Time.deltaTime;

        if (splatterTimer >= timeToSplatterBeer)
        {
            SplatterBeer();
            splatterTimer = 0;
        }
    }

    private void SplatterBeer()
    {
        AI.Stop();
        animator.SetTrigger("Attack");
    }

    public void StartSplatter()
    {
        beerParticles.Play();
    }

    public void StopSplatter()
    {
        beerParticles.Stop();
    }

}
