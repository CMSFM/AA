using UnityEngine;

public class AntarcticPlayerAnimationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AntarcticPlayerController playerController;
    [SerializeField] private Animator animator;

    [Header("Animator Parameters")]
    [SerializeField] private string horizontalSpeedParameter = "Horizontal Speed";
    [SerializeField] private string verticalSpeedParameter = "Vertical Speed";
    [SerializeField] private string groundedParameter = "Grounded";
    [SerializeField] private string slidingParameter = "Sliding";

    [Header("Run Animation")]
    [SerializeField] private float playingRunSpeedValue = 1f;
    [SerializeField] private float stoppedRunSpeedValue = 0f;

    [Header("Smoothing")]
    [SerializeField] private float horizontalSpeedSmooth = 12f;
    [SerializeField] private float verticalSpeedScale = 1f;

    private int horizontalSpeedHash;
    private int verticalSpeedHash;
    private int groundedHash;
    private int slidingHash;

    private float currentHorizontalSpeedValue;

    private void Awake()
    {
        if (playerController == null)
            playerController = GetComponentInParent<AntarcticPlayerController>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        horizontalSpeedHash = Animator.StringToHash(horizontalSpeedParameter);
        verticalSpeedHash = Animator.StringToHash(verticalSpeedParameter);
        groundedHash = Animator.StringToHash(groundedParameter);
        slidingHash = Animator.StringToHash(slidingParameter);
    }

    private void LateUpdate()
    {
        if (animator == null || playerController == null)
            return;

        bool isPlaying =
            AntarcticGameManager.Instance != null &&
            AntarcticGameManager.Instance.IsPlaying;

        bool isGrounded = isPlaying
            ? playerController.IsGrounded
            : true;

        bool isSliding = isPlaying && playerController.IsSliding;

        float targetHorizontalSpeed = isPlaying
            ? playingRunSpeedValue
            : stoppedRunSpeedValue;

        float lerpFactor = 1f - Mathf.Exp(-horizontalSpeedSmooth * Time.deltaTime);

        currentHorizontalSpeedValue = Mathf.Lerp(
            currentHorizontalSpeedValue,
            targetHorizontalSpeed,
            lerpFactor
        );

        float verticalSpeed = isPlaying
            ? playerController.CurrentVerticalVelocity * verticalSpeedScale
            : 0f;

        if (!isPlaying)
        {
            currentHorizontalSpeedValue = 0f;
            verticalSpeed = 0f;
        }

        animator.SetFloat(horizontalSpeedHash, currentHorizontalSpeedValue);
        animator.SetFloat(verticalSpeedHash, verticalSpeed);
        animator.SetBool(groundedHash, isGrounded);
        animator.SetBool(slidingHash, isSliding);
    }
}