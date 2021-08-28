
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class AIBehaviour : MonoBehaviour
{
    NavMeshAgent agent;
    [SerializeField] Vector2 walkingDistanceRange;
    [SerializeField] float minTurningAngle;
    [SerializeField] float dwellingTime;
    [SerializeField] float maxDelayToReachDestination;
    [SerializeField] float minDistanceToPlayer;
    [SerializeField] float escapeSpeed;
    public DishType dishType;
    [SerializeField] Animator animator;
    [SerializeField] private InteractionIndicator _interactionIndicator;

    Vector3 walkingDirection;
    Coroutine wandering;
    private int timeSinceNewDestination;
    GameObject player;
    Vector3 vectorToPlayer;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        player = GameObject.FindWithTag("Player");
        walkingDirection = new Vector3(Random.Range(0, 1f), 0, Random.Range(0, 1f));
    }

    void Update()
    {
        vectorToPlayer = player.transform.position - transform.position;
        if (IsPlayerNear())
        {
            if (wandering != null)
            {
                agent.ResetPath();
                StopCoroutine(wandering);
            }
            wandering = null;
            RunAway();
            return;
        }
        if (wandering != null) return;
        wandering = StartCoroutine(Wander());
    }

    public bool IsStopped()
    {
        return agent.isStopped;
    }

    public void Continue()
    {
        agent.isStopped = false;
    }

    public void Stop()
    {
        agent.isStopped = true;
    }

    public bool HasReachedDestination()
    {
        if (!agent.pathPending)
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                {
                    return true;
                }
            }
        }
        return false;

    }

    public float GetVelocityFraction()
    {
        return agent.velocity.magnitude / agent.speed;
    }

    private void LateUpdate()
    {
        if (animator != null) animator.SetFloat("Speed", GetVelocityFraction());
    }


    private void RunAway()
    {
        agent.velocity = -vectorToPlayer.normalized * escapeSpeed;
    }

    private bool IsPlayerNear()
    {
        return vectorToPlayer.sqrMagnitude < Mathf.Pow(minDistanceToPlayer, 2);
    }

    private IEnumerator Wander()
    {
        float turningAngle = Random.Range(minTurningAngle, 180);
        float walkingDistance = Random.Range(walkingDistanceRange.x, walkingDistanceRange.y);
        walkingDirection = Quaternion.AngleAxis(turningAngle, Vector3.up) * walkingDirection;

        Vector3 walkTo = walkingDirection.normalized * walkingDistance + transform.position;
        walkTo = new Vector3(walkTo.x, 0, walkTo.z);

        NavMesh.SamplePosition(walkTo, out NavMeshHit target, 1000, NavMesh.AllAreas);

        if (IsPathValid(target.position))
        {
            float timeToDestination = walkTo.magnitude / agent.speed;
            agent.destination = target.position;
            float timeSinceNewDestination = Time.time;
            yield return new WaitUntil(() =>
            {
                float time = Time.time - timeSinceNewDestination;
                return HasReachedDestination() || time - timeToDestination >= maxDelayToReachDestination;
            });
            agent.isStopped = true;
            yield return new WaitForSeconds(dwellingTime);
            agent.isStopped = false;
        }

        wandering = null;
    }



    private bool IsPathValid(Vector3 targetPosition)
    {
        NavMeshPath path = new NavMeshPath();
        bool pathCalculated = NavMesh.CalculatePath(transform.position, targetPosition, NavMesh.AllAreas, path);
        Collider[] nearColliders = Physics.OverlapSphere(targetPosition, agent.radius);
        for (int i = 0; i < nearColliders.Length; i++)
        {
            if (nearColliders[i].TryGetComponent<NavMeshAgent>(out NavMeshAgent agent)) return false;
        }
        if (!pathCalculated) return false;
        if (path.status != NavMeshPathStatus.PathComplete) return false;
        if (GetPathLength(path) < walkingDistanceRange.x) return false;

        return true;
    }

    private float GetPathLength(NavMeshPath path)
    {
        float pathLength = 0;
        for (int i = 1; i < path.corners.Length; i++)
        {
            pathLength += Vector3.Distance(path.corners[i], path.corners[i - 1]);
        }
        return pathLength;
    }

    public void SetInteractionIndicator(bool status)
    {
        _interactionIndicator.active = status;
    }


    private void OnDisable()
    {
        if (agent != null && agent.isOnNavMesh) agent.isStopped = true;
    }


}
