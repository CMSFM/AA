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
    [SerializeField] private MocapPelvisInputProvider mocapProvider;

    [Header("Keys")]
    [SerializeField] private KeyCode useKeyboardKey = KeyCode.K;
    [SerializeField] private KeyCode useMocapKey = KeyCode.M;

    [Header("Start Mode")]
    [SerializeField] private PlayerInputMode startMode = PlayerInputMode.Keyboard;

    [Header("Debug")]
    [SerializeField] private bool showDebugLog;

    public PlayerInputMode CurrentMode { get; private set; }
    public string CurrentModeName => CurrentMode.ToString().ToUpper();

    private PlayerInputProvider currentProvider;

    private bool hasLoggedMissingPlayerController;
    private bool hasLoggedMissingKeyboardProvider;
    private bool hasLoggedMissingMocapProvider;

    private void Awake()
    {
        ValidateReferences();

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
            LogMissingKeyboardProviderOnce();
            return;
        }

        SetProvider(PlayerInputMode.Keyboard, keyboardProvider);
    }

    public void SetMocapMode()
    {
        if (mocapProvider == null)
        {
            LogMissingMocapProviderOnce();
            return;
        }

        SetProvider(PlayerInputMode.Mocap, mocapProvider);
    }

    public void CalibrateCurrentInputIfMocap()
    {
        if (CurrentMode != PlayerInputMode.Mocap)
            return;

        if (mocapProvider == null)
        {
            LogMissingMocapProviderOnce();
            return;
        }

        mocapProvider.CalibrateNow();

        if (showDebugLog)
            Debug.Log("[InputProviderSwitcher] Mocap input calibrated on start.", this);
    }

    private void SetProvider(PlayerInputMode mode, PlayerInputProvider provider)
    {
        if (playerController == null)
        {
            LogMissingPlayerControllerOnce();
            return;
        }

        if (provider == null)
            return;

        currentProvider = provider;
        CurrentMode = mode;

        playerController.SetInputProvider(currentProvider);

        if (showDebugLog)
            Debug.Log($"[InputProviderSwitcher] Input Mode: {CurrentModeName}", this);
    }

    private void ValidateReferences()
    {
        if (playerController == null)
            LogMissingPlayerControllerOnce();

        if (keyboardProvider == null)
            LogMissingKeyboardProviderOnce();

        if (mocapProvider == null)
            LogMissingMocapProviderOnce();
    }

    private void LogMissingPlayerControllerOnce()
    {
        if (hasLoggedMissingPlayerController)
            return;

        hasLoggedMissingPlayerController = true;

        Debug.LogError(
            "[InputProviderSwitcher] Player Controller가 연결되지 않았습니다. " +
            "Player의 InputProviderSwitcher에서 AntarcticPlayerController를 직접 연결하세요.",
            this
        );
    }

    private void LogMissingKeyboardProviderOnce()
    {
        if (hasLoggedMissingKeyboardProvider)
            return;

        hasLoggedMissingKeyboardProvider = true;

        Debug.LogError(
            "[InputProviderSwitcher] Keyboard Provider가 연결되지 않았습니다. " +
            "Player의 InputProviderSwitcher에서 KeyboardInputProvider를 직접 연결하세요.",
            this
        );
    }

    private void LogMissingMocapProviderOnce()
    {
        if (hasLoggedMissingMocapProvider)
            return;

        hasLoggedMissingMocapProvider = true;

        Debug.LogError(
            "[InputProviderSwitcher] Mocap Provider가 연결되지 않았습니다. " +
            "Player의 InputProviderSwitcher에서 MocapPelvisInputProvider를 직접 연결하세요.",
            this
        );
    }
}