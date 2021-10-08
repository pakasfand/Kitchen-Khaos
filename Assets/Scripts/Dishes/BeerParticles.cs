using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class BeerParticles : MonoBehaviour
{

    ParticleSystem particleSys;
    PlayerInteraction player;
    DrunkEffect drunkEffect;


    List<Particle> enterParticles;

    public Action OnPlayerHit;

    private float timer;

    private void Awake()
    {
        particleSys = GetComponent<ParticleSystem>();
        player = GameObject.FindWithTag("Player").GetComponent<PlayerInteraction>();
        particleSys.trigger.SetCollider(0, player.GetComponent<Collider>());
        enterParticles = new List<Particle>();
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        timer = Mathf.Max(0, timer);
    }

    private void OnParticleTrigger()
    {
        Debug.Log("Player entered particles");
        ParticlePhysicsExtensions.GetTriggerParticles(particleSys, ParticleSystemTriggerEventType.Enter, enterParticles);
        if (enterParticles.Count != 0) OnPlayerHit?.Invoke();
    }
}
