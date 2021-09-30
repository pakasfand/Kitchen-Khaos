using System;
using System.Collections.Generic;
using System.Numerics;
using Cinemachine;
using UnityEditor;
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
        private bool _active = false;
        
        private void Update()
        {
            if(!_active) {return;}
            
            transform.LookAt(_currentWaypoint, Vector3.up);
            
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
        
        public void BeginPatrol(List<Transform> waypoints)
        {
            _active = true;
            Waypoints = waypoints;
            _currentWaypoint = Waypoints[_currentWaypointIndex];
        }
    }
    
}
