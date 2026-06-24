using UnityEngine;

public enum PlayerInputMode
{
    Keyboard,
    Mocap
}

public class InputProviderSwitcher : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AntarcticPlayerController playerController;
    [SerializeField] private PlayerInputProvider keyboardProvider;
    [SerializeField] private PlayerInputProvider mocapProvider;

    [Header("Keys")]
    [SerializeField] private KeyCode useKeyboardKey = KeyCode.K;
    [SerializeField] private KeyCode useMocapKey = KeyCode.M;

    [Header("Start Mode")]
    [SerializeField] private PlayerInputMode startMode = PlayerInputMode.Keyboard;

    [Header("Debug")]
    [SerializeField] private bool showDebugLog = true;

    public PlayerInputMode CurrentMode { get; private set; }
    public string CurrentModeName => CurrentMode.ToString().ToUpper();

    private void Awake()
    {
        if (playerController == null)
            playerController = GetComponent<AntarcticPlayerController>();

        switch (startMode)
        {
            case PlayerInputMode.Keyboard:
                SetKeyboardMode();
                break;

            case PlayerInputMode.Mocap:
                SetMocapMode();
                break;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(useKeyboardKey))
            SetKeyboardMode();

        if (Input.GetKeyDown(useMocapKey))
            SetMocapMode();
    }

    public void SetKeyboardMode()
    {
        if (keyboardProvider == null)
        {
            Debug.LogError("[InputProviderSwitcher] Keyboard Provider가 연결되지 않았습니다.", this);
            return;
        }

        SetProvider(PlayerInputMode.Keyboard, keyboardProvider);
    }

    public void SetMocapMode()
    {
        if (mocapProvider == null)
        {
            Debug.LogError("[InputProviderSwitcher] Mocap Provider가 연결되지 않았습니다.", this);
            return;
        }

        SetProvider(PlayerInputMode.Mocap, mocapProvider);
    }

    private void SetProvider(PlayerInputMode mode, PlayerInputProvider provider)
    {
        if (playerController == null)
        {
            Debug.LogError("[InputProviderSwitcher] Player Controller가 연결되지 않았습니다.", this);
            return;
        }

        playerController.SetInputProvider(provider);
        CurrentMode = mode;

        if (showDebugLog)
            Debug.Log($"[InputProviderSwitcher] Input Mode: {CurrentModeName}");
    }
}