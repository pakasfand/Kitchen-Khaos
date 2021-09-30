using System;
using System.Collections.Generic;
using UnityEngine;

namespace Patrol
{
    [Serializable]
    public class PatrolPath
    {
        public List<Transform> Waypoints;
    }
}