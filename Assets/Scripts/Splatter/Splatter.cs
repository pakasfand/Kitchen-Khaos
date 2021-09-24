using UnityEngine;
using System.Collections.Generic;
using System;

public class Splatter : MonoBehaviour
{
    [SerializeField] float maxLifetime;
    public List<Sprite> sprites; //ref to the sprites which will be used by sprites renderer

    SpriteRenderer spriteRenderer;
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

    private void OnEnable() => GetComponentInChildren<SplatterCollider>().OnTrigger += OnTrigger;
    private void OnDisable() => GetComponentInChildren<SplatterCollider>().OnTrigger -= OnTrigger;



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

    private void OnTrigger(Collider player)
    {
        player.gameObject.GetComponent<PlayerInteraction>().Stumble();
        PlayDisappearAnim();
    }

    public void Destroy()
    {
        Destroy(this.gameObject);
    }

    private void PlayDisappearAnim()
    {
        animator.SetTrigger("Disappear");
    }
}
