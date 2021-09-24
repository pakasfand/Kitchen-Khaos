

using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class Beer : MonoBehaviour
{
    [SerializeField] float timeToSplatterBeer;
    [SerializeField] BeerParticles beerParticles;

    AIBehaviour AI;
    Animator animator;
    PostProcessVolume volume;
    DrunkEffect drunkEffect;



    float splatterTimer = 0;
    private bool splattering;

    private void Awake()
    {
        drunkEffect = FindObjectOfType<DrunkEffect>();
        AI = GetComponent<AIBehaviour>();
        animator = GetComponentInChildren<Animator>();
    }

    private void OnEnable() => beerParticles.OnPlayerHit += drunkEffect.Play;
    private void OnDisable() => beerParticles.OnPlayerHit -= drunkEffect.Play;

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
        beerParticles.GetComponent<ParticleSystem>().Play();
    }

    public void StopSplatter()
    {
        beerParticles.GetComponent<ParticleSystem>().Stop();
    }

}
