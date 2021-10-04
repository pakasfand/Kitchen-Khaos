using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructibleWall : MonoBehaviour
{
    [SerializeField] LayerMask cinematicsLayer;
    [SerializeField] GameObject explosionFX;
    [SerializeField] GameObject wall;

    public static bool isDestroyed = false;

    private void Awake()
    {
        foreach (ParticleSystem ps in transform.GetComponentsInChildren<ParticleSystem>(true))
        {
            ps.Stop();
        }
    }

    private void Start()
    {
        if (isDestroyed)
        {
            DeactivateWall();
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (ExtensionMethods.LayerMaskExtensions.IsInLayerMask(cinematicsLayer, other.gameObject))
        {
            other.gameObject.SetActive(false);
            foreach (ParticleSystem ps in transform.GetComponentsInChildren<ParticleSystem>(true))
            {
                ps.Play();
            }
            DeactivateWall();
            isDestroyed = true;
        }
    }

    private void DeactivateWall()
    {
        GetComponent<Collider>().enabled = false;
        wall.SetActive(false);
    }
}
