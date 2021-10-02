
using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.Playables;

public class WitchEvent : MonoBehaviour
{

    [SerializeField] Witch witch;
    PlayableDirector playableDirector;

    [Serializable]
    public class SerializableEvent : UnityEvent { }

    public SerializableEvent OnEventFinished;


    private void Awake()
    {
        playableDirector = GetComponent<PlayableDirector>();
    }

    private void OnEnable()
    {
        playableDirector.played += OnSequencePlayed;
        playableDirector.stopped += OnSequenceStopped;
    }

    private void OnDisable()
    {
        playableDirector.played -= OnSequencePlayed;
        playableDirector.stopped -= OnSequenceStopped;
    }

    private void OnSequenceStopped(PlayableDirector pb)
    {
        witch.gameObject.SetActive(false);
        OnEventFinished?.Invoke();
    }

    private void OnSequencePlayed(PlayableDirector pb)
    {
        witch.gameObject.SetActive(true);
        StartCoroutine(witch.StartMovement());
    }
}
