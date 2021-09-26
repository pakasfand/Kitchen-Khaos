using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class BeerParticles : MonoBehaviour
{

    ParticleSystem particleSys;
    PlayerInteraction player;


    private float timer;

    private void Awake()
    {
        particleSys = GetComponent<ParticleSystem>();
        player = GameObject.FindWithTag("Player").GetComponent<PlayerInteraction>();
        particleSys.trigger.SetCollider(0, player.GetComponent<Collider>());
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        timer = Mathf.Max(0, timer);
    }

    private void OnParticleTrigger()
    {
    }
}
