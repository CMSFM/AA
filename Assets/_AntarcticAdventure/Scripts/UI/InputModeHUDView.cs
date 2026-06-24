using TMPro;
using UnityEngine;

public class InputModeHUDView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InputProviderSwitcher inputProviderSwitcher;
    [SerializeField] private TMP_Text inputModeText;

    [Header("Text")]
    [SerializeField] private string prefix = "INPUT";

    private bool hasLoggedMissingSwitcher;
    private bool hasLoggedMissingText;

    private void Awake()
    {
        ValidateReferences();
    }

    private void Update()
    {
        if (inputModeText == null)
        {
            LogMissingTextOnce();
            return;
        }

        if (inputProviderSwitcher == null)
        {
            LogMissingSwitcherOnce();
            inputModeText.text = $"{prefix}: NONE";
            return;
        }

        inputModeText.text = $"{prefix}: {inputProviderSwitcher.CurrentModeName}";
    }

    private void ValidateReferences()
    {
        if (inputProviderSwitcher == null)
        {
            LogMissingSwitcherOnce();
        }

        if (inputModeText == null)
        {
            LogMissingTextOnce();
        }
    }

    private void LogMissingSwitcherOnce()
    {
        if (hasLoggedMissingSwitcher)
            return;

        hasLoggedMissingSwitcher = true;

        Debug.LogError(
            "[InputModeHUDView] InputProviderSwitcher가 연결되지 않았습니다. " +
            "GameCanvas의 InputModeHUDView 컴포넌트에서 Player의 InputProviderSwitcher를 직접 연결하세요.",
            this
        );
    }

    private void LogMissingTextOnce()
    {
        if (hasLoggedMissingText)
            return;

        hasLoggedMissingText = true;

        Debug.LogError(
            "[InputModeHUDView] InputModeText가 연결되지 않았습니다. " +
            "GameCanvas의 InputModeHUDView 컴포넌트에서 InputModeText TMP_Text를 직접 연결하세요.",
            this
        );
    }
}