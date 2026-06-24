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

    public bool IsSliding => isSliding;
    public bool IsGrounded => characterController != null && characterController.isGrounded;
    public float CurrentVerticalVelocity => verticalVelocity;
    private CharacterController characterController;
    private float verticalVelocity;

    private bool isSliding;
    private float slideElapsedTime;

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
        if (AntarcticGameManager.Instance != null &&
            AntarcticGameManager.Instance.IsGameOver)
        {
            return;
        }

        PlayerInputState input = inputProvider != null
            ? inputProvider.ReadInput()
            : default;

        UpdateSlide(input);
        Move(input);
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

        // 기존 방식: 버튼을 꾹 누르는 것처럼 계속 이동하는 입력.
        // 모캡 Pelvis가 왼쪽에 머물면 Horizontal이 계속 -1에 가까워지고,
        // 그 결과 캐릭터가 왼쪽으로 계속 이동했다.
        //
        // return input.Horizontal * currentSideMoveSpeed;

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