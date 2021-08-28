using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationEvent : MonoBehaviour
{
    [SerializeField] private Animator _animator;

    public void ResetStumble()
    {
        _animator.SetBool("Stumble", false);
    }
    
    public void ResetPickUpLeft()
    {
        _animator.SetBool("Pick up left", false);
    }

    public void ResetPickUpRight()
    {
        _animator.SetBool("Pick up right", false);
    }

    public void ResetJump()
    {
        _animator.SetBool("Jump", false);
    }

    public void ResetEat()
    {
        _animator.SetBool("Eat", false);
    }
}
