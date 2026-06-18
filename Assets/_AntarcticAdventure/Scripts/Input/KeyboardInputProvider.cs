using UnityEngine;

public class KeyboardInputProvider : PlayerInputProvider
{
    [Header("Keyboard")]
    [SerializeField] private KeyCode leftKey = KeyCode.A;
    [SerializeField] private KeyCode rightKey = KeyCode.D;
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode slideKey = KeyCode.LeftControl;

    public override PlayerInputState ReadInput()
    {
        float horizontal = 0f;

        if (Input.GetKey(leftKey) || Input.GetKey(KeyCode.LeftArrow))
            horizontal -= 1f;

        if (Input.GetKey(rightKey) || Input.GetKey(KeyCode.RightArrow))
            horizontal += 1f;

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

        return new PlayerInputState
        {
            Horizontal = horizontal,
            JumpPressed = jumpPressed,
            SlidePressed = slidePressed,
            SlideHeld = slideHeld
        };
    }
}