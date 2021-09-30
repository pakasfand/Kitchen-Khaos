using System;
using System.Collections.Generic;
using System.Numerics;
using Cinemachine;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Patrol
{
    public class PatrolAI : MonoBehaviour
    {
        [SerializeField] private float _patrolSpeed;
        
        public List<Transform> Waypoints;

        private Transform _currentWaypoint;
        private int _currentWaypointIndex;
        
        private void Update()
        {
            transform.position = Vector3.MoveTowards(transform.position,
                _currentWaypoint.position,
                Time.deltaTime * _patrolSpeed);

            if (Vector3.Distance(transform.position,
                _currentWaypoint.position) < 0.1f)
            {
                _currentWaypointIndex += 1;

                if (_currentWaypointIndex >= Waypoints.Count)
                {
                    _currentWaypointIndex = 0;
                }
                
                _currentWaypoint = Waypoints[_currentWaypointIndex];
            }
        }
    }
}