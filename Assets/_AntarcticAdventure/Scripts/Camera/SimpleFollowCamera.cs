using UnityEngine;

public class SimpleFollowCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 4f, -7f);
    [SerializeField] private float followSmooth = 8f;

    [Header("Feedback")]
    [SerializeField] private CameraShake cameraShake;

    private void Awake()
    {
        if (cameraShake == null)
            cameraShake = GetComponent<CameraShake>();
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 targetPosition = target.position + offset;

        Vector3 shakeOffset = cameraShake != null
            ? cameraShake.CurrentOffset
            : Vector3.zero;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition + shakeOffset,
            followSmooth * Time.unscaledDeltaTime
        );

        transform.LookAt(target.position + Vector3.up * 1f);
    }
}