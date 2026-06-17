using UnityEngine;

public abstract class PlayerInputProvider : MonoBehaviour
{
    public abstract PlayerInputState ReadInput();
}