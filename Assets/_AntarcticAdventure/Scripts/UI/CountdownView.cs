using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class CountdownView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text countdownText;

    [Header("Timing")]
    [SerializeField] private float numberInterval = 0.7f;
    [SerializeField] private float goDuration = 0.5f;

    [Header("Text")]
    [SerializeField] private string goText = "GO!";

    private bool hasLoggedMissingText;

    private void Awake()
    {
        if (countdownText == null)
        {
            LogMissingTextOnce();
            return;
        }

        countdownText.gameObject.SetActive(false);
    }

    public IEnumerator PlayCountdown(Action onCountdownStarted = null)
    {
        if (countdownText == null)
        {
            LogMissingTextOnce();
            yield break;
        }

        countdownText.gameObject.SetActive(true);

        countdownText.text = "3";

        // 3이 화면에 뜬 직후 실행.
        // 여기서 모캡 캘리브레이션을 호출하면 된다.
        yield return null;
        onCountdownStarted?.Invoke();

        yield return new WaitForSecondsRealtime(numberInterval);

        countdownText.text = "2";
        yield return new WaitForSecondsRealtime(numberInterval);

        countdownText.text = "1";
        yield return new WaitForSecondsRealtime(numberInterval);

        countdownText.text = goText;
        yield return new WaitForSecondsRealtime(goDuration);

        countdownText.gameObject.SetActive(false);
    }

    private void LogMissingTextOnce()
    {
        if (hasLoggedMissingText)
            return;

        hasLoggedMissingText = true;

        Debug.LogError(
            "[CountdownView] CountdownText가 연결되지 않았습니다. " +
            "GameCanvas의 CountdownView에서 CountdownText TMP_Text를 직접 연결하세요.",
            this
        );
    }
}