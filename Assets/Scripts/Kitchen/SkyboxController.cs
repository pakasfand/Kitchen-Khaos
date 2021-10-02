using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyboxController : MonoBehaviour
{
    [SerializeField] private GameLoop _gameLoop;
    [SerializeField] private Material _skyboxMat;
    private void OnEnable()
    {
        GameLoop.OnShiftOver += OnShiftOver;
        _skyboxMat.SetFloat("_CubemapTransition", 0);
    }

    private void OnDisable()
    {
        GameLoop.OnShiftOver += OnShiftOver;
    }

    private void OnShiftOver(bool status)
    {
        if (status)
        {
            _skyboxMat.SetFloat("_CubemapTransition", ((float) _gameLoop.CurrentShiftIndex / _gameLoop.TotalShifts));
        }
    }
}
