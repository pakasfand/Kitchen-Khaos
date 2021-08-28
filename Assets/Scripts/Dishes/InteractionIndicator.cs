
using System;
using UnityEngine;

public class InteractionIndicator : MonoBehaviour
{
    [SerializeField] private MeshRenderer meshRenderer;

    public bool active
    {
        get
        {
            return _active;
        }

        set
        {
            _active = value;
            meshRenderer.enabled = _active;
        }
    }

    private bool _active;
}
