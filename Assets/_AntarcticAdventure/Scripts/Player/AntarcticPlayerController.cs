using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class AntarcticPlayerController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private PlayerInputProvider inputProvider;

    [Header("Movement")]
    [SerializeField] private float sideMoveSpeed = 7f;
    [SerializeField] private float slideSideMoveMultiplier = 0.5f;
    [SerializeField] private float xLimit = 4f;
    [SerializeField] private float fixedZ = 0f;

    [Header("Target Position Movement")]
    [SerializeField] private float targetXMoveSpeed = 12f;
    [SerializeField] private float targetXDeadZone = 0.01f;

    [Header("Jump")]
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -25f;

    [Header("Slide")]
    [SerializeField] private float minimumSlideDuration = 0.25f;
    [SerializeField] private float normalHeight = 2f;
    [SerializeField] private float slideHeight = 1f;
    [SerializeField] private Vector3 normalCenter = new Vector3(0f, 0f, 0f);
    [SerializeField] private Vector3 slideCenter = new Vector3(0f, -0.5f, 0f);

    [Header("Start Input Guard")]
    [SerializeField] private float actionInputIgnoreTimeOnStart = 0.2f;

    public bool IsSliding => isSliding;
    public bool IsGrounded => characterController != null && characterController.isGrounded;
    public float CurrentVerticalVelocity => verticalVelocity;
public bool IsStartActionGuardActive =>
    wasControllable &&
    Time.unscaledTime < controlStartedTime + actionInputIgnoreTimeOnStart;
    private CharacterController characterController;
    private float verticalVelocity;

    private bool isSliding;
    private float slideElapsedTime;

    private bool wasControllable;
    private float controlStartedTime;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

        if (inputProvider == null)
            inputProvider = GetComponent<PlayerInputProvider>();

        ApplyNormalCollider();
        ClampPosition();
    }

    public void SetInputProvider(PlayerInputProvider provider)
    {
        inputProvider = provider;
    }

    private void Update()
    {
        bool canControl = CanControlPlayer();

        if (!canControl)
        {
            wasControllable = false;
            StopPlayerActionWhileNotPlaying();
            return;
        }

        if (!wasControllable)
        {
            wasControllable = true;
            controlStartedTime = Time.unscaledTime;
            verticalVelocity = 0f;
        }

        PlayerInputState input = inputProvider != null
            ? inputProvider.ReadInput()
            : default;

        input = ApplyStartInputGuard(input);

        UpdateSlide(input);
        Move(input);
    }

    private bool CanControlPlayer()
    {
        return AntarcticGameManager.Instance != null &&
               AntarcticGameManager.Instance.IsPlaying;
    }

    private PlayerInputState ApplyStartInputGuard(PlayerInputState input)
    {
        if (Time.unscaledTime < controlStartedTime + actionInputIgnoreTimeOnStart)
        {
            input.JumpPressed = false;
            input.SlidePressed = false;
            input.SlideHeld = false;
        }

        return input;
    }

    private void StopPlayerActionWhileNotPlaying()
    {
        verticalVelocity = 0f;

        if (isSliding)
        {
            isSliding = false;
            ApplyNormalCollider();
        }

        ClampPosition();
    }

    private void UpdateSlide(PlayerInputState input)
    {
        if (!isSliding)
        {
            if (characterController.isGrounded && (input.SlidePressed || input.SlideHeld))
            {
                StartSlide();
            }

            return;
        }

        slideElapsedTime += Time.deltaTime;

        bool canEndSlide = slideElapsedTime >= minimumSlideDuration;
        bool wantsToStopSliding = !input.SlideHeld;

        if (canEndSlide && wantsToStopSliding)
        {
            EndSlide();
        }
    }

    private void StartSlide()
    {
        isSliding = true;
        slideElapsedTime = 0f;
        ApplySlideCollider();

        if (AntarcticAudioManager.Instance != null)
            AntarcticAudioManager.Instance.PlaySlideStart();
    }

    private void EndSlide()
    {
        isSliding = false;
        ApplyNormalCollider();

        if (AntarcticAudioManager.Instance != null)
            AntarcticAudioManager.Instance.PlaySlideEnd();
    }

    private void ApplyNormalCollider()
    {
        characterController.height = normalHeight;
        characterController.center = normalCenter;
    }

    private void ApplySlideCollider()
    {
        characterController.height = slideHeight;
        characterController.center = slideCenter;
    }

    private void Move(PlayerInputState input)
    {
        if (characterController.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        if (characterController.isGrounded && input.JumpPressed && !isSliding)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

            if (AntarcticAudioManager.Instance != null)
                AntarcticAudioManager.Instance.PlayJump();
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 move = Vector3.zero;

        move.x = CalculateHorizontalMove(input);
        move.y = verticalVelocity;
        move.z = 0f;

        characterController.Move(move * Time.deltaTime);

        ClampPosition();
    }

    private float CalculateHorizontalMove(PlayerInputState input)
    {
        float currentSideMoveSpeed = isSliding
            ? sideMoveSpeed * slideSideMoveMultiplier
            : sideMoveSpeed;

        if (input.HasTargetX)
        {
            float currentX = transform.position.x;
            float targetX = Mathf.Clamp(input.TargetX, -xLimit, xLimit);

            float deltaToTarget = targetX - currentX;

            if (Mathf.Abs(deltaToTarget) <= targetXDeadZone)
                return 0f;

            float nextX = Mathf.MoveTowards(
                currentX,
                targetX,
                targetXMoveSpeed * Time.deltaTime
            );

            return (nextX - currentX) / Time.deltaTime;
        }

        return input.Horizontal * currentSideMoveSpeed;
    }

    private void ClampPosition()
    {
        Vector3 position = transform.position;
        position.x = Mathf.Clamp(position.x, -xLimit, xLimit);
        position.z = fixedZ;
        transform.position = position;
    }
}