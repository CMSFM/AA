using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [Header("Default Shake")]
    [SerializeField] private float defaultDuration = 0.25f;
    [SerializeField] private float defaultMagnitude = 0.18f;

    [Header("Direct Transform Shake")]
    [SerializeField] private bool shakeTransformDirectly = true;

    public Vector3 CurrentOffset { get; private set; }

    private Vector3 originalLocalPosition;

    private float remainingTime;
    private float duration;
    private float magnitude;

    private void Awake()
    {
        originalLocalPosition = transform.localPosition;
    }

    private void OnEnable()
    {
        originalLocalPosition = transform.localPosition;
        CurrentOffset = Vector3.zero;
    }

    private void LateUpdate()
    {
        if (remainingTime <= 0f)
        {
            CurrentOffset = Vector3.zero;

            if (shakeTransformDirectly)
                transform.localPosition = originalLocalPosition;

            return;
        }

        remainingTime -= Time.unscaledDeltaTime;

        float t = remainingTime / Mathf.Max(0.001f, duration);
        float currentMagnitude = magnitude * t;

        CurrentOffset = new Vector3(
            Random.Range(-1f, 1f) * currentMagnitude,
            Random.Range(-1f, 1f) * currentMagnitude,
            0f
        );

        if (shakeTransformDirectly)
            transform.localPosition = originalLocalPosition + CurrentOffset;
    }

    public void Play()
    {
        Play(defaultDuration, defaultMagnitude);
    }

    public void Play(float shakeDuration, float shakeMagnitude)
    {
        if (shakeTransformDirectly)
        {
            originalLocalPosition = transform.localPosition - CurrentOffset;
        }

        duration = Mathf.Max(0.001f, shakeDuration);
        remainingTime = duration;
        magnitude = shakeMagnitude;
    }

    public void Stop()
    {
        remainingTime = 0f;
        CurrentOffset = Vector3.zero;

        if (shakeTransformDirectly)
            transform.localPosition = originalLocalPosition;
    }
}