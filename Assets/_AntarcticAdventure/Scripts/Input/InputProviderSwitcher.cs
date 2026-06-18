using UnityEngine;

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
    [SerializeField] private bool startWithMocap = true;

    private void Awake()
    {
        if (playerController == null)
            playerController = GetComponent<AntarcticPlayerController>();

        if (startWithMocap && mocapProvider != null)
        {
            SetProvider(mocapProvider, "Mocap");
        }
        else if (keyboardProvider != null)
        {
            SetProvider(keyboardProvider, "Keyboard");
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(useKeyboardKey) && keyboardProvider != null)
        {
            SetProvider(keyboardProvider, "Keyboard");
        }

        if (Input.GetKeyDown(useMocapKey) && mocapProvider != null)
        {
            SetProvider(mocapProvider, "Mocap");
        }
    }

    private void SetProvider(PlayerInputProvider provider, string label)
    {
        if (playerController == null || provider == null)
            return;

        playerController.SetInputProvider(provider);

        Debug.Log($"[InputProviderSwitcher] Input Provider changed: {label}");
    }
}