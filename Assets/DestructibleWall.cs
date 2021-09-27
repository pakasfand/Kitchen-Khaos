using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructibleWall : MonoBehaviour
{
    [SerializeField] LayerMask cinematicsLayer;

    private void OnCollisionEnter(Collision other)
    {
        if (ExtensionMethods.LayerMaskExtensions.IsInLayerMask(cinematicsLayer, other.gameObject))
        {
            other.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }
    }
}
