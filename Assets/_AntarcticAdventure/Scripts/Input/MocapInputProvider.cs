using UnityEngine;

public class MocapInputProvider : PlayerInputProvider
{
    [Header("Debug Values")]
    [Range(-1f, 1f)]
    [SerializeField] private float debugHorizontal;

    [SerializeField] private bool debugJump;
    [SerializeField] private bool debugSlide;

    private float mocapHorizontal;
    private bool mocapJumpPressed;
    private bool mocapSlidePressed;

    public override PlayerInputState ReadInput()
    {
        PlayerInputState state = new PlayerInputState
        {
            Horizontal = Mathf.Clamp(mocapHorizontal + debugHorizontal, -1f, 1f),
            JumpPressed = mocapJumpPressed || debugJump,
            SlidePressed = mocapSlidePressed || debugSlide
        };

        mocapJumpPressed = false;
        mocapSlidePressed = false;
        debugJump = false;
        debugSlide = false;

        return state;
    }

    public void SetMocapInput(float horizontal, bool jumpPressed, bool slidePressed)
    {
        mocapHorizontal = Mathf.Clamp(horizontal, -1f, 1f);

        if (jumpPressed)
            mocapJumpPressed = true;

        if (slidePressed)
            mocapSlidePressed = true;
    }
}