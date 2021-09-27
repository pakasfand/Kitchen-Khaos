using System;

using UnityEngine;
using UnityEngine.Playables;

public class WitchEvent : MonoBehaviour
{

    [SerializeField] RouteFollower witch;
    [SerializeField] Animation spellCastAnim;
    PlayableDirector playableDirector;
    [SerializeField] GameObject fireball;


    private void Awake()
    {
        playableDirector = GetComponent<PlayableDirector>();
    }

    private void OnEnable()
    {
        playableDirector.played += OnSequencePlayed;
        witch.OnAllRoutesCompleted += PlaySpellCast;
        playableDirector.stopped += OnSequenceStopped;
    }

    private void OnDisable()
    {
        playableDirector.played -= OnSequencePlayed;
        witch.OnAllRoutesCompleted -= PlaySpellCast;
        playableDirector.stopped -= OnSequenceStopped;
    }

    private void OnSequenceStopped(PlayableDirector pb)
    {
        witch.gameObject.SetActive(false);
    }

    private void OnSequencePlayed(PlayableDirector pb)
    {
        witch.gameObject.SetActive(true);
        witch.StartMovement();
    }

    private void PlaySpellCast()
    {
        fireball.SetActive(true);
        spellCastAnim.Play();
    }
}
