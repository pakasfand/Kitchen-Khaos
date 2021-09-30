
using UnityEngine;
using UnityEngine.Playables;

public class WitchEvent : MonoBehaviour
{

    [SerializeField] Witch witch;
    PlayableDirector playableDirector;


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
    }

    private void OnSequencePlayed(PlayableDirector pb)
    {
        witch.gameObject.SetActive(true);
        StartCoroutine(witch.StartMovement());
    }
}
