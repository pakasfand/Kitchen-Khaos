
using System;
using HighlightPlus;
using UnityEngine;

public class InteractionIndicator : MonoBehaviour
{
    [SerializeField] private HighlightEffect _highlightEffect;

    public bool active
    {
        get
        {
            return _active;
        }

        set
        {
            _active = value;
            _highlightEffect.highlighted = _active;
        }
    }

    private bool _active;
}
