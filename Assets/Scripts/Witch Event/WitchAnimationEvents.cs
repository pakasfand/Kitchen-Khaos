using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WitchAnimationEvents : MonoBehaviour
{

    [SerializeField] Witch witch;

    public void ShowFireball()
    {
        witch.ShowFireball();
    }

    public void ThrowFireball()
    {
        witch.ThrowFireball();
    }
}
