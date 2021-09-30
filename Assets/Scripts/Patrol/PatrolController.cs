using System;
using System.Collections.Generic;
using UnityEngine;

namespace Patrol
{
    public class PatrolController : MonoBehaviour
    {
        [SerializeField] private List<Patrol> _patrols;

        private void Start()
        {
            foreach (var patrol in _patrols)
            {
                patrol.PatrolAI.BeginPatrol(patrol.PatrolPath.Waypoints);
            }
        }
    }
}
