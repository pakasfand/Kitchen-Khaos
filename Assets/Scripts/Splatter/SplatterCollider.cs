using System;
using UnityEngine;

public class SplatterCollider : MonoBehaviour
{

    public Action<Collider> OnTrigger;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player")) OnTrigger?.Invoke(other);
    }
}
