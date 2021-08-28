
using UnityEngine;

public class InteractionIndicator : MonoBehaviour
{

    MeshRenderer meshRenderer;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

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
