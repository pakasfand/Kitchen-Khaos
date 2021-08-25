using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private float _detectionRadius;
    [SerializeField] private LayerMask _interactionLayers;
    [SerializeField] private Animator _animator;

    private List<DishTypes> _dishesCollected;
    private bool _isInteracting;

    public static Action OnPlayerStartedCleaning;
    public static Action OnPlayerStoppedCleaning;

    public bool IsInteracting => _isInteracting;

    public List<DishTypes> DishesCollected => _dishesCollected;

    private void Awake()
    {
        _dishesCollected = new List<DishTypes>();
    }

    public void OnInteract(InputAction.CallbackContext value)
    {
        if (value.started)
        {
            var hitColliers = Physics.OverlapSphere(transform.position,
                                _detectionRadius,
                                _interactionLayers);

            for (int i = 0; i < hitColliers.Length; i++)
            {
                if (TryToCleanDishes(hitColliers[i])) { return; }
                if (TryToPickUpDish(hitColliers[i])) { return; }
            }
        }

        if (_isInteracting && value.canceled)
        {
            _isInteracting = false;
            OnPlayerStoppedCleaning?.Invoke();
        }
    }

    private bool TryToCleanDishes(Collider collider)
    {
        if (_dishesCollected.Count == 0) { return false; }

        var sink = collider.GetComponent<Sink>();

        if (sink)
        {
            StartCleaningDishes();
            return true;
        }

        return false;
    }

    private void StartCleaningDishes()
    {
        // Play clean dishes anim
        _isInteracting = true;
        OnPlayerStartedCleaning?.Invoke();
    }

    private bool TryToPickUpDish(Collider collider)
    {
        var enemyAi = collider.GetComponent<AIBehaviour>();

        if (enemyAi)
        {
            PickUpDish(enemyAi);
            return true;
        }

        return false;
    }

    private void PickUpDish(AIBehaviour enemyAi)
    {
        // Play pick up anim
        enemyAi.StopAllCoroutines();
        _dishesCollected.Add(enemyAi.DishType);
        Destroy(enemyAi.gameObject);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _detectionRadius);
    }
}
