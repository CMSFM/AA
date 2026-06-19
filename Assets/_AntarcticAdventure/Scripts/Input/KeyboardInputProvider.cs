using UnityEngine;

public class KeyboardInputProvider : PlayerInputProvider
{
    [Header("Virtual Pelvis Horizontal")]
    [SerializeField] private float virtualPelvisMoveSpeed = 0.7f;
    [SerializeField] private float virtualPelvisLimit = 0.35f;
    [SerializeField] private float targetXAtRange = 3f;
    [SerializeField] private KeyCode recenterKey = KeyCode.C;

    [Header("Keys")]
    [SerializeField] private KeyCode leftKey = KeyCode.A;
    [SerializeField] private KeyCode rightKey = KeyCode.D;
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode slideKey = KeyCode.LeftControl;

    [Header("Old Button-like Horizontal Mode - Kept For Reference")]
    [SerializeField] private bool useOldButtonLikeHorizontalMode;

    private float virtualPelvisX;

    public override PlayerInputState ReadInput()
    {
        if (Input.GetKeyDown(recenterKey))
        {
            virtualPelvisX = 0f;
        }

        UpdateVirtualPelvisX();

        bool jumpPressed =
            Input.GetKeyDown(jumpKey) ||
            Input.GetKeyDown(KeyCode.UpArrow);

        bool slidePressed =
            Input.GetKeyDown(slideKey) ||
            Input.GetKeyDown(KeyCode.DownArrow) ||
            Input.GetKeyDown(KeyCode.S);

        bool slideHeld =
            Input.GetKey(slideKey) ||
            Input.GetKey(KeyCode.DownArrow) ||
            Input.GetKey(KeyCode.S);

        if (useOldButtonLikeHorizontalMode)
        {
            float horizontal = ReadOldButtonLikeHorizontal();

            return new PlayerInputState
            {
                Horizontal = horizontal,
                HasTargetX = false,
                TargetX = 0f,
                JumpPressed = jumpPressed,
                SlidePressed = slidePressed,
                SlideHeld = slideHeld
            };
        }

        float targetX = ReadTargetXFromVirtualPelvis();

        return new PlayerInputState
        {
            Horizontal = 0f,
            HasTargetX = true,
            TargetX = targetX,
            JumpPressed = jumpPressed,
            SlidePressed = slidePressed,
            SlideHeld = slideHeld
        };
    }

    private void UpdateVirtualPelvisX()
    {
        float direction = 0f;

        if (Input.GetKey(leftKey) || Input.GetKey(KeyCode.LeftArrow))
            direction -= 1f;

        if (Input.GetKey(rightKey) || Input.GetKey(KeyCode.RightArrow))
            direction += 1f;

        virtualPelvisX += direction * virtualPelvisMoveSpeed * Time.deltaTime;
        virtualPelvisX = Mathf.Clamp(
            virtualPelvisX,
            -virtualPelvisLimit,
            virtualPelvisLimit
        );
    }

    private float ReadTargetXFromVirtualPelvis()
    {
        float normalized = virtualPelvisX / Mathf.Max(0.001f, virtualPelvisLimit);
        normalized = Mathf.Clamp(normalized, -1f, 1f);

        return normalized * targetXAtRange;
    }

    private float ReadOldButtonLikeHorizontal()
    {
        float horizontal = 0f;

        if (Input.GetKey(leftKey) || Input.GetKey(KeyCode.LeftArrow))
            horizontal -= 1f;

        if (Input.GetKey(rightKey) || Input.GetKey(KeyCode.RightArrow))
            horizontal += 1f;

        // 기존 방식:
        // A를 누르고 있는 동안 Horizontal = -1,
        // D를 누르고 있는 동안 Horizontal = 1.
        // PlayerController는 이 값을 속도로 해석해서 계속 이동했다.

        return horizontal;
    }
}