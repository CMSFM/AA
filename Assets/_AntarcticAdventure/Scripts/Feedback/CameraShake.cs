using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [Header("Default Shake")]
    [SerializeField] private float defaultDuration = 0.25f;
    [SerializeField] private float defaultMagnitude = 0.18f;

    public Vector3 CurrentOffset { get; private set; }

    private float remainingTime;
    private float duration;
    private float magnitude;

    private void Update()
    {
        if (remainingTime <= 0f)
        {
            CurrentOffset = Vector3.zero;
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
    }

    public void Play()
    {
        Play(defaultDuration, defaultMagnitude);
    }

    public void Play(float shakeDuration, float shakeMagnitude)
    {
        duration = Mathf.Max(0.001f, shakeDuration);
        remainingTime = duration;
        magnitude = shakeMagnitude;
    }
}