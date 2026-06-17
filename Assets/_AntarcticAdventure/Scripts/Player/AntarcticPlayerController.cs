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

    [Header("Jump")]
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -25f;

    [Header("Slide")]
    [SerializeField] private float slideDuration = 0.75f;
    [SerializeField] private float normalHeight = 2f;
    [SerializeField] private float slideHeight = 1f;
    [SerializeField] private Vector3 normalCenter = new Vector3(0f, 0f, 0f);
    [SerializeField] private Vector3 slideCenter = new Vector3(0f, -0.5f, 0f);

    public bool IsSliding => isSliding;
    public bool IsGrounded => characterController != null && characterController.isGrounded;

    private CharacterController characterController;
    private float verticalVelocity;

    private bool isSliding;
    private float slideTimer;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

        if (inputProvider == null)
            inputProvider = GetComponent<PlayerInputProvider>();

        ApplyNormalCollider();
        ClampPosition();
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
        if (!isSliding && characterController.isGrounded && input.SlidePressed)
        {
            StartSlide();
        }

        if (!isSliding)
            return;

        slideTimer -= Time.deltaTime;

        if (slideTimer <= 0f)
        {
            EndSlide();
        }
    }

    private void StartSlide()
    {
        isSliding = true;
        slideTimer = slideDuration;
        ApplySlideCollider();
    }

    private void EndSlide()
    {
        isSliding = false;
        ApplyNormalCollider();
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
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

        verticalVelocity += gravity * Time.deltaTime;

        float currentSideMoveSpeed = isSliding
            ? sideMoveSpeed * slideSideMoveMultiplier
            : sideMoveSpeed;

        Vector3 move = Vector3.zero;
        move.x = input.Horizontal * currentSideMoveSpeed;
        move.y = verticalVelocity;
        move.z = 0f;

        characterController.Move(move * Time.deltaTime);

        ClampPosition();
    }

    private void ClampPosition()
    {
        Vector3 position = transform.position;
        position.x = Mathf.Clamp(position.x, -xLimit, xLimit);
        position.z = fixedZ;
        transform.position = position;
    }
}