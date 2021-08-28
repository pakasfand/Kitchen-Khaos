using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    [SerializeField] private float _speedBoost;
    [SerializeField] private float _powerUpLifeTime;

    public static Action<float, float> OnPowerUpConsumed;

    public void ConsumePowerUp()
    {
        OnPowerUpConsumed?.Invoke(_powerUpLifeTime, _speedBoost);
        Destroy(gameObject);
    }
}
