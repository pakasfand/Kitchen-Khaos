using UnityEngine;
using System.Collections.Generic;
using System;

public class Splatter : MonoBehaviour
{
    [SerializeField] float maxLifetime;
    public List<Sprite> sprites; //ref to the sprites which will be used by sprites renderer

    SpriteRenderer spriteRenderer;//ref to sprite renderer component
    Animator animator;
    ParticleSystem particles;
    float remainingTime = 0;

    public static Action OnSplatterCreated;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponent<Animator>();
        particles = GetComponentInChildren<ParticleSystem>();
    }

    private void Start()
    {
        remainingTime = Mathf.Infinity;
        spriteRenderer.sprite = sprites[UnityEngine.Random.Range(0, sprites.Count)];
        OnSplatterCreated?.Invoke();
    }


    private void Update()
    {
        remainingTime = Mathf.Max(0, remainingTime - Time.deltaTime);
        if (remainingTime == 0)
        {
            PlayDisappearAnim();
        }
    }

    public void StartTimer()
    {
        remainingTime = maxLifetime;
        particles.Play();
    }

    public void Destroy()
    {
        Destroy(this.gameObject);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerInteraction>().Stumble();
            PlayDisappearAnim();
        }
    }

    private void PlayDisappearAnim()
    {
        animator.SetTrigger("Disappear");
        particles.Stop();
    }
}
