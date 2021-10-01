using System;
using Patrol;
using UnityEngine;

namespace ExtensionMethods.Audio
{
    public class WitchAudioController : MonoBehaviour
    {
        [SerializeField] private AudioSource _witchLaugh;
        
        private void OnEnable()
        {
            PatrolAI.OnPatrolComplete += OnPatrolComplete;
        }

        private void OnDisable()
        {
            PatrolAI.OnPatrolComplete -= OnPatrolComplete;
        }

        private void OnPatrolComplete()
        {
            _witchLaugh.Play();
        }
    }
}