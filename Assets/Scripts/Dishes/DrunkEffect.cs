using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using Random = UnityEngine.Random;

public class DrunkEffect : MonoBehaviour
{

    [SerializeField] PostProcessVolume volume;
    [SerializeField] Vector2 scaleRange;

    LensDistortion lensDistortion;
    DepthOfField depthOfField;

    [SerializeField] float startTime;
    [SerializeField] float blendTimeBetweenStates;
    [SerializeField] float duration;
    [SerializeField] AnimationCurve focus;
    [SerializeField] private AudioSource _musicAudioSource;
    [SerializeField] private AnimationCurve _musicFocus;
    
    struct BufferFloat
    {
        public float newValue;
        public float oldValue;

        public void SetNewValue(float value)
        {
            oldValue = newValue;
            newValue = value;
        }
    }


    float focusTimer;


    BufferFloat centerX;
    BufferFloat centerY;
    BufferFloat scale;

    bool isPlaying = false;

    BeerParticles beerParticles;

    private void Awake()
    {
        beerParticles = GetComponentInChildren<BeerParticles>();
        volume.sharedProfile.TryGetSettings(out lensDistortion);
        volume.sharedProfile.TryGetSettings(out depthOfField);
    }


    private void Update()
    {
        if (isPlaying)
        {
            focusTimer += Time.deltaTime;
            depthOfField.focusDistance.value = focus.Evaluate(focusTimer);
            _musicAudioSource.pitch = _musicFocus.Evaluate(focusTimer);
        }
        else
        {
            focusTimer = 0;
            _musicAudioSource.pitch = 1;
        }
    }

    public void Play()
    {
        if (!isPlaying)
        {
            isPlaying = true;
            StartCoroutine(PlayEffect());
        }
    }

    private IEnumerator PlayEffect()
    {
        float timer = 0;

        PickNewValues();
        LerpLens(1);

        yield return new WaitWhile(() =>
        {
            timer += Time.deltaTime;
            volume.weight = Mathf.Lerp(0, 1, timer / startTime);
            return timer <= startTime;
        });

        volume.weight = 1;
        PickNewValues();

        timer = 0;
        float t = 0;

        yield return new WaitWhile(() =>
        {
            timer += Time.deltaTime;
            t += Time.deltaTime / blendTimeBetweenStates;

            if (t >= 1)
            {
                PickNewValues();
                t = 0;
            }

            LerpLens(t);
            return timer <= duration;
        });

        timer = 0;
        t = 0;

        yield return new WaitWhile(() =>
        {
            t += Time.deltaTime / blendTimeBetweenStates;
            volume.weight = Mathf.Lerp(1f, 0f, t);

            return t <= 1;

        });

        timer = 0;
        t = 0;

        volume.weight = 0;
        isPlaying = false;
    }

    private void LerpLens(float t)
    {
        lensDistortion.centerX.value = Mathf.Lerp(centerX.oldValue, centerX.newValue, t);
        lensDistortion.centerY.value = Mathf.Lerp(centerY.oldValue, centerY.newValue, t);
        lensDistortion.scale.value = Mathf.Lerp(scale.oldValue, scale.newValue, t);
    }

    private void PickNewValues()
    {
        centerX.SetNewValue(centerX.oldValue == 0 ? 1 : centerX.oldValue * -1);
        centerY.SetNewValue(Mathf.Sign(Random.Range(-1f, 1f)));
        scale.SetNewValue(Random.Range(scaleRange.x, scaleRange.y));
    }
}