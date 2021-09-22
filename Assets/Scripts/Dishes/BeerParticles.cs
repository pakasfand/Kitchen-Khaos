using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class BeerParticles : MonoBehaviour
{
    [SerializeField] float drunkEffectTime;

    ParticleSystem particleSys;
    PlayerInteraction player;

    private void Awake()
    {
        particleSys = GetComponent<ParticleSystem>();
        player = GameObject.FindWithTag("Player").GetComponent<PlayerInteraction>();
        particleSys.trigger.SetCollider(0, player.GetComponent<Collider>());
    }

    private void OnParticleTrigger()
    {
        player.GetDrunk();
    }
}
