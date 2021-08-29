using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KitchenAudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource _musicAudioPlayer;
    [SerializeField] private AudioSource _sfxAudioPlayer;
    [SerializeField] private AudioSource _dishAudioPlayer;

    [Header("Music Clips")]
    [SerializeField] private AudioClip _gameMusic;
    [SerializeField] private AudioClip _endMusic;

    [Header("Sfx")]
    [SerializeField] private AudioClip _dishDropped;
    [SerializeField] private AudioClip _chefHurt;
    [SerializeField] private AudioClip _sink;
    [SerializeField] private AudioClip _splash;
    [SerializeField] private AudioClip _blast;
    [SerializeField] private AudioClip _eat;

    [Header("Plates")]
    [SerializeField] private List<AudioClip> _plateSounds;

    private void OnEnable()
    {
        PlayerInteraction.OnPlayerStumble += OnPlayerStumble;
        PlayerInteraction.OnDishPickedUp += OnDishPickedUp;
        PlayerInteraction.OnChefIgnited += OnChefIgnited;
        PlayerInteraction.OnPlayerStartedCleaning += OnPlayerStartedCleaning;
        PlayerInteraction.OnPlayerStoppedCleaning += OnPlayerStoppedCleaning;
        PowerUp.OnPowerUpConsumed += OnPowerUpConsumed;
        Splatter.OnSplatterCreated += OnSplatterCreated;
        Sink.OnDishesCleaned += OnDishesCleaned;
        Pot.OnPotExplodes += OnPotExplodes;
        GameLoop.OnShiftOver += OnShiftOver;
    }

    private void OnDisable()
    {
        PlayerInteraction.OnPlayerStumble -= OnPlayerStumble;
        PlayerInteraction.OnDishPickedUp -= OnDishPickedUp;
        PlayerInteraction.OnChefIgnited -= OnChefIgnited;
        PlayerInteraction.OnPlayerStartedCleaning -= OnPlayerStartedCleaning;
        PlayerInteraction.OnPlayerStoppedCleaning -= OnPlayerStoppedCleaning;
        PowerUp.OnPowerUpConsumed -= OnPowerUpConsumed;
        Splatter.OnSplatterCreated -= OnSplatterCreated;
        Sink.OnDishesCleaned -= OnDishesCleaned;
        Pot.OnPotExplodes -= OnPotExplodes;
        GameLoop.OnShiftOver -= OnShiftOver;
    }

    private void OnShiftOver()
    {
        _musicAudioPlayer.clip = _endMusic;
        _musicAudioPlayer.Play();
    }

    private void OnPowerUpConsumed(float arg1, float arg2)
    {
        _sfxAudioPlayer.PlayOneShot(_eat);
    }

    private void OnPotExplodes()
    {
        _sfxAudioPlayer.PlayOneShot(_blast);
    }

    private void OnChefIgnited()
    {
        _sfxAudioPlayer.PlayOneShot(_chefHurt);
    }

    private void OnDishPickedUp()
    {
        int rand = UnityEngine.Random.Range(0, _plateSounds.Count);

        _dishAudioPlayer.PlayOneShot(_plateSounds[rand]);
    }

    private void OnPlayerStumble()
    {
        _dishAudioPlayer.PlayOneShot(_dishDropped);
    }

    private void OnPlayerStartedCleaning(List<DishType> obj)
    {
        _dishAudioPlayer.clip = _sink;
        _dishAudioPlayer.Play();
    }

    private void OnPlayerStoppedCleaning()
    {
        if (_dishAudioPlayer.clip == _sink)
        {
            _dishAudioPlayer.clip = null;
            _dishAudioPlayer.Stop();
        }
    }

    private void OnDishesCleaned()
    {
        if (_dishAudioPlayer.clip == _sink)
        {
            _dishAudioPlayer.clip = null;
            _dishAudioPlayer.Stop();
        }
    }

    private void OnSplatterCreated()
    {
        _sfxAudioPlayer.PlayOneShot(_splash);
    }
}
