using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChefWinAnimation : MonoBehaviour
{
    [SerializeField] Rigidbody rigidBody;
    [SerializeField] float speed;
    [SerializeField] AnimationClip run;
    [SerializeField] AnimationClip flip;

    float timer;

    private void Start()
    {
        rigidBody.velocity = -Vector3.forward * speed;
    }

    public void Stop()
    {
        rigidBody.velocity = Vector3.zero;
    }



}
